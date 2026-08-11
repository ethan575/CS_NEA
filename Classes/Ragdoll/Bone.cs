using _3D_Engine.Classes.Objects;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.Ragdoll
{
    public class Bone: RagdollObjects
    {
        public string name { get; set; }
        //public RigidBody RB { get; set; }

        public Bone(World world, WorldObject visual, Vector3 position, string name)   
        {
            this.Visual = visual;
            this.name = name;


            // Create a rigid body for the bone
            RB = world.CreateRigidBody();
            RB.SetMassInertia(1);
            RB.Friction = 0.5f;
            //RB.AddShape(new BoxShape(shapeDims*0.5f));
            List<JVector> verts = new List<JVector>();
            
            foreach (Vector3 v in ModelImporter.LoadOnlyVertices("models/Raw/bone.stl"))
            {
                verts.Add(new JVector(v.X * visual.GetScale().X, v.Y * visual.GetScale().Y, v.Z * visual.GetScale().Z));
            }
            RB.Position = new JVector(position.X, position.Y, position.Z);
            RB.Orientation = ToJQuaternion(visual.GetOrientation());
            RB.AddShape(new PointCloudShape(verts));
            
        }

        public override void UpdateVisual()
        {
            // Update the visual representation of the bone based on the rigid body's position and orientation
            Visual.setPosition(FromJVec(RB.Position));
            Visual.setRotation(FromJquaternion(RB.Orientation));
            Visual.Draw();
            //Console.WriteLine($"Bone {name} Rotation: ({rot.X}, {rot.Y}, {rot.Z}, {rot.W})");
            //Console.WriteLine($"Visual {name} Rotation: ({Visual.GetOrientation().X}, {Visual.GetOrientation().Y}, {Visual.GetOrientation().Z}, {Visual.GetOrientation().W})");
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
