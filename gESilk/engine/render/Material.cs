using System.Runtime.InteropServices;
using gESilk.engine.assimp;
using Silk.NET.Maths;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class Material
{
    private ShaderProgram _program;
    private readonly List<ISetting> _settings = new();

    public Material(ShaderProgram program)
    {
        _program = program;
    }

    public void Use(Matrix4X4<float> model)
    {
        _program.Use();
        _program.SetUniform("view", view);
        _program.SetUniform("projection", projection);
        _program.SetUniform("model", model);
        foreach (var setting in _settings)
        {
            setting.Use(_program);
        }
    }

    public void AddSetting(ISetting setting)
    {
        _settings.Add(setting);
    }
    public void EditSetting(int index, ISetting setting)
    {
        _settings[index] = setting;
    }
}