using Assimp;
using gESilk.engine.render;
using Silk.NET.Maths;
using static gESilk.engine.Globals;

namespace gESilk.engine.assimp;

public class Material
{
    private ShaderProgram _program;
    private readonly List<Setting> _settings = new();


    public Material()
    {
    }

    public void Use()
    {
        foreach (var setting in _settings)
        {
            setting.Use();
        }
    }

    public void AddSetting(Setting setting)
    {
        _settings.Add(setting);
    }
}

public interface Setting
{
    public void Use()
    {
    }
}
public class ShaderSetting<T> : Setting
{
    private string _uniformName;
    private T _value;
    public ShaderSetting(string name, T reference)
    {
        _uniformName = name;
        _value = reference;
    }

    public void Use()
    {
        if (typeof(T) == typeof(int))
        {
            Console.WriteLine((int)((object)_value ?? 0));
            //gl.ProgramUniform1(program.Get(), gl.GetUniformLocation(program.Get(), _uniformName), (int) (object) _value);        
        }
    }
}


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
    }
    
    public List<Vector3D<float>> Vert;
    public List<Vector2D<float>> TexCoord;
    public List<Vector3D<float>> Normal;
    public List<Vector3D<float>> Tangent;
    public List<Vector3D<int>> Faces;
    public int MaterialId = 0;
}

public class Mesh
{
    private List<MeshData> _meshes = new();

    public void AddMesh(MeshData mesh)
    {
        _meshes.Add(mesh);
    }
    
}

public static class AssimpLoader
{
    private static Scene _scene;

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
                tempMesh.Vert.Add(new(vert.X, vert.Y, vert.Z));
            }
            foreach (var vert in mesh.Normals)
            {
                tempMesh.Normal.Add(new(vert.X, vert.Y, vert.Z));
            }
            foreach (var vert in mesh.Tangents)
            {
                tempMesh.Tangent.Add(new(vert.X, vert.Y, vert.Z));
            }
            foreach (var vert in mesh.Faces)
            {
                tempMesh.Faces.Add(new(vert.Indices[0],vert.Indices[1],vert.Indices[2]));
            }
            foreach (var vert in mesh.TextureCoordinateChannels[0])
            {
                tempMesh.TexCoord.Add(new(vert.X, vert.Y));
            }

            tempMesh.MaterialId = _scene.MaterialCount;
            outMesh.AddMesh(tempMesh);

        }
        
        return outMesh;
        
    }
}