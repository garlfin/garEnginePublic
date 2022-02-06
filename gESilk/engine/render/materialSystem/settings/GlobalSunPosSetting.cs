using gESilk.engine.render.assets;

namespace gESilk.engine.render.materialSystem.settings;

public class GlobalSunPosSetting : ShaderSetting
{
    
    private new string UniformName;
    
    
    public GlobalSunPosSetting(string name) : base(name)
    {
        UniformName = name;
    }

    public override void Use(ShaderProgram program)
    {
        program.SetUniform(UniformName, Globals.SunPos);
    }
}
