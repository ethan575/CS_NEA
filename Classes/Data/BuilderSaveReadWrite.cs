using _3D_Engine.Classes.Helper;
using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.Ragdoll;
using _3D_Engine.Classes.Scenes;
using Jitter2;
using Jitter2.LinearMath;
using OpenTK.Mathematics;
using System.Drawing;

namespace _3D_Engine.Classes.Data
{
    public static class BuilderSaveReadWrite
    {

        static float[] jointVertex = ModelImporter.LoadModel("models/Raw/joint.stl");
        static float[] boneVertex = ModelImporter.LoadModel("models/Raw/bone.stl");

        public static void BuilderSaveFile(string path, RagdollBody body)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path cannot be empty");

            using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(path)))
            {
                /////// write all the properties of the rigidbody that are necessary for reconstruction
                ///// structure
                /// number of bones
                ///     number of properties in each bone (here say 2)
                ///         bone 1 property 1
                ///         bone 1 property 2 
                ///         
                ///         bone 2 property 1
                ///         bone 2 property 2
                ///         
                ///         etc
                ///         
                /// number of joints
                ///     number of properties in each joint (here 2)
                ///         joint 1 property 1
                ///         joint 1 property 2 
                ///         
                ///         joint 2 property 1
                ///         joint 2 property 2
                ///
                ///         

                bw.Write(body.Bones.Count);
                bw.Write(3); // RB position and orientation

                foreach (Bone bone in body.Bones)
                {
                    bw.Write(bone.BoneID);
                    SerialiseVector3(bw, TypeConvertHelper.FromJVec(bone.RB.Position)); // Pos
                    SerialiseQuaternion(bw, bone.RB.Orientation);

                }

                bw.Write(body.Joints.Count);
                bw.Write(7);

                foreach (Joint joint in body.Joints)
                {
                    bw.Write(joint.JointID);
                    bw.Write(joint.BoneA_ID);
                    bw.Write(joint.BoneB_ID);
                    bw.Write(joint.stiffness);
                    bw.Write(joint.angularDamping);
                    SerialiseVector3(bw, TypeConvertHelper.FromJVec(joint.anchorPoint));
                    SerialiseVector3(bw, TypeConvertHelper.FromJVec(joint.jointAxis));                }
                    


            }
        }

        public static RagdollBody BuilderLoadFile(string path, World world, Shader shader, Scene scene)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path cannot be empty");

            Bone.noOfBones = 0;
            Joint.noOfJoints = 0;

            List<Bone> bones = new List<Bone>();
            List<Joint> joints = new List<Joint>();

            using (var br = new BinaryReader(File.OpenRead(path)))
            {
                int noOfBones = br.ReadInt32(); 
                int noOfBoneProperties = br.ReadInt32();

                for (int i = 0; i < noOfBones; i++)  // each bone here
                {
                    /// read
                    int BoneID = br.ReadInt32();
                    DeserialiseVector3(br, out Vector3 position);
                    DeserialiseQuaternion(br, out JQuaternion orientation);


                    // instantiate
                    WorldObject BoneVisual = new WorldObject("bVisual", shader, boneVertex, scene, new Vector3(0.5f, 0.5f, 0.5f));
                    BoneVisual.setScale(1f, 0.5f, 0.5f);
                    Bone newBone = new Bone(world, BoneVisual, position, "bone");
                    newBone.RB.Position = TypeConvertHelper.ToJVec(position);
                    newBone.RB.Orientation = orientation;
                    newBone.setBoneID(BoneID);
                    newBone.RB.AffectedByGravity = true;

                    newBone.RB.SetMassInertia(1);
                    newBone.RB.Friction = 0.5f;
                    newBone.RB.ApplyImpulse(new JVector(0, 1f, 0));
                    bones.Add(newBone);
                }

                int noOfJoints = br.ReadInt32();
                int noOfJointProperties = br.ReadInt32();

                for (int i = 0; i < noOfJoints ; i++)
                {
                    // read
                    int jointID = br.ReadInt32();
                    int BoneA_ID = br.ReadInt32();
                    int BoneB_ID = br.ReadInt32();
                    float stiffness = br.ReadSingle();
                    float angularDamping = br.ReadSingle();
                    DeserialiseVector3(br, out Vector3 anchorPoint);
                    DeserialiseVector3(br, out Vector3 JointAxis);

                    Bone BoneA = bones.Find(b => b.BoneID == BoneA_ID);
                    Bone BoneB = bones.Find(b => b.BoneID == BoneB_ID);

                    //Console.WriteLine(BoneA);

                    WorldObject JointVisual = new WorldObject("jVisual", shader, jointVertex, scene, new Vector3(0.5f, 0.5f, 0.5f));
                    Joint newJoint = new Joint(world, BoneA, BoneB, JointVisual);

                    joints.Add(newJoint);
                }

            }


            RagdollBody body = new RagdollBody(bones, joints);
            return body;
        }


        private static void SerialiseVector3(BinaryWriter bw, Vector3 vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
            bw.Write(vec.Z);
        }

        private static void DeserialiseVector3(BinaryReader br, out Vector3 vec)
        {
            vec = new Vector3();
            vec.X = br.ReadSingle();
            vec.Y = br.ReadSingle();
            vec.Z = br.ReadSingle();
        }

        private static void SerialiseQuaternion(BinaryWriter bw, JQuaternion quat)
        {
            bw.Write(quat.X);
            bw.Write(quat.Y);
            bw.Write(quat.Z);
            bw.Write(quat.W);
        }

        private static void DeserialiseQuaternion(BinaryReader br, out JQuaternion quat)
        {
            quat = new JQuaternion();
            quat.X = br.ReadSingle();
            quat.Y = br.ReadSingle();
            quat.Z = br.ReadSingle();
            quat.W = br.ReadSingle();
        }

    }
}
