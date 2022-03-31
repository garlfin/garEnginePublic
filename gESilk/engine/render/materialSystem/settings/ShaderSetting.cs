namespace gESilk.engine.render.materialSystem.settings;

public abstract class ShaderSetting
{
    protected int RealLocation = -1;
    protected string UniformName;
    private bool _enabled = true;

    protected ShaderSetting(string name)
    {
        UniformName = name;
    }

    public virtual void Use(ShaderProgram program)
    {
        if (!_enabled) return;
        if (RealLocation == -1) RealLocation = program.GetUniform(UniformName);
        if (RealLocation == -1) _enabled = false;
    }

    public virtual void Cleanup(ShaderProgram program)
    {
        if (RealLocation == -1) _enabled = false;
    }
}