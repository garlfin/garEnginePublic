using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL;

namespace gESilk.engine.render.materialSystem.settings;

public class FloatSetting : ShaderSetting
{
    private readonly float _value;
    private float _prevValue;

    public FloatSetting(string name, float value) : base(name)
    {
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        if (RealLocation == -1) return;
        GL.GetUniform(program.Get(), RealLocation, out _prevValue);
        program.SetUniform(RealLocation, _value);
    }

    public override void Cleanup(ShaderProgram program)
    {
        program.SetUniform(RealLocation, _prevValue);
    }
}