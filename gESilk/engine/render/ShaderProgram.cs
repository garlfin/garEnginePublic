using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class ShaderProgram
{
    private int _shaderId;

    public ShaderProgram(string path)
    {
        string[] file = File.ReadAllText(path).Split("#FRAGMENT");
        int vertex = new Shader(file[0], ShaderType.VertexShader).Get();
        int fragment = new Shader(file[1], ShaderType.FragmentShader).Get();
        
        _shaderId = GL.CreateProgram();
        
        GL.AttachShader(_shaderId, vertex);
        GL.AttachShader(_shaderId, fragment);
        
        GL.LinkProgram(_shaderId);
        
        GL.DetachShader(_shaderId, vertex);
        GL.DetachShader(_shaderId, fragment);

        string programLog = GL.GetProgramInfoLog(_shaderId);
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

    private int GetUniform(string name)
    {
        return GL.GetUniformLocation(_shaderId, name);
    }
    public void SetUniform(string name, int value)
    {
        GL.Uniform1(GetUniform(name), value);
    }
    public void SetUniform(string name, ref Matrix4 value)
    {
        
        GL.UniformMatrix4(GetUniform(name), false, ref value);
        
    }
    public void SetUniform(string name, Vector3 value)
    {
        GL.Uniform3(GetUniform(name),  value);
    }
}