using Jitter2.LinearMath;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;
using Quaternion = OpenTK.Mathematics.Quaternion;
using MathHelper = OpenTK.Mathematics.MathHelper;

namespace _3D_Engine.Classes.Objects
{
    public class Transform : Entity
    {
        public Transform(string name, Shader shader, float[] vertices, Vector3 color)
            : base(name, shader, vertices, color)
        {

        }
        public Transform(string name, Shader shader, float[] vertices, Vector3 color, WorldObject parent)
            : base(name, shader, vertices, color)
        {
            
        }

        public virtual void setPosition(float x, float y, float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
        }

        public virtual void setPosition(Vector3 v)
        {
            position = v;
        }

        public virtual void setRotation(float x, float y, float z)
        {
            //rotation.X = MathHelper.DegreesToRadians(x);
            //rotation.Y = MathHelper.DegreesToRadians(y);
            //rotation.Z = MathHelper.DegreesToRadians(z);
                rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x), 
                    MathHelper.DegreesToRadians(y), 
                    MathHelper.DegreesToRadians(z));
            rotation.Normalize(); // normalize to prevent gimbal lock
        }
        public virtual void setRotation(Quaternion q)
        {
            rotation = q;
            rotation.Normalize(); // normalize to prevent gimbal lock
            //rotation.Normalize(); // normalize to prevent gimbal lock
        }

        public void moveBy(float x, float y, float z)
        {
            position.X += x;
            position.Y += y;
            position.Z += z;
        }
        public void moveBy(Vector3 v)
        {
            position += v;
        }
        public void rotate(float x, float y, float z)
        {
            //rotation.X += MathHelper.DegreesToRadians(x);
            //rotation.Y += MathHelper.DegreesToRadians(y);
            //rotation.Z += MathHelper.DegreesToRadians(z);
            rotation *= Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(x),
                    MathHelper.DegreesToRadians(y),
                    MathHelper.DegreesToRadians(z));
            rotation.Normalize(); 
        }
        public void Scale(float x, float y, float z)
        {
            scale.X *= x;
            scale.Y *= y;
            scale.Z *= z;
        }
        public void Scale(float factor)
        {
            scale.X *= factor;
            scale.Y *= factor;
            scale.Z *= factor;
        }

        public void setScale(float x, float y, float z)
        {
            scale.X = x;
            scale.Y = y;
            scale.Z = z;
        }

        protected Matrix4 CreateTransformationMatix()
        {
            //return Matrix4.CreateScale(scale.X, scale.Y, scale.Z) *
            //       Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation.X, rotation.Y, rotation.Z)) * // quaternion rotation to prevent gimbal lock
            //       Matrix4.CreateTranslation(position.X, position.Y, position.Z);

            // flipped x postion so negative is left
            return Matrix4.CreateScale(scale.X, scale.Y, scale.Z) *
                   Matrix4.CreateFromQuaternion(rotation) * // quaternion rotation to prevent gimbal lock
                   Matrix4.CreateTranslation(-position.X, position.Y, position.Z);
        }
     //}
        public Matrix4 WorldMatrix // return worl space matrix
        {
            get
            {
                Matrix4 local = CreateTransformationMatix();
                return local;
            }
        }
    }
}