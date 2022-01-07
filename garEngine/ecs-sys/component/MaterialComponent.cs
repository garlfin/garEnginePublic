using garEngine.render;
using garEngine.render.model;

namespace garEngine.ecs_sys.component;

public class MaterialComponent : Component
{
    private Material[] _materials;

    public MaterialComponent(MeshObject meshObject, Material baseMaterial)
    {
        _materials = new Material[meshObject.Length()];
        for (int i = 0; i < meshObject.Length(); i++)
        {
            _materials[i] = baseMaterial;
        }
    }

    public void SetMaterial(int index, Material material)
    {
        _materials[index] = material;
    }

    public Material GetMaterial(int index)
    {
        return _materials[index];
    }
}