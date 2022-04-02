﻿using gESilk.engine.render.assets.textures;

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
        program.SetUniform(RealLocation, _value.Use(TextureSlotManager.GetUnit()));
    }
}

public static class TextureSlotManager
{
    private static int _currentTextureUnit;

    private static readonly int[] Pairs;

    static TextureSlotManager()
    {
        Pairs = new int[20];
        for (int i = 0; i < 20; i++)
        {
            Pairs[i] = i;
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
}