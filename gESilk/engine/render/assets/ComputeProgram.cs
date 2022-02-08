using gESilk.engine.render.materialSystem;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets;

public class ComputeProgram : Asset
{

    private readonly int _shaderId;
    
    public ComputeProgram(string path)
    {
        AssetManager.Register(this);

        var file = File.ReadAllText(path);
        var compute = new Shader(file, ShaderType.ComputeShader).Get();
        
        _shaderId = GL.CreateProgram();

        GL.AttachShader(_shaderId, compute);

        GL.LinkProgram(_shaderId);

        GL.DetachShader(_shaderId, compute);

        GL.DeleteShader(compute);
  

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
    public void SetUniform(int name, int value)
    {
        if (name == -1) return;
        GL.Uniform1(name, value);
    }

    public void SetUniform(string name, float value)
    {
        var uniform = GetUniform(name);
        if (uniform == -1) return;
        GL.Uniform1(uniform, value);
    }
    public void SetUniform(int name, float value)
    {
        if (name == -1) return;
        GL.Uniform1(name, value);
    }

    public void SetUniform(string name, Matrix4 value)
    {
        var uniform = GetUniform(name);
        if (uniform == -1) return;
        GL.UniformMatrix4(uniform, true, ref value);
    }
    
    public void SetUniform(int name, Matrix4 value)
    {
        if (name == -1) return;
        GL.UniformMatrix4(name, true, ref value);
    }

    public void SetUniform(string name, Vector3 value)
    {
        var uniform = GetUniform(name);
        if (uniform == -1) return;
        GL.Uniform3(uniform, value);
    }
    
    public void SetUniform(int name, Vector3 value)
    {
        if (name == -1) return;
        GL.Uniform3(name, value);
    }

    public override void Delete()
    {
       GL.DeleteProgram(_shaderId);
    }
}