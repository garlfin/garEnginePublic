using Assimp;
using OpenTK.Mathematics;

namespace gModel.res;


/*
    using var stream = File.Open($"output/{fileNoExtension}.gmod", FileMode.Create);
    using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
    mesh.Write(writer);
 */
public static class MathHelper
{
    public static void WriteVec3(BinaryWriter stream, Vector3D vector3D)
    {
        stream.Write(vector3D.X);
        stream.Write(vector3D.Y);
        stream.Write(vector3D.Z);
    }
    
    public static void WriteVec3(this Vector3 tempVec, BinaryWriter writer)
    {
        writer.Write(tempVec.X);
        writer.Write(tempVec.Y);
        writer.Write(tempVec.Z);
    }

    public static void WriteVec3(BinaryWriter stream, IntVec3 vector3D)
    {
        stream.Write(vector3D.X);
        stream.Write(vector3D.Y);
        stream.Write(vector3D.Z);
    }

    public static void WriteVec2(BinaryWriter stream, Vector2D vector2D)
    {
        stream.Write(vector2D.X);
        stream.Write(vector2D.Y);
    }

    public static string ReadString(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var finalString = "";
        for (var i = 0; i < length; i++) finalString += reader.ReadChar();

        return finalString;
    }

    public static Vector3 ReadVec3(BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}