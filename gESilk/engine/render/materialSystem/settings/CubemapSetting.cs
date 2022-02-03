using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL;

namespace gESilk.engine.render.materialSystem.settings;

public class CubemapSetting : ShaderSetting
{
    public new string UniformName;
    private readonly CubemapTexture _value;


    public CubemapSetting(string name, CubemapTexture value) : base(name)
    {
        UniformName = name;
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        program.SetUniform(UniformName, _value.Use());
    }
}