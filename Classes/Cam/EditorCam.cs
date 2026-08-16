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

        private Game game;

        private float Yaw;
        private float Pitch;

        private float OrbitRadius = 10;
        private Vector3 CamPivot;

        private bool isTransitioning = false;
        private Vector3 targetPosition;
        private Vector3 targetFront;
        private Vector3 targetUp;
        private float t;

        public EditorCam(Game game, KeyboardState kbState, MouseState mouseState, float sizeX, float sizeY) 
        {
            this.kbState = kbState;
            this.game = game;
            this.Size = new Vector2(sizeX, sizeY);

            game.MouseWheel += ScrollTrigger;
        }


        public void Update(float dt)
        {
            float speed = 5f;


            //if (kbState.IsKeyDown(Keys.T))
            //    Position += Front.Normalized() * speed * dt;
            //if (kbState.IsKeyDown(Keys.G))
            //    Position -= Front.Normalized() * speed * dt;
            //if (kbState.IsKeyDown(Keys.F))
            //    Position -= Vector3.Cross(Front, Up).Normalized() * speed * dt;
            //if (kbState.IsKeyDown(Keys.H))
            //    Position += Vector3.Cross(Front, Up).Normalized() * speed * dt;
            //if (kbState.IsKeyDown(Keys.Space))
            //    Position += Up.Normalized() * speed * dt;
            //if (kbState.IsKeyDown(Keys.LeftShift))
            //    Position -= Up.Normalized() * speed * dt;

            //if (kbState.IsKeyDown(Keys.U))
            //    Rotation += new Vector3(0, 1, 0) * dt;
            //if (kbState.IsKeyDown(Keys.O))
            //    Rotation -= new Vector3(0, 1, 0)* dt;
            //if (kbState.IsKeyDown(Keys.D8))
            //    Rotation -= new Vector3(1, 0, 0) * dt;
            //if (kbState.IsKeyDown(Keys.K))
            //    Rotation += new Vector3(1, 0, 0) * dt;
            // Mouse movement for rotation would be handled elsewhere, as it requires mouse input


            
            if (game.mouseState.IsButtonPressed(MouseButton.Middle))
            {
                game.CursorState = OpenTK.Windowing.Common.CursorState.Confined; // only switch once not every frame
            }
            else if (game.mouseState.IsButtonReleased(MouseButton.Middle))
            {
                game.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
            }


            if (game.mouseState.IsButtonDown(MouseButton.Middle) && game.keyboardState.IsKeyDown(Keys.LeftShift))
            {
                Vector3 OffsetCamRight = Vector3.Cross(this.Up, this.Front).Normalized() * game.mouseState.Delta.X * 0.1f;
                Vector3 OffsetCamUp = this.Up * game.mouseState.Delta.Y * 0.1f;
                Vector3 TotalOffset = OffsetCamRight + OffsetCamUp;
                this.Position += TotalOffset;
                this.CamPivot += TotalOffset;
            }
            else if (game.mouseState.IsButtonDown(MouseButton.Middle))
            {
                this.MoveOribital(game.mouseState.Delta * 0.01f, Vector3.Zero);
            }

            if (isTransitioning)
            {
                Position = Vector3.Lerp(Position, targetPosition, t * dt);
                Front = Vector3.Lerp(Front, targetFront, t * dt).Normalized();
                Up = Vector3.Lerp(Up, targetUp, t * dt).Normalized();
                

                if ((Position - targetPosition).Length < 0.01f && (Front - targetFront).Length < 0.01f && (Up - targetUp).Length < 0.01f)
                {
                    isTransitioning = false;
                }
            }

            
        }

        public void OnFrameRender(Shader shader)
        {
            var projection = this.GetProjectionMatrix(Size.X, Size.Y);
            var view = this.GetViewMatrix();

            shader.Use();
            shader.SetMatrix4("projection", projection);
            shader.SetMatrix4("view", view);

        }

        public void transitionPositionRotation(Vector3 targetPosition, Vector3 Front, Vector3 Up, float t)
        {
            isTransitioning = true;
            this.targetPosition = targetPosition;
            this.targetFront = Front;
            this.targetUp = Up;
            this.t = t;
        }

        public void MoveOribital(Vector2 deltaMousePos, Vector3 Centre)
        {
            this.CamPivot = Centre;

            // deltapos: X - pitch, y - yaw
            this.Yaw += deltaMousePos.Y;
            this.Pitch += deltaMousePos.X;

            Vector3 dir = Vector3.Zero;
            dir.X = this.OrbitRadius * MathF.Cos(Pitch) * MathF.Cos(Yaw);
            dir.Y = this.OrbitRadius * MathF.Sin(Yaw);
            dir.Z = this.OrbitRadius * MathF.Cos(Yaw) * MathF.Sin(Pitch);

            this.Position = this.CamPivot + (dir * this.OrbitRadius);
            this.LookAtPos(this.CamPivot);
        }

        private void ScrollTrigger(OpenTK.Windowing.Common.MouseWheelEventArgs obj)
        {
            if (game.keyboardState.IsKeyDown(Keys.M)) // if a bone is being moved
            {
                return;
            }
                this.OrbitRadius -= obj.OffsetY;
            this.OrbitRadius = Math.Clamp(this.OrbitRadius, 2, 30);

            Vector3 dir = Vector3.Zero;
            dir.X = this.OrbitRadius * MathF.Cos(Pitch) * MathF.Cos(Yaw);
            dir.Y = this.OrbitRadius * MathF.Sin(Yaw);
            dir.Z = this.OrbitRadius * MathF.Cos(Yaw) * MathF.Sin(Pitch);

            this.Position = this.CamPivot + (dir * this.OrbitRadius);
        }
    }
}
