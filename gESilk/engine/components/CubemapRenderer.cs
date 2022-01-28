using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class CubemapRenderer : Component
{
    private readonly Mesh _mesh;

    public CubemapRenderer(Mesh mesh)
    {
        CubemapMManager.Register(this);
        _mesh = mesh;
    }
    
    public override void Update(bool isShadow)
    {
        _mesh.Render(Entity.GetComponent<MaterialComponent>().GetMaterial(0), Matrix4.Identity);
    }
    public override void Update(float gameTime)
    {
        _mesh.Render(Entity.GetComponent<MaterialComponent>().GetMaterial(0), Matrix4.Identity);
    }
}
class CubemapMManager : BaseSystem<CubemapRenderer>
{
}