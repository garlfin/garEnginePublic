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
        base.Use(program);
        RealLocation = program.GetUniform(UniformName);
        program.SetUniform(RealLocation, _value);
    }

    public override void Cleanup(ShaderProgram program)
    {
        program.SetUniform(RealLocation, 0f);
    }
}