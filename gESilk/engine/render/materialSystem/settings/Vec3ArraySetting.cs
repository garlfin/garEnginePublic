using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem.settings;

public class Vec3ArraySetting : ShaderSetting
{
    private readonly Vector3[] _data;
    private int[]? _location = null;
    public Vec3ArraySetting(string name, Vector3[] data) : base(name)
    {
        _data = data;
    }

    public override void Use(ShaderProgram program)
    {
        if (_location == null)
        {
            _location = new int[_data.Length];
            for (var i = 0; i < _data.Length; i++)
            {
                _location[i] = program.GetUniform($"{UniformName}[{i}]");
            }
        }
        for (var i = 0; i < _data.Length; i++)
        {
            program.SetUniform(_location[i], _data[i]);
        }
    }
}