using Assimp;

namespace gModel.res;

public readonly struct MeshData
{
    public readonly Vector3D[] Vert;
    public readonly Vector2D[] TexCoord;
    public readonly Vector3D[] Normal;
    public readonly Vector3D[] Tangent;
    public readonly IntVec3[] Faces;
    public readonly int MaterialId;

    public MeshData(Vector3D[] vert, Vector2D[] texCoord, Vector3D[] normal, Vector3D[] tangent, IntVec3[] faces,
        int materialId)
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
    public readonly int MatCount;

    public Mesh(int meshLength, int matCount)
    {
        MatCount = matCount;
        Meshes = new MeshData[meshLength];
    }
}