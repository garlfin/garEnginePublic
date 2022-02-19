using Assimp;
using gESilk.engine.render.assets;
using Mesh = gESilk.engine.render.assets.Mesh;

namespace gESilk.engine.assimp;

public struct IntVec3
{
    private int _x;
    private int _y;
    private int _z;

    public IntVec3(int x, int y, int z)
    {
        _x = x;
        _y = y;
        _z = z;
    }
}

public static class AssimpLoader
{
    private static Scene? _scene;

    public static Mesh GetMeshFromFile(string path,
        PostProcessSteps steps = PostProcessSteps.Triangulate | PostProcessSteps.OptimizeGraph |
                                 PostProcessSteps.OptimizeMeshes | PostProcessSteps.FindInvalidData |
                                 PostProcessSteps.CalculateTangentSpace)
    {
        Mesh outMesh = new();
        _scene = Globals.Assimp.ImportFile(path, steps);

        foreach (Assimp.Mesh? mesh in _scene.Meshes)
        {
            MeshData tempMesh = new();
            foreach (Face? vert in mesh.Faces)
                if (vert.IndexCount == 3)
                    tempMesh.Faces.Add(new IntVec3(vert.Indices[0], vert.Indices[1], vert.Indices[2]));
            foreach (Vector3D vert in mesh.TextureCoordinateChannels[0]) tempMesh.TexCoord.Add(new Vector2D(vert.X, vert.Y));

            tempMesh.Vert = mesh.Vertices;
            tempMesh.Normal = mesh.Normals;
            tempMesh.Tangent = mesh.Tangents;
            tempMesh.MaterialId = mesh.MaterialIndex;
            tempMesh.Data = new VertexArray(tempMesh);
            outMesh.AddMesh(tempMesh);
        }

        outMesh.SetMatCount(_scene.MaterialCount);
        return outMesh;
    }
}