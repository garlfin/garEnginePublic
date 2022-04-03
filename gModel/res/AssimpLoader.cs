using Assimp;

namespace gModel.res;

public struct IntVec3
{
    public int X;
    public int Y;
    public int Z;

    public IntVec3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public static class AssimpLoader
{
    private static readonly AssimpContext Context;

    static AssimpLoader()
    {
        Context = new AssimpContext();
    }

    public static Mesh GetMeshFromFile(string path,
        PostProcessSteps steps = PostProcessSteps.Triangulate | PostProcessSteps.OptimizeGraph |
                                 PostProcessSteps.OptimizeMeshes | PostProcessSteps.FindInvalidData |
                                 PostProcessSteps.CalculateTangentSpace)
    {
        var scene = Context.ImportFile(path, steps);
        var outMesh = new Mesh(scene.MeshCount, scene.MaterialCount);

        if (scene is null || !scene.HasMeshes) throw new Exception($"Error in mesh: {path}");
        for (var v = 0; v < scene.Meshes.Count; v++)
        {
            var mesh = scene.Meshes[v];
            var texCoord = new Vector2D[mesh.TextureCoordinateChannels[0].Count];
            for (int i = 0; i < texCoord.Length; i++)
            {
                var texCoord3D = mesh.TextureCoordinateChannels[0][i];
                texCoord[i] = new Vector2D(texCoord3D.X, texCoord3D.Y);
            }

            var faces = new IntVec3[mesh.Faces.Count];
            for (int i = 0; i < faces.Length; i++)
            {
                var face = mesh.Faces[i].Indices;
                faces[i] = new IntVec3(face[0], face[1], face[2]);
            }

            var tempMesh = new MeshData(mesh.Vertices.ToArray(), texCoord, mesh.Normals.ToArray(),
                mesh.Tangents.ToArray(), faces, mesh.MaterialIndex);
            outMesh.Meshes[v] = tempMesh;
        }

        return outMesh;
    }
}