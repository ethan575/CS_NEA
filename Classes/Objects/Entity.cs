using _3D_Engine.Classes.Scenes;
using OpenTK.Graphics.OpenGL4;
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector3 = OpenTK.Mathematics.Vector3;



namespace _3D_Engine.Classes.Objects
{
    public class Entity
    {
        public string Name { get; set; }
        public Shader shader { get; set; }
        public Mesh mesh { get; protected set; }
        public Vector3 color { get; set; } = Vector3.Zero;
        public Vector3 defaultColor { get; set; } = Vector3.Zero;

        protected Vector3 position = Vector3.Zero; // world position
        //protected Vector3 rotation = Vector3.Zero; // radian // seems like we need to implement quaternions :(
        protected Quaternion rotation = Quaternion.Identity;
        protected Vector3 scale = Vector3.One;

        public Scene scene { get; protected set; }

        public Entity(string name, Shader shader, float[] vertices, Vector3 color)
        {
            Name = name;
            this.shader = shader;
            mesh = CreateMesh(vertices);
            this.color = color;
            this.defaultColor = color;
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
        

        protected Mesh CreateMesh(float[] vertices)
        {
            // Generate and bind VAO
            int VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            // Generate and bind VBO
            int VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            //  data is position-only (3 floats for vertex)
            // or interleaved position+normal (6 floats per vertex).
            int componentsPerVertex = vertices.Length % 6 == 0 ? 6 : 3;
            int stride = componentsPerVertex * sizeof(float);

            // Position attribute (location = 0)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // Normal attribute (location = 1) if present (pos, nx,ny,nz)
            if (componentsPerVertex >= 6)
            {
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            }

            return new Mesh(VertexArrayObject, VertexBufferObject, vertices);
        }
    }
}
