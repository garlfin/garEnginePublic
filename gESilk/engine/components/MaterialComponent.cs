using gESilk.engine.render.assets;
using gESilk.engine.render.materialSystem;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class MaterialComponent : Component
{
    public List<Material> Materials = new();
    public CubemapCapture SkyboxTexture;
    public Mesh Mesh;
    public Material DefaultMaterial;

    public override void Activate()
    {
        for (int i = 0; i < Mesh.Length(); i++)
        {
            Materials.Add(DefaultMaterial);
        }
    }

    public void ChangeMaterial(int index, Material material)
    {
        Materials[index] = material;
    }

    public void ChangeMaterial(List<Material> materials)
    {
        Materials = materials;
    }

    public List<Material> GetMaterials()
    {
        return Materials;
    }

    public Material GetMaterial(int index)
    {
        return Materials[index];
    }

    public void GetNearestCubemap()
    {
        SkyboxTexture = CubemapCaptureManager.GetNearest(Owner.GetComponent<Transform>()?.Location ?? Vector3.Zero);
    }
}