using gESilk.engine.render.assets;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class FbRenderer : Component
{
    private readonly Mesh _mesh;

    public FbRenderer(Mesh mesh)
    {
        FbRendererSystem.Register(this);
        _mesh = mesh;
    }

    public override void Update(float gameTime)
    {
        _mesh.Render(Owner.GetComponent<MaterialComponent>()?.GetMaterials(), Matrix4.Identity);
    }

    public override void UpdateMouse(MouseMoveEventArgs args)
    {
    }
}

internal class FbRendererSystem : BaseSystem<FbRenderer>
{
}