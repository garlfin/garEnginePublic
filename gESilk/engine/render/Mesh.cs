using gESilk.engine.assimp;
using Silk.NET.Maths;

namespace gESilk.engine.render;

public class Mesh
{
    private List<MeshData> _meshes = new();

    public void Render(List<Material> materials, Matrix4X4<float> model)
    {
        foreach (var mesh in _meshes)
        {
            materials[mesh.MaterialId].Use(model);
            mesh.Data?.Render();
        }
    }

    public void Render(int index, List<Material> materials, Matrix4X4<float> model)
    {
        materials[_meshes[index].MaterialId].Use(model);
        _meshes[index].Data?.Render();
    }
    public void Render(int index, Material material, Matrix4X4<float> model)
    {
        material.Use(model);
        _meshes[index].Data?.Render();
    }

    public void AddMesh(MeshData mesh)
    {
        _meshes.Add(mesh);
    }

    public int Length()
    {
        return _meshes.Count;
    }
    
}