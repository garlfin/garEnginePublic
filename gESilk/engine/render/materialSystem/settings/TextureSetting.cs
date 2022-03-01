using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.materialSystem.settings;

public class TextureSetting : ShaderSetting
{
    private readonly Texture _value;


    public TextureSetting(string name, Texture value) : base(name)
    {
        _value = value;
    }

    public override void Use(ShaderProgram program)
    {
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        program.SetUniform(RealLocation, _value.Use(TextureSlotManager.GetUnit()));
    }
}

public static class TextureSlotManager
{
    private static int _currentTextureUnit;

    public static void ResetUnit()
    {
        _currentTextureUnit = 0;
    }

    public static int GetUnit()
    {
        _currentTextureUnit++;
        return _currentTextureUnit - 1;
    }
}