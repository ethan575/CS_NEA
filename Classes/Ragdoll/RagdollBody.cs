using Jitter2.LinearMath;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollBody
    {
        public List<Bone> Bones { get; set; }
        public List<Joint> Joints { get; set; }

        public RagdollBody(List<Bone> bones, List<Joint> joints)
        {
            this.Bones = bones ?? new List<Bone>();
            this.Joints = joints ?? new List<Joint>();
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
