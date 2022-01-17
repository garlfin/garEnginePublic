using gESilk.engine.assimp;
using gESilk.engine.render;

namespace gESilk.engine.components;

public class MaterialComponent : Component
{
    private List<Material> _materials = new List<Material>();

    public MaterialComponent(Mesh mesh, Material defaultMaterial)
    {
        for (int i = 0; i < mesh.Length(); i++)
        {
            _materials.Add(defaultMaterial);
        }
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

}