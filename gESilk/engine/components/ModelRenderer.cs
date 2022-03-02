using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private readonly Mesh _mesh;
    private readonly bool _isStatic;

    public ModelRenderer(Mesh mesh, bool isStatic = true)
    {
        ModelRendererSystem.Register(this);
        _mesh = mesh;
        _isStatic = isStatic;
    }

    public override void Update(float gameTime)
    {
        var _modelTransform = Owner.GetComponent<Transform>();

        var state = Owner.Application.State();

        if (state is EngineState.RenderState)
        {
            _mesh.Render(Owner.GetComponent<MaterialComponent>()?.GetMaterials(),
                _modelTransform?.Model ?? Matrix4.Identity, DepthFunction.Equal);
        }
        else if (state is EngineState.RenderShadowState or EngineState.RenderDepthState)
        {
            _mesh.Render(Globals.DepthMaterial, _modelTransform?.Model ?? Matrix4.Identity,
                Owner.GetComponent<MaterialComponent>()?.GetMaterials());
        }
        else if (state is EngineState.GenerateCubemapState or EngineState.GenerateSkyboxState)
        {
            if (!_isStatic) return;
            _mesh.Render(Owner.GetComponent<MaterialComponent>()?.GetMaterials(),
                _modelTransform?.Model ?? Matrix4.Identity);
        }
    }

    public override void UpdateMouse(MouseMoveEventArgs args)
    {
    }
}

internal class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}