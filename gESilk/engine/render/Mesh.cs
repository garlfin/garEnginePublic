using gESilk.engine.assimp;
using OpenTK.Mathematics;

namespace gESilk.engine.render;

public class Mesh
{
    private List<MeshData> _meshes = new();
    private int materialCount;
    
    public void Render(List<Material> materials, Matrix4 model)
    {
        foreach (var mesh in _meshes)
        {
            materials[mesh.MaterialId].Use(model);
            mesh.Data.Render();
        }
    }

    public int GetMatCount()
    {
        return materialCount;
    }

    public void SetMatCount(int length)
    {
        materialCount = length;
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