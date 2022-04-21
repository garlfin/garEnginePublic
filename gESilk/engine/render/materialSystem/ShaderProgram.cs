using gESilk.engine.misc;
using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem;

public class ShaderProgram : Asset
{
    private int _shaderId;

    public ShaderProgram(string path)
    {
        AssetManager.Register(this);

        var file = File.ReadAllText(path).Split("-FRAGMENT-");

        int geometry = -1;

        if (file[1].Contains("-GEOMETRY-"))
        {
            var geomShader = file[1].Split("-GEOMETRY-");
            file[1] = geomShader[0];
            geometry = new Shader(geomShader[1], ShaderType.GeometryShader).Get();
        }

        var vertex = new Shader(file[0], ShaderType.VertexShader).Get();
        var fragment = new Shader(file[1], ShaderType.FragmentShader).Get();

        _shaderId = GL.CreateProgram();

        GL.AttachShader(_shaderId, vertex);
        GL.AttachShader(_shaderId, fragment);

        if (!geometry.GlNull()) GL.AttachShader(_shaderId, geometry);

        GL.LinkProgram(_shaderId);

        GL.DetachShader(_shaderId, vertex);
        GL.DetachShader(_shaderId, fragment);
        if (!geometry.GlNull()) GL.DetachShader(_shaderId, geometry);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
        if (!geometry.GlNull()) GL.DeleteShader(geometry);

        var programLog = GL.GetProgramInfoLog(_shaderId);

        if (string.IsNullOrEmpty(programLog)) return;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("SHADER ERROR: " + programLog);
        Console.ForegroundColor = default;
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
        if (uniform.GlNull()) return;
        GL.ProgramUniform1(_shaderId, uniform, value);
    }

    public void SetUniform(int name, int value)
    {
        if (name.GlNull()) return;
        GL.ProgramUniform1(_shaderId, name, value);
    }

    public void SetUniform(string name, float value)
    {
        var uniform = GetUniform(name);
        if (uniform.GlNull()) return;
        GL.ProgramUniform1(_shaderId, uniform, value);
    }

    public void SetUniform(int name, float value)
    {
        if (name.GlNull()) return;
        GL.ProgramUniform1(_shaderId, name, value);
    }

    public void SetUniform(string name, Matrix4 value)
    {
        var uniform = GetUniform(name);
        if (uniform.GlNull()) return;
        GL.ProgramUniformMatrix4(_shaderId, uniform, true, ref value);
    }

    public void SetUniform(int name, Matrix4 value)
    {
        if (name.GlNull()) return;
        GL.ProgramUniformMatrix4(_shaderId, name, true, ref value);
    }

    public void SetUniform(string name, Vector3 value)
    {
        var uniform = GetUniform(name);
        if (uniform.GlNull()) return;
        GL.ProgramUniform3(_shaderId, uniform, value);
    }

    public void SetUniform(int name, Vector3 value)
    {
        if (name.GlNull()) return;
        GL.ProgramUniform3(_shaderId, name, value);
    }

    public override void Delete()
    {
        if (_shaderId == -1) return;
        GL.DeleteProgram(_shaderId);
        _shaderId = -1;
    }
}