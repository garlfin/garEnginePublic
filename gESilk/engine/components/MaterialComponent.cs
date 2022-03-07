using gESilk.engine.render.assets;
using gESilk.engine.render.materialSystem;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class MaterialComponent : Component
{
    private List<Material> _materials = new();
    public CubemapCapture SkyboxTexture;

    public MaterialComponent(Mesh mesh, Material defaultMaterial)
    {
        for (var i = 0; i < mesh.GetMatCount(); i++) _materials.Add(defaultMaterial);
    }

    public MaterialComponent(List<Material> materials)
    {
        _materials = materials;
    }

    public void ChangeMaterial(int index, Material material)
    {
        _materials[index] = material;
    }

    public void ChangeMaterial(List<Material> materials)
    {
        _materials = materials;
    }

    public List<Material> GetMaterials()
    {
        return _materials;
    }

    public Material GetMaterial(int index)
    {
        return _materials[index];
    }

    public void GetNearestCubemap()
    {
        SkyboxTexture = CubemapCaptureManager.GetNearest(Owner.GetComponent<Transform>()?.Location ?? Vector3.Zero);
    }
}