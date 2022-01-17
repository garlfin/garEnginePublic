using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class ShaderProgram
{
    private uint _shaderId;

    private Shader _vertex, _fragment;

    public ShaderProgram(string path)
    {
        string[] file = File.ReadAllText(path).Split("#FRAGMENT");
        uint vertex = new Shader(file[0], ShaderType.VertexShader).Get();
        uint fragment = new Shader(file[1], ShaderType.FragmentShader).Get();
        
        _shaderId = gl.CreateProgram();
        
        gl.AttachShader(_shaderId, vertex);
        gl.AttachShader(_shaderId, fragment);
        
        gl.LinkProgram(_shaderId);
        
        gl.DetachShader(_shaderId, vertex);
        gl.DetachShader(_shaderId, fragment);
        
        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);
        
        string programLog = gl.GetProgramInfoLog(_shaderId);
        Console.WriteLine(!string.IsNullOrEmpty(programLog) ? programLog : $"{programLog}: Program Initialized");
    }

    public void Use()
    {
        gl.UseProgram(_shaderId);
    }

    public uint Get()
    {
        return _shaderId;
    }
    
    public void SetUniform(string name, int value)
    {
        gl.Uniform1(gl.GetUniformLocation(_shaderId, name), value);
    }
    public unsafe void SetUniform(string name, Matrix4X4<float> value)
    {
        
        gl.UniformMatrix4(gl.GetUniformLocation(_shaderId, name), false, (float*) &value);
        
    }
    public void SetUniform(string name, Vector3D<float> value)
    {
        gl.Uniform3(gl.GetUniformLocation(_shaderId, name),  value.X, value.Y, value.Z);
    }
}