using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.render.materialSystem;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets;

public class Mesh
{
    private readonly List<MeshData> _meshes = new();
    private bool _isSkybox;
    private int _materialCount;


    public void IsSkybox(bool value)
    {
        _isSkybox = value;
    }

    public void Render()
    {
        foreach (var mesh in _meshes) mesh.Data?.Render();
    }

    public void Render(List<Material> materials, Matrix4 model)
    {
        foreach (var mesh in _meshes)
        {
            materials[mesh.MaterialId].Use(_isSkybox, model, null);
            mesh.Data?.Render();
            materials[mesh.MaterialId].Cleanup();
        }
    }

    public void Render(Material material, Matrix4 model)
    {
        material.Use(_isSkybox, model, null);
        foreach (var mesh in _meshes) mesh.Data?.Render();
        material.Cleanup();
    }


    public void Render(List<Material> materials, Matrix4 model, DepthFunction function, CubemapCapture? capture)
    {
        foreach (var mesh in _meshes)
        {
            materials[mesh.MaterialId].Use(_isSkybox, model, capture, function);
            mesh.Data?.Render();
            materials[mesh.MaterialId].Cleanup();
        }
    }

    public void Render(Material material, Matrix4 model, List<Material> materials)
    {
        for (var index = 0; index < _meshes.Count; index++)
        {
            var mesh = _meshes[index];
            material.Use(_isSkybox, model, null, materials[index].GetDepthFunction());
            mesh.Data?.Render();
            material.Cleanup();
        }
    }

    public void Render(Material material, Matrix4 model, DepthFunction function, bool doCull = true)
    {
        foreach (var mesh in _meshes)
        {
            material.Use(_isSkybox, model, null, function, doCull);
            mesh.Data?.Render();
            material.Cleanup();
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

    public void Render(int index, List<Material> materials, Matrix4 model, DepthFunction? function = null)
    {
        materials[_meshes[index].MaterialId].Use(_isSkybox, model, null, function);
        _meshes[index].Data?.Render();
        materials[_meshes[index].MaterialId].Cleanup();
    }

    public void Render(int index, Material material, Matrix4 model, DepthFunction? function = null)
    {
        material.Use(_isSkybox, model, null, function);
        _meshes[index].Data?.Render();
        material.Cleanup();
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