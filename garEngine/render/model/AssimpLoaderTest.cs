using Assimp;

namespace garEngine.render.model;

public static class assimpContextClass
{
    private static AssimpContext Context = new AssimpContext();

    public static AssimpContext get()
    {
        return Context;
    }
}

public class AssimpLoaderTest
{
    private Scene _scene;
    public struct Intvec3{
        int x;
        int y;
        int z;

        public Intvec3(int x_, int y_, int z_)
        {
            x = x_;
            y = y_;
            z = z_;
        }
    }

    public struct MeshStruct
    {
        public List<Vector3D> points;
        public List<Vector3D> normal;
        public List<Vector3D> tangents;
        public List<Vector2D> uvs;
        public List<Intvec3> faces;

    }

    private MeshStruct myMesh;
    public AssimpLoaderTest(string path, PostProcessSteps flags = PostProcessSteps.Triangulate | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.FindInvalidData )
    {
        _scene = assimpContextClass.get().ImportFile(path, flags);
        if (_scene.SceneFlags.HasFlag(SceneFlags.Incomplete))
        {
            throw new Exception("Error occurred in assimp");
        }
        if (!_scene.HasMeshes)
        {
            throw new Exception("No meshes in the file");
        }
        List<Intvec3> tmpfaces = new();
        foreach (var face in _scene.Meshes[0].Faces)
        {
            if (face.Indices.Count == 3)
            {
                tmpfaces.Add(new Intvec3(face.Indices[0], face.Indices[1], face.Indices[2]));
            }
        }

        List<Vector2D> tmpUvs = new();
        foreach (var uv in _scene.Meshes[0].TextureCoordinateChannels[0])
        {
            tmpUvs.Add(new Vector2D(uv.X, uv.Y));
        }
        myMesh = new MeshStruct()
        {
            points = _scene.Meshes[0].Vertices,
            normal = _scene.Meshes[0].Normals,
            faces = tmpfaces,
            tangents = _scene.Meshes[0].Tangents,
            uvs = tmpUvs
            
        };

    }

    public MeshStruct getMesh()
    {
        return myMesh;
    }




}