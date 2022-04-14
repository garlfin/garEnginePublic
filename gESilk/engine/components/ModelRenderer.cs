using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    public Mesh Mesh;

    public ModelRenderer()
    {
        ModelRendererSystem.Register(this);
    }

    public override void Update(float gameTime)
    {
        var modelTransform = Owner.GetComponent<Transform>();

        var state = Owner.Application.AppState;

        if (state != EngineState.RenderLinearShadowState && state != EngineState.RenderShadowState &&
            state != EngineState.RenderDepthState)
        {
            if (!Owner.IsStatic && state is not EngineState.RenderState) return;
            Mesh.Render(Owner.GetComponent<MaterialComponent>().GetMaterials(), modelTransform.Model,
                state is EngineState.RenderState ? DepthFunction.Equal : DepthFunction.Less,
                Owner.IsStatic ? Owner.GetComponent<MaterialComponent>().SkyboxTexture : null);
        }
        else
        {
            Mesh.Render(
                state is EngineState.RenderLinearShadowState ? Globals.LinearDepthMaterial : Globals.DepthMaterial,
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