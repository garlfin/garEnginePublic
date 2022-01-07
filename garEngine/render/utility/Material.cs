using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace garEngine.render.utility;

public class Material  : Asset
{
    private readonly ShaderProgram _program;
    public List<ShaderSettingTex> ShaderSettingTexes = new List<ShaderSettingTex>();
    public string Name;
    
    private List<TextureUnit> availableTargets =
        new List<TextureUnit>
        {
            TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3
        };
        

    public Material(ShaderProgram program)
    {
        _program = program;
        MaterialManager.Register(this);
    }

    public override void Delete()
    {
        GL.DeleteProgram(_program.Id);
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