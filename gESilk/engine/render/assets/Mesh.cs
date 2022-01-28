using gESilk.engine.assimp;
using gESilk.engine.render.materialSystem;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets;

public class Mesh
{
    private readonly List<MeshData> _meshes = new();
    private int _materialCount;
    private bool _isSkybox;

    public void IsSkybox(bool value)
    {
        _isSkybox = value;
    }
    
    public void Render(List<Material> materials, Matrix4 model)
    {
        for (var index = 0; index < _meshes.Count; index++)
        {
            var mesh = _meshes[index];
            materials[mesh.MaterialId].Use(model, _isSkybox);
            mesh.Data?.Render();
        }
    }

    public void Render(Material material, Matrix4 model)
    {
        foreach (var mesh in _meshes)
        {
            material.Use(model, _isSkybox);
            mesh.Data?.Render();
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
        materials[_meshes[index].MaterialId].Use(model, _isSkybox);
        _meshes[index].Data?.Render();
    }

    public void Render(int index, Material material, Matrix4 model)
    {
        material.Use(model, _isSkybox);
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