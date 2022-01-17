using Silk.NET.OpenGL;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class Shader
{
    private readonly uint _id;

    public Shader(string data, ShaderType type)
    {
        
        _id = gl.CreateShader(type);
        gl.ShaderSource(_id, data);
        gl.CompileShader(_id);

        var shaderLog = gl.GetShaderInfoLog(_id);
        Console.WriteLine(!string.IsNullOrEmpty(shaderLog) ? $"{_id}: {shaderLog}" : $"{_id}: Shader Initialized");
    }

    public uint Get()
    {
        return _id;
    }
}