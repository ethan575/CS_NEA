using _3D_Engine.Classes.NeuralNet;
using Jitter2;
using Jitter2.LinearMath;
using MathNet.Numerics.LinearAlgebra;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollBody
    {
        public List<Bone> Bones { get; set; }
        public List<Joint> Joints { get; set; }

        public NeuralNetwork Brain { get; private set; } 

        public JVector BodyVelocity { get; private set; }

        public JVector TargetPosition = JVector.Zero;
        public JVector COMPosition { get; private set; } = JVector.Zero;

        public float EnergyUsed = 0;

        public FitnessObjective[] fitnessObjectives = new FitnessObjective[]
        {
            // ragdoll is the parameter in lambda function
            new FitnessObjective(1f, ragdoll => Objectives.EvalProximityToTarget(ragdoll.COMPosition, ragdoll.TargetPosition)),
            new FitnessObjective(0.5f, ragdoll => Objectives.EvalEnergyUsed(ragdoll.EnergyUsed))
        };


        public RagdollBody(List<Bone> bones, List<Joint> joints)
        {
            this.Bones = bones ?? new List<Bone>();
            this.Joints = joints ?? new List<Joint>();
        }

        public void SetBrain(NeuralNetwork brain)
        {
            // each bone has 6 inputs (velocity and rotation)
            // target position
            // current position

            if (brain.LayerDims[0] != (this.Joints.Count * 2) + 6 )
            {
                throw new ArgumentException("Input layer size must match the number of bones times 6 and additional 2.");
            }
            Brain = brain;

            this.ApplyForce(JVector.UnitY*10);
        }

        public JVector ComputeCentreOfMass()
        {
            JVector centerOfMass = JVector.Zero;
            float totalMass = 0f;
            foreach (var bone in Bones)
            {
                float mass = bone.RB.Mass;
                centerOfMass += bone.RB.Position * mass;
                totalMass += mass;
            }
            if (totalMass > 0)
            {
                centerOfMass = centerOfMass * (1 / totalMass);
            }
            return centerOfMass;
        }

        public JVector ComputeVelocity()
        {
            JVector total = JVector.Zero;
            float totalMass = 0;

            foreach(Bone b in this.Bones)
            {
                total += b.RB.Mass * b.RB.Velocity;
                totalMass += b.RB.Mass;
            }

            return total * (1/totalMass);
        }

        /* 
         inputs = [
        each joint angle,
        each joint angular velocity,
        COM velocity,
        direction to target,
        Reference bone faceing direction (e.g., forward vector of the torso)

        ]
         */

        public Matrix<float> GetBrainInputMatrix()
        {
            Matrix<float> inputMatrix = Matrix<float>.Build.Dense(Brain.LayerDims[0], 1);
            int LastWrittenIdx = 0;

            // all joint values 
            for (int i = 0; i < Joints.Count; i+=2) // each joint has 2 inputs
            {
                Joint joint = Joints[i];
                inputMatrix[i, 0] = joint.HindgeJointConstraint.HingeAngle.Angle.Radian / (2*MathF.PI);
                inputMatrix[i + 1, 0] = joint.HindgeJointConstraint.Motor.TargetVelocity / 1200;

                LastWrittenIdx = i + 1;
            }

            // general body info
            // max velocity = 100
          
            inputMatrix[LastWrittenIdx + 1, 0] = this.BodyVelocity.X /100;
            inputMatrix[LastWrittenIdx + 2, 0] = this.BodyVelocity.Y / 100;
            inputMatrix[LastWrittenIdx + 3, 0] = this.BodyVelocity.Z / 100;

            JVector DirToTarget = (TargetPosition - this.ComputeCentreOfMass());
            DirToTarget = JVector.Normalize(DirToTarget);
            inputMatrix[LastWrittenIdx + 4, 0] = DirToTarget.X;
            inputMatrix[LastWrittenIdx + 5, 0] = DirToTarget.Y;
            inputMatrix[LastWrittenIdx + 6, 0] = DirToTarget.Z;

            LastWrittenIdx = LastWrittenIdx + 6;



            return inputMatrix;
        }

        public void BrainNNFF(Matrix<float> input)
        {
            if (Brain == null)
            {
                throw new InvalidOperationException("Brain is not set.");
            }
            
            else if (input.RowCount != Brain.LayerDims[0] || input.ColumnCount != 1)
            {
                throw new ArgumentException("Input matrix dimensions do not match the expected input layer size.");
            }

            Matrix<float> output = Brain.GetNetworkOutput(input);

            Console.Write("");
            // Process the output to apply forces or torques to the bones
            //!! may need to fix the target angle function 
            // iterate through the output and apply forces/torques to the bones
            for (int i = 0; i < Joints.Count; i++)
            {
                var t = Math.Clamp(output[i, 0] * 4000, -1200, 1200);
                Joints[i].HindgeJointConstraint.Motor.TargetVelocity = t;//*10000;
                EnergyUsed += Math.Abs(t / 128);
                Console.WriteLine("t: " + output[i, 0] * 400);
                //Console.WriteLine(Joints[i].HindgeJointConstraint.Motor.TargetVelocity);
            }

        }

        public void AddBone(Bone bone)
        {
            Bones.Add(bone);
        }
        public void AddJoint(Joint joint)
        {
            Joints.Add(joint);
        }

        public void ApplyTorque(Vector3 torque, Bone bone)
        {
            bone.RB.AddForce(new JVector(torque.X, torque.Y, torque.Z) );
        }

        public void ApplyForce(JVector force)
        {
            foreach (Bone b  in Bones)
            {
                b.RB.AddForce(force);
            }
        }

        public void ResetAngularVelocities()
        {
            foreach (var item in Joints)
            {
                item.HindgeJointConstraint.Motor.TargetVelocity = 0;
            }
        }

        public void Update(float dt)
        {
            foreach (var bone in Bones)
            {
                bone.UpdateVisual();

            }
            foreach (var joint in Joints)
            {
                joint.UpdateVisual();

            }

            COMPosition = this.ComputeCentreOfMass();
            this.BodyVelocity = ComputeVelocity(); 

        }

        public void setPostion(JVector position)
        {
            JVector offset = position - this.ComputeCentreOfMass();
            foreach (var bone in Bones)
            {
                bone.RB.Position += offset;
                bone.UpdateVisual();
            }
        }

        public RagdollBody Clone(World world)
        {
            List<Bone> clonedBones = new List<Bone>();
            foreach (var bone in Bones)
            {
                clonedBones.Add((Bone)bone.Clone(world));
            }
            List<Joint> clonedJoints = new List<Joint>();
            foreach (var joint in Joints)
            {
                clonedJoints.Add((Joint)joint.Clone(world));
            }
            RagdollBody clonedRagdoll = new RagdollBody(clonedBones, clonedJoints);
            clonedRagdoll.SetBrain(this.Brain.Copy()); 
            return clonedRagdoll;
        }

        public void EndOfGenerationTasks()
        {
            Brain.ExecuteEndOfGenerationTasks();

            var dist = (this.TargetPosition - this.ComputeCentreOfMass()).Length();
            if (Objectives.MaxDistanceFromPopulation < dist)
            {
                Objectives.MaxDistanceFromPopulation = dist;
            }
        }

    }
}
