using System.Net.Mime;
using OpenTK.Graphics.OpenGL4;

namespace garEngine.render
{
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
                if (!string.IsNullOrEmpty(shaderLog))
                {
                    Console.WriteLine(shaderLog);
                }
                else
                {
                    Console.WriteLine($"{shaderLocation}: Shader Initialized");
                }
    
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

                if (!string.IsNullOrEmpty(programLog))
                {
                    Console.WriteLine(programLog);
                }
                else
                {
                    Console.WriteLine($"{shaderProgram}: Program Initialized");
                }

                return new ShaderProgram { Id = shaderProgram };

            }
        }
    
    public struct Shader
    {
        public int Id;
    }

    public struct ShaderProgram
    {
        public int Id;
        public List<ShaderSettingTex> ShaderSettingTexes = new List<ShaderSettingTex>();
    }

    public struct ShaderSettingTex
    {
        public string uniformName;
        public Texture value;
    }
    
    }

   
    
