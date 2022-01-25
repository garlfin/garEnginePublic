using System.Runtime.InteropServices;
using gESilk.engine.assimp;
using OpenTK.Mathematics;

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

    public void Use(Matrix4 model)
    {
        _program.Use();
        _program.SetUniform("view", ref view);
        _program.SetUniform("projection", ref projection);
        _program.SetUniform("model", ref model);
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