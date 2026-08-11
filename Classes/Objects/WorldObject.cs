using _3D_Engine.Classes.Scenes;
using Vector3 = OpenTK.Mathematics.Vector3;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3D_Engine.Classes.Objects
{
    public class WorldObject : Transform
    {

        // colliders moved to physics object not all world objects need colliders

        public WorldObject(string name, Shader shader, float[] vertices, Scene scene, Vector3 color)
            : base(name, shader, vertices, color)
        {
            this.scene = scene;

            SceneManager.RenderedEntities.Add(this);
        }
        public WorldObject(string name, Shader shader, float[] vertices, Vector3 color, WorldObject parent, Scene scene)
            : base(name, shader, vertices, color, parent)
        {
            this.scene = scene;

            SceneManager.RenderedEntities.Add(this);
        }
        // try only use in onrenderframe, otherwise the shader will be used multiple times and cause performance issues
        // pretty much deprecated, use recursive drawing func instead
        public void Draw()
        {
            shader.Use();
            //ApplyTransformation();
            shader.SetMatrix4("model", WorldMatrix);
            shader.SetVector3("ObjColor", color);
            mesh.Draw();
        }

        public virtual void Start() { } // initialisation logic
        public virtual void Update(float dt) { } // per-frame update logic (animations, physics)
        public virtual void OnDestroy() { } // cleanup logic before the object is destroyed (dispose shaders, buffers)


        public virtual void Destroy()
        {
            
            SceneManager.RenderedEntities.Remove(this);
            // Dispose  any resources of this object (e.g. shader, buffers)
            mesh.Dispose();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
