using gESilk.engine.render.assets;

namespace gESilk.engine.render.materialSystem.settings;

public class FloatSetting : ShaderSetting
{
    private float _value;
    public FloatSetting(string name, float value) : base(name)
    {
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        program.SetUniform(RealLocation, _value);
    }
}