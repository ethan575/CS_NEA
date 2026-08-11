using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Shader
{
    public int Handle;
    private bool disposed = false;
    public int VertexShader, FragmentShader;

    public Shader(string vertexPath, string fragmentPath)
    {
        
        CompileShaders(vertexPath, fragmentPath, out VertexShader, out FragmentShader);
        LinkShaderProgram(VertexShader, FragmentShader);


        string vertLog = GL.GetShaderInfoLog(VertexShader);
        string fragLog = GL.GetShaderInfoLog(FragmentShader);
        string programLog = GL.GetProgramInfoLog(Handle);

        Console.WriteLine("VERTEX LOG: " + vertLog);
        Console.WriteLine("FRAGMENT LOG: " + fragLog);
        Console.WriteLine("PROGRAM LOG: " + programLog);

    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            GL.DeleteProgram(Handle);
            disposed = true;
        }
    }

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location, false, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(location, vector);
    }   


    // needs to be called manually or memory leak so GPU resources will not be freed
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Finalizer/ destructor runs when the object is not referened anymore
    // and is being collected by the garbage collector
    // make sure that the shader program is deleted when the object is collected 
    // free up GPU resources
    ~Shader()
    {
        if (disposed == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }

    private void CompileShaders(string vertexPath, string fragmentPath, out int VertexShader, out int FragmentShader)
    {
        string VertexShaderSource = File.ReadAllText(vertexPath);
        string FragmentShaderSource = File.ReadAllText(fragmentPath);

        VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, VertexShaderSource);
        FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, FragmentShaderSource);

        GL.CompileShader(VertexShader);
        GL.CompileShader(FragmentShader);

        int success;

        GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(VertexShader);
            Console.WriteLine($"Error compiling vertex shader: {infoLog}");
        }
        GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(FragmentShader);
            Console.WriteLine($"Error compiling fragment shader: {infoLog}");
        }
    }

    private void LinkShaderProgram(int VertexShader, int FragmentShader)
    {
        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, VertexShader);
        GL.AttachShader(Handle, FragmentShader);

        GL.LinkProgram(Handle);

        int success;
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine($"Error linking shader program: {infoLog}");
        }

        GL.DetachShader(Handle, VertexShader);
        GL.DetachShader(Handle, FragmentShader);
        GL.DeleteShader(VertexShader);
        GL.DeleteShader(FragmentShader);
    }

}