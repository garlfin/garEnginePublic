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
        base.Use(program);
        program.SetUniform(RealLocation, _value.Use(SlotManager.GetUnit()));
    }
}

public static class SlotManager
{
    private static int _currentTextureUnit;

    private static readonly int[] Pairs;

    private static int _program;

    static SlotManager()
    {
        Pairs = new int[GL.GetInteger(GetPName.MaxCombinedTextureImageUnits)];
        for (int i = 0; i < Pairs.Length; i++)
        {
            Pairs[i] = -1;
        }
    }

    public static bool IsSlotSame(int slot, int texId)
    {
        return Pairs[slot] == texId;
    }

    public static void SetSlot(int slot, int texId)
    {
        Pairs[slot] = texId;
    }

    public static void ResetUnit()
    {
        _currentTextureUnit = 0;
    }

    public static int GetUnit()
    {
        _currentTextureUnit++;
        return _currentTextureUnit - 1;
    }
    
    public static bool IsSlotSame(ShaderProgram program)
    {
        return _program == program.Get();
    }

    public static void SetSlot(ShaderProgram program)
    {
        _program = program.Get();
    }
}