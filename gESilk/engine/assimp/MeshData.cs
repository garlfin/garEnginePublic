using Assimp;
using gESilk.engine.render.assets;

namespace gESilk.engine.assimp;

public struct MeshData
{
    public MeshData()
    {
        Vert = new List<Vector3D>();
        TexCoord = new List<Vector2D>();
        Normal = new List<Vector3D>();
        Tangent = new List<Vector3D>();
        Faces = new List<IntVec3>();
        MaterialId = 0;
        Data = null;
    }

    public List<Vector3D> Vert;
    public List<Vector2D> TexCoord;
    public List<Vector3D> Normal;
    public List<Vector3D> Tangent;
    public List<IntVec3> Faces;
    public int MaterialId;
    public VertexArray? Data;
}