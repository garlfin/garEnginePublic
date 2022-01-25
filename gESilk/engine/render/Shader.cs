using OpenTK.Graphics.OpenGL4;

using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class Shader
{
    private readonly int _id;

    public Shader(string data, ShaderType type)
    {
        
        _id = GL.CreateShader(type);
        GL.ShaderSource(_id, data);
        GL.CompileShader(_id);

        var shaderLog = GL.GetShaderInfoLog(_id);
        Console.WriteLine(!string.IsNullOrEmpty(shaderLog) ? $"{_id}: {shaderLog}" : $"{_id}: Shader Initialized");
    }

    public int Get()
    {
        return _id;
    }
}