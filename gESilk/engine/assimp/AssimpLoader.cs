using Assimp;
using gESilk.engine.render;
using Silk.NET.Maths;
using static gESilk.engine.Globals;
using Mesh = gESilk.engine.render.Mesh;
using Vector3D = Assimp.Vector3D;

namespace gESilk.engine.assimp;

public static class AssimpLoader
{
    private static Scene _scene;

    private static void AddXYZ(Vector3D vector3D, List<float> list, bool twoD = false)
    {
        list.Add(vector3D.X);
        list.Add(vector3D.Y);
        if (!twoD) list.Add(vector3D.Z);
    }

    public static Mesh GetMeshFromFile(string path,
        PostProcessSteps steps = PostProcessSteps.Triangulate | PostProcessSteps.OptimizeGraph |
                                 PostProcessSteps.OptimizeMeshes | PostProcessSteps.FindInvalidData |
                                 PostProcessSteps.CalculateTangentSpace)
    {
        Mesh outMesh = new();
        _scene = Globals.Assimp.ImportFile(path, steps);
        
        foreach (var mesh in _scene.Meshes)
        {
            MeshData tempMesh = new();
            foreach (var vert in mesh.Vertices)
            {
                AddXYZ(vert, tempMesh.Vert);
            }
            foreach (var vert in mesh.Normals)
            {
                AddXYZ(vert, tempMesh.Normal);
            }
            foreach (var vert in mesh.Tangents)
            {
                AddXYZ(vert, tempMesh.Tangent);
            }
            foreach (var vert in mesh.Faces)
            {
                tempMesh.Faces.Add(vert.Indices[0]);
                tempMesh.Faces.Add(vert.Indices[1]);
                tempMesh.Faces.Add(vert.Indices[2]);
            }
            foreach (var vert in mesh.TextureCoordinateChannels[0])
            {
                AddXYZ(vert, tempMesh.TexCoord, true);
            }

            tempMesh.MaterialId = _scene.MaterialCount;
            tempMesh.Data = new VertexArray(tempMesh);
            outMesh.AddMesh(tempMesh);

        }
        return outMesh;
        
    }
}