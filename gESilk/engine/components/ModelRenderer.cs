using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using static gESilk.Program;

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
        Transform? _modelTransform = Entity.GetComponent<Transform>();

        var state = MainWindow.State();

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