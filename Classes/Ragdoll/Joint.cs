using _3D_Engine.Classes.Objects;
using Jitter2;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using OpenTK.Mathematics;

namespace _3D_Engine.Classes.Ragdoll
{
    public class Joint : RagdollObjects
    {
        public static int noOfJoints = 0; 
        public int JointID { get; private set; }

        public Bone BoneA { get; private set; }
        public int BoneA_ID;

        public Bone BoneB { get; private set; }
        public int BoneB_ID;

        public HingeJoint HindgeJointConstraint { get; set; }
        public float stiffness { get; set; } = 80f;
        public float angularDamping { get; set; } = 5f;

        public Vector3 position { get; set; } = Vector3.Zero;

        public JVector anchorPoint { get; private set; }
        public JVector jointAxis { get; private set; } = JVector.UnitZ;

        public WorldObject HingeAxisVisual { get; set; }

        /// <summary>
        ///joint
        /// </summary>
        /// <param name="boneA">The Root bone.</param>
        /// <param name="boneB">The child bone.</param>
        public Joint(World world, Bone boneA, Bone boneB, WorldObject Visual) // used when bones are present already
        {
            this.JointID = Joint.noOfJoints;
            Joint.noOfJoints++;

            BoneA = boneA;
            BoneB = boneB;
            this.Visual = Visual;
            //Constraint = world.CreateConstraint<BallSocket>(BoneA.RB, BoneB.RB);
            float halfLengthA = boneA.Visual.GetScale().X * 0.5f;
            float halfLengthB = boneB.Visual.GetScale().X * 0.5f;
            // b1.GetWorldPosition().X + (b1.GetWorldScale().X * 8), 7, 10)
            anchorPoint = boneA.RB.Position + new JVector(boneA.Visual.GetScale().X * 4, 0, 0);


            HindgeJointConstraint = new HingeJoint(world, boneA.RB, BoneB.RB, anchorPoint, jointAxis, true);
            HindgeJointConstraint.Motor.MaximumForce = 400f;
            HindgeJointConstraint.Motor.IsEnabled = true;


        }

        public Joint(World world, WorldObject Visual, JVector anchorPoint, WorldObject hingeAxisVisual) // for builder when bones not present yet
        {
            this.JointID = Joint.noOfJoints;
            Joint.noOfJoints++;

            this.Visual = Visual;
            this.Visual.setPosition(FromJVec(anchorPoint));

            this.anchorPoint = anchorPoint;
            this.HingeAxisVisual = hingeAxisVisual;
        }

        public void initialiseConstraints(World world)
        {
            HindgeJointConstraint = new HingeJoint(world, BoneA.RB, BoneB.RB, anchorPoint, jointAxis, true);
            HindgeJointConstraint.Motor.MaximumForce = 200f;
            HindgeJointConstraint.Motor.IsEnabled = true;
        }

        public void SetJointAxis(Vector3 axis)
        {
            jointAxis = ToJVec(axis.Normalized());
            if (HindgeJointConstraint != null)
            {
                HindgeJointConstraint.Remove();
                HindgeJointConstraint = new HingeJoint(BoneA.RB.World, BoneA.RB, BoneB.RB, anchorPoint, jointAxis, true);
                HindgeJointConstraint.Motor.MaximumForce = 200f;
            }
        }

        public void SetJointAnchor(Vector3 anchor)
        {
            anchorPoint = ToJVec(anchor);
            if (HindgeJointConstraint != null)
            {
                HindgeJointConstraint.Remove();
                HindgeJointConstraint = new HingeJoint(BoneA.RB.World, BoneA.RB, BoneB.RB, anchorPoint, jointAxis, true);
                HindgeJointConstraint.Motor.MaximumForce = 200f;
            }
        }

        public void setBoneA(Bone bone)
        {
            this.BoneA = bone;
            this.BoneA_ID = bone.BoneID;
        }

        public void setBoneB(Bone bone)
        {
            this.BoneB = bone;
            this.BoneB_ID = bone.BoneID;
        }


        // create own PD controller for target angles
        // https://stackoverflow.com/questions/72848440/how-to-find-the-relative-difference-between-two-quaternions
        // https://www.codestudy.net/blog/difference-between-two-quaternions/
        // a quaternion stores cos(angle/2) in W
        // BoneA considered root
        /// <param name="TargetAngle">target in radian</param>
        public void TorqueToTargetAngle(float TargetAngle)
        {
            float error = TargetAngle - HindgeJointConstraint.HingeAngle.Angle.Radian;

            //Console.WriteLine(error);
            if (Math.Abs(error) < 1e-7)
            {
                return;
            }

            // PD torque  = stiffness * angle - damping * relativeAngVel
            float correction = ((float)error * stiffness);

            RigidJoint();
            HindgeJointConstraint.Motor.TargetVelocity = correction;

        }

        // allow free rotation alogn hindge
        public void FreeJoint()
        {
            HindgeJointConstraint.Motor.IsEnabled = false;
        }
        public void RigidJoint()
        {
            HindgeJointConstraint.Motor.IsEnabled = true;
        }

        public override void UpdateVisual()
        {
            // Update the visual representation of the bone based on the rigid body's position and orientation
            Visual?.setPosition(position);
            Visual?.Draw();

            if (HingeAxisVisual != null)
            {
                HingeAxisVisual.setPosition(position);
                HingeAxisVisual.Draw();
            }
            //Console.WriteLine($"Bone {name} Rotation: ({rot.X}, {rot.Y}, {rot.Z}, {rot.W})");
            //Console.WriteLine($"Visual {name} Rotation: ({Visual.GetOrientation().X}, {Visual.GetOrientation().Y}, {Visual.GetOrientation().Z}, {Visual.GetOrientation().W})");
        }

        public void destroy()
        {
            Visual.Destroy();
            HingeAxisVisual.Destroy();
            HindgeJointConstraint?.Remove();
            
        }


        static Vector3 FromJVec(JVector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        static JVector ToJVec(Vector3 v)
        {
            return new JVector(v.X, v.Y, v.Z);
        }
        static Quaternion FromJquaternion(JQuaternion q)
        {
            // some random bug reqiures negative
            return new Quaternion(-q.X, q.Y, q.Z, -q.W);
        }
        static JQuaternion ToJQuaternion(Quaternion q)
        {
            return new JQuaternion(-q.X, q.Y, q.Z, -q.W);
        }
    }
}
