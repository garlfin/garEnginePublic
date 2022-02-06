using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL;

namespace gESilk.engine.render.materialSystem.settings;

public class NoiseTexSetting : ShaderSetting
{
    public new string UniformName;
    private readonly NoiseTexture _value;


    public NoiseTexSetting(string name, NoiseTexture value) : base(name)
    {
        UniformName = name;
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        program.SetUniform(UniformName, _value.Use());
    }
}