using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.CameraPresets
{
    public class EditorCam: Camera
    {
        public KeyboardState kbState;
        public Vector2 Size;
        public EditorCam(KeyboardState kbState, float sizeX, float sizeY) 
        {
            this.kbState = kbState;
            this.Size = new Vector2(sizeX, sizeY);
        }

        public void Update(float dt)
        {
            float speed = 5f;


            if (kbState.IsKeyDown(Keys.T))
                Position += Vector3.UnitZ * speed * dt;
            if (kbState.IsKeyDown(Keys.G))
                Position -= Vector3.UnitZ * speed * dt;
            if (kbState.IsKeyDown(Keys.F))
                Position += Vector3.UnitX * speed * dt;
            if (kbState.IsKeyDown(Keys.H))
                Position -= Vector3.UnitX * speed * dt;
            if (kbState.IsKeyDown(Keys.Space))
                Position += Vector3.UnitY * speed * dt;
            if (kbState.IsKeyDown(Keys.LeftShift))
                Position -= Vector3.UnitY * speed * dt;

            if (kbState.IsKeyDown(Keys.U))
                Rotation += new Vector3(0, 1, 0) * dt;
            if (kbState.IsKeyDown(Keys.O))
                Rotation -= new Vector3(0, 1, 0)* dt;
            if (kbState.IsKeyDown(Keys.D8))
                Rotation += new Vector3(1, 0, 0) * dt;
            if (kbState.IsKeyDown(Keys.K))
                Rotation -= new Vector3(1, 0, 0) * dt;
            // Mouse movement for rotation would be handled elsewhere, as it requires mouse input
        }

        public void OnFrameRender(Shader shader)
        {
            var projection = this.GetProjectionMatrix(Size.X, Size.Y);
            var view = this.GetViewMatrix();

            shader.Use();
            shader.SetMatrix4("projection", projection);
            shader.SetMatrix4("view", view);

        }
    }
}
