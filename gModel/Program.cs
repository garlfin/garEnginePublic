using System.Text;
using Assimp;
using gModel.res;

namespace gModel;

internal static class Program
{
    static void Main(string[] args)
    {
        // I don't see the need to use classes here.
        var file = args[0];
        var fileNoExtension = Path.GetFileNameWithoutExtension(file);
        var fileName = Path.GetFileName(file);

        if (!File.Exists(file)) throw new ArgumentException($"File {fileName} doesn't exist!");
        if (!Directory.Exists("output")) Directory.CreateDirectory("output");

        var mesh = AssimpLoader.GetMeshFromFile(file);

        using var stream = File.Open($"output/{fileNoExtension}.gmod", FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        writer.Write(new[] { 'G', 'M', 'O', 'D' });
        writer.Write((short)0); // Build Version
        writer.Write(mesh.MatCount);

        foreach (var item in mesh.Meshes)
        {
            writer.Write(item.Vert.Length);
            foreach (var vector in item.Vert)
            {
                WriteVec3(writer, vector);
            }

            writer.Write(item.TexCoord.Length);
            foreach (var vector in item.TexCoord)
            {
                WriteVec2(writer, vector);
            }

            writer.Write(item.Normal.Length);
            foreach (var vector in item.Normal)
            {
                WriteVec3(writer, vector);
            }

            writer.Write(item.Tangent.Length);
            foreach (var vector in item.Tangent)
            {
                WriteVec3(writer, vector);
            }

            writer.Write(item.Faces.Length);
            foreach (var vector in item.Faces)
            {
                WriteVec3(writer, vector);
            }

            writer.Write((byte)item.MaterialId);
        }
    }

    public static void WriteVec3(BinaryWriter stream, Vector3D vector3D)
    {
        stream.Write(vector3D.X);
        stream.Write(vector3D.Y);
        stream.Write(vector3D.Z);
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
}