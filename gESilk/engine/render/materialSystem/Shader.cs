using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.materialSystem;

public class Shader
{
    private readonly int _id;

    public Shader(string data, ShaderType type)
    {
        _id = GL.CreateShader(type);
        GL.ShaderSource(_id, data);
        GL.CompileShader(_id);

        var shaderLog = GL.GetShaderInfoLog(_id);

        if (shaderLog.Length > 1) Console.WriteLine(shaderLog);
    }

    public int Get()
    {
        return _id;
    }
}