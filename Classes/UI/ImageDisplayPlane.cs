using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.Scenes;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.UI
{
    public class ImageDisplayPlane
    {
        public string Name { get; set; }
        public Shader shader { get; set; }
        public Mesh mesh { get; protected set; }

        protected Vector3 position = Vector3.Zero; // world position
        //protected Vector3 rotation = Vector3.Zero; // radian // seems like we need to implement quaternions :(
        protected Quaternion rotation = Quaternion.Identity;
        protected Vector3 scale = Vector3.One;

        public Scene scene { get; protected set; }

        public ImageDisplayPlane()
        {
            shader = new Shader("Shaders/TextureDisplay/shader.vert", "Shaders/TextureDisplay/shader.frag");
            mesh = CreateMesh();
        }
        

        public void SetScene(Scene scene)
        {
            this.scene = scene;
        }

        public Vector3 GetPosition()
        {
            return position;

        }

        public Quaternion GetOrientation()
        {
            return rotation;
        }

        public Quaternion GetRotation()
        {

            return rotation;

        }
        public Vector3 GetScale()
        {

            return scale;

        }


        public Mesh CreateMesh()
        {

            float[] planeVertices =
            {
                // Triangle 1
                -0.5f, -0.5f, 0f,   0f, 0f, 1f,   0f, 0f,
                 0.5f, -0.5f, 0f,   0f, 0f, 1f,   1f, 0f,
                 0.5f,  0.5f, 0f,   0f, 0f, 1f,   1f, 1f,

                // Triangle 2
                -0.5f, -0.5f, 0f,   0f, 0f, 1f,   0f, 0f,
                 0.5f,  0.5f, 0f,   0f, 0f, 1f,   1f, 1f,
                -0.5f,  0.5f, 0f,   0f, 0f, 1f,   0f, 1f
            };


            // Generate and bind VAO
            int VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            // Generate and bind VBO
            int VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, planeVertices.Length * sizeof(float), planeVertices, BufferUsageHint.StaticDraw);

            //  data is position-only (3 floats for vertex)
            // or interleaved position+normal (6 floats per vertex).;


            // Position attribute (location = 0)
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);


            return new Mesh(VertexArrayObject, VertexBufferObject, planeVertices);
        }
    }
}
