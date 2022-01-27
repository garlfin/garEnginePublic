using gESilk.engine.assimp;
using gESilk.engine.render.materialSystem;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets;

public class Mesh
{
    private readonly List<MeshData> _meshes = new();
    private int _materialCount;

    public void Render(List<Material> materials, Matrix4 model)
    {
        foreach (var mesh in _meshes)
        {
            materials[mesh.MaterialId].Use(model);
            mesh.Data?.Render();
            materials[mesh.MaterialId].Cleanup();
        }
    }

    public int GetMatCount()
    {
        return _materialCount;
    }

    public void SetMatCount(int length)
    {
        _materialCount = length;
    }

    public void Render(int index, List<Material> materials, Matrix4 model)
    {
        materials[_meshes[index].MaterialId].Use(model);
        _meshes[index].Data?.Render();
    }

    public void Render(int index, Material material, Matrix4 model)
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