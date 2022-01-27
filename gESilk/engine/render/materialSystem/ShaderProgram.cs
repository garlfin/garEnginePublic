using gESilk.engine.misc;
using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem;

public class ShaderProgram : Asset
{
    private readonly int _shaderId;

    public ShaderProgram(string path)
    {
        ShaderProgramManager.Register(this);

        var file = File.ReadAllText(path).Split("#FRAGMENT");
        var vertex = new Shader(file[0], ShaderType.VertexShader).Get();
        var fragment = new Shader(file[1], ShaderType.FragmentShader).Get();

        _shaderId = GL.CreateProgram();

        GL.AttachShader(_shaderId, vertex);
        GL.AttachShader(_shaderId, fragment);

        GL.LinkProgram(_shaderId);

        GL.DetachShader(_shaderId, vertex);
        GL.DetachShader(_shaderId, fragment);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        var programLog = GL.GetProgramInfoLog(_shaderId);
        Console.WriteLine(!string.IsNullOrEmpty(programLog) ? programLog : $"{_shaderId}: Program Initialized");
    }

    public void Use()
    {
        GL.UseProgram(_shaderId);
    }

    public int Get()
    {
        return _shaderId;
    }

    public int GetUniform(string name)
    {
        return GL.GetUniformLocation(_shaderId, name);
    }

    public void SetUniform(string name, int value)
    {
        var uniform = GetUniform(name);
        if (uniform == -1) return;
        GL.Uniform1(uniform, value);
    }

    public void SetUniform(string name, Matrix4 value)
    {
        var uniform = GetUniform(name);
        if (uniform == -1) return;
        GL.UniformMatrix4(uniform, false, ref value);
    }

    public void SetUniform(string name, Vector3 value)
    {
        var uniform = GetUniform(name);
        if (uniform == -1) return;
        GL.Uniform3(uniform, value);
    }

    public override void Delete()
    {
        GL.DeleteProgram(_shaderId);
    }
}

internal class ShaderProgramManager : AssetManager<ShaderProgram>
{
}