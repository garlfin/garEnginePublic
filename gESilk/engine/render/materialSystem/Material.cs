using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static gESilk.engine.Globals;

namespace gESilk.engine.render.materialSystem;

public class Material
{
    private readonly ShaderProgram _program;
    private readonly List<ShaderSetting> _settings = new();
    private bool _clearTranslation;

    public Material(ShaderProgram program)
    {
        _program = program;
    }

    public void ClearTranslation(bool value)
    {
        _clearTranslation = value;
    }

    public void Use(Matrix4 model)
    {
        _program.Use();
        _program.SetUniform("view", _clearTranslation ? View.ClearTranslation() : View);
        _program.SetUniform("projection", Projection);
        _program.SetUniform("model", model);
        foreach (var setting in _settings) setting.Use(_program);
    }

    public void Cleanup()
    {
        foreach (var setting in _settings) setting.Cleanup();
    }

    public void AddSetting(ShaderSetting setting)
    {
        _settings.Add(setting);
    }

    public void EditSetting(int index, ShaderSetting setting)
    {
        _settings[index] = setting;
    }
}