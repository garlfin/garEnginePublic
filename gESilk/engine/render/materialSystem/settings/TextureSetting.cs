using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;

namespace gESilk.engine.render.materialSystem.settings;

public class TextureSetting : ShaderSetting
{
    private readonly Texture _value;
    private readonly int _slot;


    public TextureSetting(string name, Texture value, int slot) : base(name)
    {
        UniformName = name;
        _value = value;
        _slot = slot;
    }

    public override void Use(ShaderProgram program)
    {
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        program.SetUniform(RealLocation, _value.Use(_slot));
    }
}