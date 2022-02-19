using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem.settings;

public class Vec3Setting : ShaderSetting
{
    private readonly Vector3 _value;

    public Vec3Setting(string name, Vector3 value) : base(name)
    {
        UniformName = name;
        _value = value;
    }

    public Vec3Setting(string name, ref Vector3 value) : base(name)
    {
        UniformName = name;
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        program.SetUniform(UniformName, _value);
    }
}