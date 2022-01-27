using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.materialSystem.settings;

public abstract class ShaderSetting
{
    public string UniformName;

    public ShaderSetting(string name)
    {
        UniformName = name;
    }

    public virtual void Use(ShaderProgram program)
    {
        GL.ProgramUniform1(program.Get(), program.GetUniform(UniformName), 0);
    }

    public virtual void Cleanup()
    {
    }
}