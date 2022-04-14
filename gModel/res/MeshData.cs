using Assimp;

namespace gModel.res;

public readonly struct MeshData
{
    public readonly Vector3D[] Vert;
    public readonly Vector2D[] TexCoord;
    public readonly Vector3D[] Normal;
    public readonly Vector3D[] Tangent;
    public readonly IntVec3[] Faces;
    public readonly string MaterialId;

    public MeshData(Vector3D[] vert, Vector2D[] texCoord, Vector3D[] normal, Vector3D[] tangent, IntVec3[] faces,
        string materialId)
    {
        Vert = vert;
        TexCoord = texCoord;
        Normal = normal;
        Tangent = tangent;
        Faces = faces;
        MaterialId = materialId;
    }
    
}

public readonly struct Mesh
{
    public readonly MeshData[] Meshes;

    public Mesh(int meshLength, int matCount)
    {
        Meshes = new MeshData[meshLength];
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write((ushort) 2); // Build Version
        
        writer.Write(Meshes.Length);

        foreach (var item in Meshes)
        {
            writer.Write(item.Vert.Length);
            foreach (var vector in item.Vert)
            {
                MathHelper.WriteVec3(writer, vector);
            }

            writer.Write(item.TexCoord.Length);
            foreach (var vector in item.TexCoord)
            {
                MathHelper.WriteVec2(writer, vector);
            }

            writer.Write(item.Normal.Length);
            foreach (var vector in item.Normal)
            {
                MathHelper.WriteVec3(writer, vector);
            }

            writer.Write(item.Tangent.Length);
            foreach (var vector in item.Tangent)
            {
                MathHelper.WriteVec3(writer, vector);
            }

            writer.Write(item.Faces.Length);
            foreach (var vector in item.Faces)
            {
                MathHelper.WriteVec3(writer, vector);
            }

            writer.Write(item.MaterialId);
        }
    }
}