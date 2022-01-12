using Assimp;
using gESilk.engine.render;
using static gESilk.engine.Globals;

namespace gESilk.engine.assimp;

public class Material
{
    private ShaderProgram program;
    
    public void Use()
    {
        
    }
}

public class ShaderSetting<T>
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
            gl.ProgramUniform1(program.Get(), gl.GetUniformLocation(program.Get(), _uniformName), (int) _value);        
        }
    }
}



public unsafe class AssimpLoader
{
    private Scene _scene;

    public AssimpLoader(string path,
        PostProcessSteps steps = PostProcessSteps.Triangulate | PostProcessSteps.OptimizeGraph |
                                 PostProcessSteps.OptimizeMeshes | PostProcessSteps.FindInvalidData |
                                 PostProcessSteps.CalculateTangentSpace)
    {
        _scene = Globals.Assimp.ImportFile(path, steps);
    }
}