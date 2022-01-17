using gESilk.engine.render;

namespace gESilk.engine.assimp;

public struct MeshData
{

    public MeshData()
    {
        Vert = new();
        TexCoord = new();
        Normal = new();
        Tangent = new();
        Faces = new();
        MaterialId = 0;
        Data = null;
    }
    
    public List<float> Vert;
    public List<float> TexCoord;
    public List<float> Normal;
    public List<float> Tangent;
    public List<int> Faces;
    public int MaterialId = 0;
    public VertexArray? Data;
}