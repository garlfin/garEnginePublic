using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL;

namespace gESilk.engine.render.materialSystem.settings;

public class TextureSetting : ShaderSetting
{
    public new string UniformName;
    private readonly Texture _value;


    public TextureSetting(string name, Texture value) : base(name)
    {
        UniformName = name;
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        program.SetUniform(UniformName, _value.Use());
    }
}