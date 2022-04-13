using OpenTK.Mathematics;

namespace gModel.res;

public struct Material
{
    public int MaterialID;
    public string Name;
    public ushort UniformCount;
    public IScriptValue[] Uniforms;

    public void Write(BinaryWriter writer)
    {
        writer.Write(MaterialID);
        writer.Write(Name);
        writer.Write(UniformCount);
        foreach (var uniform in Uniforms)
        {
            uniform.Write(writer);
        }
    }
}

public enum UniformTypeEnum
{ 
    Int = 0,
    Float = 1,
    Vector2 = 2,
    Vector3 = 3,
    Texture2D = 4,
    Texture3D = 5,
    String = 6
}

public struct Entity
{
    public string Name;
    
    public Vector3 Location;
    public Vector3 Rotation;
    public Vector3 Scale;

    public String[] Scripts;
    public IScriptValue[] ScriptValues;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Name);
        Location.WriteVec3(writer);
        Rotation.WriteVec3(writer);
        Scale.WriteVec3(writer);
        
        writer.Write((ushort) Scripts.Length);
        foreach (var script in Scripts)
        {
            writer.Write(script);
        }

        try
        {
            writer.Write((ushort) ScriptValues.Length);
            foreach (var scriptValue in ScriptValues)
            {
                scriptValue.Write(writer);
            }
        }
        catch (NullReferenceException)
        {
            writer.Write((ushort) 0);
        }
    }
}

public interface IScriptValue
{
    public int ScriptIndex { get; set; }
    public string Name { get; set; }
    public UniformTypeEnum ValueType { get; set; }

    public void Write(BinaryWriter writer);

}

public struct ScriptValue<T> : IScriptValue
{
    public int ScriptIndex { get; set; }
    public string Name { get; set; }
    public UniformTypeEnum ValueType { get; set; }
    public T Value;

    public void Write(BinaryWriter writer)
    {
        writer.Write(ScriptIndex);
        writer.Write(Name);
        writer.Write((int) ValueType);
        if (typeof(T) == typeof(Vector3))
        {
            ((Vector3) (object) Value).WriteVec3(writer);
        }
        else if (typeof(T) == typeof(int))
        {
            writer.Write((int) (object) Value);
        }
        else if (typeof(T) == typeof(float))
        {
            writer.Write((float) (object) Value);
        }
        else if (typeof(T) == typeof(string))
        {
            writer.Write((string) (object) Value);
        }
    }
    
}

