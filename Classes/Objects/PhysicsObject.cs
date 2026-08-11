using _3D_Engine.Classes.Scenes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using MathHelper = OpenTK.Mathematics.MathHelper;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace _3D_Engine.Classes.Objects
{
    public class PhysicsObject : WorldObject
    {
        protected Game game;
        protected Scene scene;
        protected Entity? parent;
        public RigidBody Rbody { get; private set; }

        public Vector3 velocity = Vector3.Zero;
        float mass = 1.0f;
        public bool ObeysGravity { get; private set; } = true;//obeys gravity

        public PhysicsObject(string name, Shader shader, float[] vertices, Scene scene, Game game, RigidBody rb, Vector3 color)
            : base(name, shader, vertices, scene, color)
        {
            this.scene = scene;
            this.game = game;
            Rbody = rb;

            rb.Position = new JVector(position.X, position.Y, position.Z);
            scene.AllPhysicsObjects.Add(this);


        }
        public PhysicsObject(string name, Shader shader, float[] vertices, WorldObject parent, Scene scene, Game game, RigidBody rb, Vector3 color)
            : base(name, shader, vertices, color, parent, scene)
        {
            this.parent = parent;
            this.scene = scene;
            this.game = game;
            Rbody = rb;

            rb.Position = new JVector(position.X, position.Y, position.Z);
            scene.AllPhysicsObjects.Add(this);

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

        public override void setPosition(float x, float y, float z)
        {
            base.setPosition(x, y, z);
            Rbody.Position = ToJVec(position);
        }
        public override void setPosition(Vector3 v)
        {
            base.setPosition(v);
            Rbody.Position = ToJVec(position);
        }
        public override void setRotation(float x, float y, float z)
        {

            rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x),
                MathHelper.DegreesToRadians(y),
                MathHelper.DegreesToRadians(z));
            Rbody.Orientation = new JQuaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            rotation.Normalize(); // normalize to prevent gimbal lock
        }

        public void setObeyGravity(bool obey)
        {
            Rbody.AffectedByGravity = obey;
        }

        public void setMass(float mass)
        {
            this.mass = mass;
        }
        public float getMass()
        {
            return mass; ///needs chaging to jitter
        }
        public void setRestitution(float e)
        {
            Rbody.Restitution = e;

        }

        public void setVelocity(float x, float y, float z)
        {
            velocity.X = x;
            velocity.Y = y;
            velocity.Z = z;

            Rbody.Velocity = new JVector(x,y,z);
        }
        public void setVelocity(Vector3 v)
        {
            velocity = v;
            Rbody.Velocity = new JVector(v.X, v.Y, v.Z);
        }
        public void changeVelociy(Vector3 v)
        {
            velocity += v;
            Rbody.Velocity += new JVector(v.X, v.Y, v.Z);
        }




        public override void Update(float dt)
        {
            position = FromJVec(Rbody.Position);
            rotation = FromJquaternion(Rbody.Orientation);
            rotation.Normalize();
            //Console.WriteLine(Rbody.Position);
        }



        public override void Destroy()
        {
            base.Destroy();
            
        }
    }
}
