using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private readonly Application _application;
    private readonly Mesh _mesh;
    private readonly bool _isStatic;

    public ModelRenderer(Mesh mesh, Application application, bool isStatic = true)
    {
        ModelRendererSystem.Register(this);
        _application = application;
        _mesh = mesh;
        _isStatic = isStatic;
    }

    public override void Update(float gameTime)
    {
        var _modelTransform = Entity.GetComponent<Transform>();

        var state = _application.State();

        if (state is EngineState.RenderState)
        {
            _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials(),
                _modelTransform?.Model ?? Matrix4.Identity, DepthFunction.Equal);
        }
        else if (state is EngineState.RenderShadowState or EngineState.RenderDepthState)
        {
            _mesh.Render(Globals.DepthMaterial, _modelTransform?.Model ?? Matrix4.Identity,
                Entity.GetComponent<MaterialComponent>()?.GetMaterials());
        }
        else if (state is EngineState.GenerateCubemapState)
        {
            if (!_isStatic) return;
            _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials(),
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