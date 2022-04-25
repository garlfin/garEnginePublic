using Assimp;
using gESilk.engine.render.assets;
using OpenTK.Mathematics;
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

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }

        _scene = Globals.Assimp.ImportFile(path, steps);

        if (_scene is null || _scene.HasMeshes == false) throw new Exception($"Error in mesh: {path}");
        foreach (var mesh in _scene.Meshes)
        {
            MeshData tempMesh = new();
            foreach (var vert in mesh.Faces)
                if (vert.IndexCount == 3)
                    tempMesh.Faces.Add(new IntVec3(vert.Indices[0], vert.Indices[1], vert.Indices[2]));
            foreach (var vert in mesh.TextureCoordinateChannels[0]) tempMesh.TexCoord.Add(new Vector2(vert.X, vert.Y));

            tempMesh.Vert = mesh.Vertices.Vec3DtoVec3();
            tempMesh.Normal = mesh.Normals.Vec3DtoVec3();
            tempMesh.Tangent = mesh.Tangents.Vec3DtoVec3();
            tempMesh.MaterialId = mesh.MaterialIndex;
            tempMesh.Data = new VertexArray(tempMesh);
            outMesh.AddMesh(tempMesh);
        }

        outMesh.SetMatCount(_scene.MaterialCount);
        return outMesh;
    }

    public static List<Vector3> Vec3DtoVec3(this List<Vector3D> vector)
    {
        List<Vector3> outVec = new List<Vector3>();

        foreach (var vector3D in vector)
        {
            outVec.Add(new Vector3(vector3D.X, vector3D.Y, vector3D.Z));
        }

        return outVec;
    }
}