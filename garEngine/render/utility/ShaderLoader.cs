using OpenTK.Graphics.OpenGL4;

namespace garEngine.render.utility;

public static class ShaderLoader
{
    public static float HexToFloat(int hexVal)
    {
        return (float)hexVal / 256;
    }
    public static Shader LoadShader(string shaderLocation, ShaderType type)
    {
        int shaderId = GL.CreateShader(type);
        GL.ShaderSource(shaderId, File.ReadAllText(shaderLocation));
        GL.CompileShader(shaderId);

        string shaderLog = GL.GetShaderInfoLog(shaderId);
        Console.WriteLine(!string.IsNullOrEmpty(shaderLog) ? shaderLog : $"{shaderLocation}: Shader Initialized");

        return new Shader {Id = shaderId};
    }

    public static ShaderProgram LoadShaderProgram(string vertex, string fragment)
    {
        int shaderProgram = GL.CreateProgram();
        Shader vertexShader = LoadShader(vertex, ShaderType.VertexShader);
        Shader fragmentShader = LoadShader(fragment, ShaderType.FragmentShader);
    
        GL.AttachShader(shaderProgram, vertexShader.Id);
        GL.AttachShader(shaderProgram, fragmentShader.Id);
        GL.LinkProgram(shaderProgram);
        GL.DetachShader(shaderProgram, vertexShader.Id);
        GL.DetachShader(shaderProgram, fragmentShader.Id);
        GL.DeleteShader(vertexShader.Id);
        GL.DeleteShader(fragmentShader.Id);

        string programLog = OpenTK.Graphics.OpenGL.GL.GetProgramInfoLog(shaderProgram);

        Console.WriteLine(!string.IsNullOrEmpty(programLog) ? programLog : $"{shaderProgram}: Program Initialized");

        return new ShaderProgram { Id = shaderProgram };

    }
}