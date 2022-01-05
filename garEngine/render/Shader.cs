
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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
    }

    public class Material
    {
        private readonly ShaderProgram _program;
        public List<ShaderSettingTex> ShaderSettingTexes = new List<ShaderSettingTex>();
        private List<TextureUnit> availableTargets =
            new List<TextureUnit>
            {
                TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3
            };
        

        public Material(ShaderProgram program)
        {
            _program = program;
        }

        public void AddSetting(string name, Texture value)
        {
            ShaderSettingTexes.Add(new ShaderSettingTex{uniformName = name, value = value});
        }
        public void Use()
        {
            GL.UseProgram(_program.Id);
            
            int i = 0;
            foreach (ShaderSettingTex settingTex in ShaderSettingTexes)
            {
                GL.ActiveTexture(availableTargets[i]);
                GL.BindTexture(TextureTarget.Texture2D, settingTex.value.id);
                GL.Uniform1(GL.GetUniformLocation(_program.Id, settingTex.uniformName), i+1);
                i++;
            }
        }

        public void SetUniform(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(_program.Id, name), value);
        }
        public void SetUniform(string name, ref Matrix4 value)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(_program.Id, name), false, ref value);
        }
        public void SetUniform(string name, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(_program.Id, name),  value);
        }
    }
    

    public struct ShaderSettingTex
    {
        public string uniformName;
        public Texture value;
    }

}

   
    
