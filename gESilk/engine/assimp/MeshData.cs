using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine.assimp;

public struct MeshData
{
    public MeshData()
    {
        Vert = new List<Vector3>();
        TexCoord = new List<Vector2>();
        Normal = new List<Vector3>();
        Tangent = new List<Vector3>();
        Faces = new List<IntVec3>();
        MaterialId = 0;
        Data = null;
    }

    public List<Vector3> Vert;
    public List<Vector2> TexCoord;
    public List<Vector3> Normal;
    public List<Vector3> Tangent;
    public List<IntVec3> Faces;
    public int MaterialId;
    public VertexArray? Data;
}