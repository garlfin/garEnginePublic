﻿using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static gESilk.engine.Globals;

namespace gESilk.engine.render.materialSystem;

public class Material
{
    private readonly ShaderProgram _program;
    private readonly List<ShaderSetting> _settings = new();
    private readonly DepthFunction _function;

    public Material(ShaderProgram program, DepthFunction function = DepthFunction.Equal)
    {
        _program = program;
        _function = function;
    }
    

    public void Use(Matrix4 model, bool clearTranslation)
    {
        GL.DepthFunc(_function);
        _program.Use();
        _program.SetUniform("view", clearTranslation ? View.ClearTranslation() : View);
        _program.SetUniform("projection", Projection);
        _program.SetUniform("model", model);
        _program.SetUniform("viewPos", Camera.Position);
        for (var i = 0; i < _settings.Count; i++)
        {
            var setting = _settings[i];
            setting.Use(_program);
        }
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