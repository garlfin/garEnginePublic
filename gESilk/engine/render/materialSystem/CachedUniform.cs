using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem;

public class CachedUniform<T>
{
    private int _id;
    private readonly ShaderProgram _program;
    private readonly string _uniformName;


    public CachedUniform(ShaderProgram program, string uniformName)
    {
        _program = program;
        _uniformName = uniformName;
        _id = program.GetUniform(uniformName);
    }

    public void RecalculateUniform()
    {
        _id = _program.GetUniform(_uniformName);
    }

    public void Use(T value)
    {
        if (_id == -1) return;

        switch (value)
        {
            case int:
                _program.SetUniform(_id, (int)(object)value);
                break;
            case Vector3:
                _program.SetUniform(_id, (Vector3)(object)value);
                break;
            case float:
                _program.SetUniform(_id, (float)(object)value);
                break;
            case Matrix4:
                _program.SetUniform(_id, (Matrix4)(object)value);
                break;
            case Texture:
                _program.SetUniform(_id, ((Texture)(object)value).Use(SlotManager.GetUnit()));
                break;
            default:
                throw new ArgumentException($"Type {value.GetType()} is not recognized.");
        }
    }
}