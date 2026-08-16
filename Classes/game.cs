using _3D_Engine;
using _3D_Engine.Classes;
using _3D_Engine.Classes.CameraPresets;
using _3D_Engine.Classes.Scenes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Game : GameWindow
{
    public Shader shader;
    //Camera cam = new Camera();
    EditorCam cam;

    public Matrix4 projection { get; private set; }
    public Matrix4 view {get; private set;}

    public float fps;

    public KeyboardState keyboardState;
    public MouseState mouseState;

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings()
        { ClientSize = new Vector2i(width, height), Title = title })
    {
        keyboardState = KeyboardState;
        mouseState = MouseState;
        cam = new EditorCam(this, keyboardState, mouseState, Size.X, Size.Y);
    }

    public Camera GetCamera()
    {
        return cam;
    }


    protected override void OnLoad()
    {
        

        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Viewport(0, 0, Size.X, Size.Y);
        GL.Enable(EnableCap.DepthTest);
        VSync = VSyncMode.On;
        shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

        shader.SetVector3("lightPos", new OpenTK.Mathematics.Vector3(10.0f, 10.0f, 10.0f));

        cam.Position = new Vector3(0, 10, -8);
        cam.Front = Vector3.UnitZ - new Vector3(0, 0.5f, 0);

        projection = cam.GetProjectionMatrix(Size.X, Size.Y);
        view = cam.GetViewMatrix();

        ///change this at some point////////////////////////
        ///// just starts with the simulation scene, will add a menu scene later 
        SceneManager.CurrentScene = new SimulationScene(this);

        shader.Use();
        shader.SetMatrix4("projection", projection);
        shader.SetMatrix4("view", view);



    }

    protected override void OnUnload()
    {

        // Dispose shader, buffers, textures etc
        shader.Dispose();

        base.OnUnload();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {

        fps = 1f / (float)args.Time; // args.Time is time since last frame

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        

        // activate shader
        shader.Use();

        if (SceneManager.CurrentScene.IsReady)
        {
            SceneManager.CurrentScene?.OnFrameRender((float)args.Time); // pass dt
        }
        //// draw  here
        //SceneManager.CurrentScene?.OnFrameRender((float)args.Time); // pass dt

        cam.OnFrameRender(shader);


        //Console.WriteLine("transform = " + GL.GetUniformLocation(shader.Handle, "transform"));
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {

        cam.Update((float)args.Time); // update the camera

        view = cam.GetViewMatrix();
        shader.Use();
        shader.SetMatrix4("view", view);

        // update physics, algorithms
        if (SceneManager.CurrentScene.IsReady)
        {
            SceneManager.CurrentScene?.Update((float)args.Time); // execute only if not null
        }
        
        keyboardState = KeyboardState; // update the keyboardstate so it can be accessed else where
        mouseState = MouseState; // update the mousestate so it can be accessed else where

    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);

        //projection = Matrix4.CreateOrthographicOffCenter(0f, e.Width, 0f, e.Height, -1f, 1f);
        projection = cam.GetProjectionMatrix(e.Width, e.Height);
        view = cam.GetViewMatrix();
        shader.Use();
        shader.SetMatrix4("projection", projection);
        shader.SetMatrix4("view", view);
    }
}
