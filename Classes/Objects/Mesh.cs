using OpenTK.Graphics.OpenGL;

namespace _3D_Engine.Classes.Objects
{
    public class Mesh
    {
        public int VertexArrayObject;
        public int VertexBufferObject;
        public float[] vertices;

        public float[] position = new float[3] { 0f, 0f, 0f };

        public Mesh(int VertexArrayObject, int VertexBufferObject, float[] vertices)
        {
            this.VertexArrayObject = VertexArrayObject;
            this.VertexBufferObject = VertexBufferObject;
            this.vertices = vertices;
        }

        public void Draw()
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / 3);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(VertexBufferObject);
        }
    }
}
