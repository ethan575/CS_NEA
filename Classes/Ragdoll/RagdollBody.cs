using _3D_Engine.Classes.NeuralNet;
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

            if (brain.LayerDims[0] != (this.Joints.Count * 6) + 2 )
            {
                throw new ArgumentException("Input layer size must match the number of bones times 6 and additional 2.");
            }
            Brain = brain;
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
            for (int i = 0; i < Bones.Count; i++)
            {
                Joints[i].HindgeJointConstraint.Motor.TargetVelocity = output[i, 0];
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

        }

    }
}
