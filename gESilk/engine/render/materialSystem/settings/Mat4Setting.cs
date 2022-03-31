using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem.settings;

public class Mat4Setting : ShaderSetting
{
    private readonly Matrix4 _value;

    public Mat4Setting(string name, Matrix4 value) : base(name)
    {
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        base.Use(program);
        program.SetUniform(RealLocation, _value);
    }
}