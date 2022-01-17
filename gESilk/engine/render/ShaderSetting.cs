using gESilk.engine.assimp;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class ShaderSetting<T> : ISetting
{
    private string _uniformName;
    private T _value;
    public ShaderSetting(string name, T reference)
    {
        _uniformName = name;
        _value = reference;
    }

    public void Use(ShaderProgram program)
    {
        if (typeof(T) == typeof(int))
        {
            gl.ProgramUniform1(program.Get(), gl.GetUniformLocation(program.Get(), _uniformName), (int) (object) _value);        
        } else if (typeof(T) == typeof(Texture))
        {
            gl.ProgramUniform1(program.Get(), gl.GetUniformLocation(program.Get(), _uniformName), ((Texture) (object) _value).Use());       
        }
    }
}