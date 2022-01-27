using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.materialSystem.settings;

public class DepthFuncSetting : ShaderSetting
{
    public new string UniformName;
    private readonly DepthFunction _value;

    public DepthFuncSetting(string name, DepthFunction function) : base(name)
    {
        UniformName = name;
        _value = function;
    }
    
    public override void Use(ShaderProgram program)
    {
        GL.DepthFunc(_value);
    }

    public override void Cleanup()
    {
        GL.DepthFunc(DepthFunction.Less);
    }
}