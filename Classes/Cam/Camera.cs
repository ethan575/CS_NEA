using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace _3D_Engine.Classes.CameraPresets
{
    public class Camera
    {
        public Vector3 Position;
        Vector3 _rotation; // should be same as rotation returning rotation directly cause overflow
        public Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                SetRotation(value);
                
            }
        }
        public Vector3 Front = Vector3.UnitZ;//-Vector3.UnitZ;
        Vector3 Up = Vector3.UnitY;

        public float Fov = 60f;

        public Camera()
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Front = Vector3.UnitZ - new Vector3(0, 0.5f, 0);
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up); // facing direction
        }

        public Matrix4 GetProjectionMatrix(float width, float height)
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(Fov),
                (float)width / (float)height,
                0.1f,
                1000f
            );
        }

        public void SetRotation(Vector3 rotation)
        {
            Quaternion quaternion = Quaternion.FromEulerAngles(rotation.X, rotation.Y, rotation.Z);
            Front = Vector3.Transform(Vector3.UnitZ, quaternion);
            Up = Vector3.Transform(Vector3.UnitY, quaternion);
            _rotation = rotation;

        }

        public void ResetRotation()
        {
            Front = Vector3.UnitZ;//-Vector3.UnitZ;
            Up = Vector3.UnitY;
        }
    }
}
