using System.Windows.Forms;
using gESilk.engine.render;
using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class FBRenderer : Component
{
    private readonly Mesh _mesh;

    public FBRenderer(Mesh mesh)
    {
        FBRendererSystem.Register(this);
        _mesh = mesh;
    }
    
    public override void Update(float gameTime)
    {
        _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials(), Matrix4.Identity);
    }

    public override void UpdateMouse(MouseMoveEventArgs args)
    {
    }
}

internal class FBRendererSystem : BaseSystem<FBRenderer>
{
}