using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private readonly Mesh _mesh;

    public ModelRenderer(Mesh mesh)
    {
        ModelRendererSystem.Register(this);
        _mesh = mesh;
    }

    public override void Update(float gameTime)
    {
        var modelTransform = Owner.GetComponent<Transform>();

        var state = Owner.Application.AppState;

        if (state != EngineState.RenderPointShadowState && state != EngineState.RenderShadowState && state != EngineState.RenderDepthState)
        {
            if (!Owner.IsStatic && state is not EngineState.RenderState) return;
            _mesh.Render(Owner.GetComponent<MaterialComponent>().GetMaterials(), modelTransform.Model,
               state is EngineState.RenderState ? DepthFunction.Equal : DepthFunction.Less, Owner.IsStatic ? Owner.GetComponent<MaterialComponent>().SkyboxTexture : null);
        }
        else
        {
            _mesh.Render(
                state is EngineState.RenderPointShadowState ? Globals.LinearDepthMaterial : Globals.DepthMaterial,
                modelTransform.Model);
        }
    }

    public override void UpdateMouse(MouseMoveEventArgs args)
    {
    }
}

internal class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}