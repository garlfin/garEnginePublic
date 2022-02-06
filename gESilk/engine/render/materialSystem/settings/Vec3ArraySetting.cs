using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem.settings;

public class Vec3ArraySetting : ShaderSetting
{
    private readonly Vector3[] _data;
    public Vec3ArraySetting(string name, Vector3[] data) : base(name)
    {
        _data = data;
    }

    public override void Use(ShaderProgram program)
    {
        for (var i = 0; i < _data.Length; i++)
        {
            program.SetUniform($"{UniformName}[{i}]", _data[i]);
        }
    }
}