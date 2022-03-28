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

        var state = Owner.Application.State();

        if (state is EngineState.RenderState)
        {
            _mesh.Render(Owner.GetComponent<MaterialComponent>().GetMaterials(), modelTransform.Model,
                DepthFunction.Equal, Owner.IsStatic ? Owner.GetComponent<MaterialComponent>().SkyboxTexture : null);
        }
        else if (state is EngineState.GenerateCubemapState or EngineState.GenerateSkyboxState
                 or EngineState.IterationCubemapState)
        {
            if (!Owner.IsStatic) return;
            _mesh.Render(Owner.GetComponent<MaterialComponent>().GetMaterials(), modelTransform.Model,
                DepthFunction.Less, Owner.IsStatic ? Owner.GetComponent<MaterialComponent>().SkyboxTexture : null);
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