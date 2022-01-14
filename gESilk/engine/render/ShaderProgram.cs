using Silk.NET.OpenGL;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class ShaderProgram
{
    private uint _shaderId;

    public ShaderProgram(string path)
    {
        string[] file = File.ReadAllText(path).Split("#VERTEX");
        uint vertex = gl.CreateShader(ShaderType.VertexShader);
        uint fragment = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(vertex, file[0]);
        gl.ShaderSource(fragment, file[1]);
    }

    public void Use()
    {
        gl.UseProgram(_shaderId);
    }

    public uint Get()
    {
        return _shaderId;
    }
}