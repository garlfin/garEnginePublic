using gESilk.engine.render.assets;

namespace gESilk.engine.render.materialSystem.settings;

public abstract class ShaderSetting
{
    protected string UniformName;
    protected int RealLocation = -1;

    protected ShaderSetting(string name)
    {
        UniformName = name;
    }

    public virtual void Use(ShaderProgram program)
    {
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        program.SetUniform(RealLocation, 0);
    }

    public virtual void Cleanup(ShaderProgram program)
    {
    }
}