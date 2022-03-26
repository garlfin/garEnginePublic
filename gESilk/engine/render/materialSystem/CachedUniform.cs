using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem;

public class CachedUniform<T>
{
    private int _id;
    private readonly ShaderProgram _program;


    public CachedUniform(ShaderProgram program, string uniformName)
    {
        _program = program;
        _id = program.GetUniform(uniformName);
    }

    public void Use(T _value)
    {
        if (_id == -1) return;

        switch (_value)
        {
            case int:
                _program.SetUniform(_id, (int)(object)_value);
                break;
            case Vector3:
                _program.SetUniform(_id, (Vector3)(object)_value);
                break;
            case float:
                _program.SetUniform(_id, (float)(object)_value);
                break;
            case Matrix4:
                _program.SetUniform(_id, (Matrix4)(object)_value);
                break;
            case Texture:
                _program.SetUniform(_id, ((Texture)(object)_value).Use(TextureSlotManager.GetUnit()));
                break;
            default:
                throw new ArgumentException($"Type {_value.GetType()} is not recognized.");
        }
    }
}