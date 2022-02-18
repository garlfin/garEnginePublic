using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static gESilk.Program;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private readonly Mesh _mesh;
    private Transform? _modelTransform;
    private MaterialComponent? _materialComponent;

    public ModelRenderer(Mesh mesh)
    {
        ModelRendererSystem.Register(this);
        _mesh = mesh;
    }

    private Matrix4 model;

    public override void Update(float gameTime)
    {
        _modelTransform = Entity?.GetComponent<Transform>();
        EngineState state = MainWindow.State();
        
        if (state == EngineState.RenderState) {
            _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials(), model, DepthFunction.Equal);
            
        } else if (state is EngineState.RenderShadowState or EngineState.RenderDepthState) {
            if (state == EngineState.RenderShadowState) model = CreateModelMatrix();
            _mesh.Render(Globals.DepthMaterial, model, Entity.GetComponent<MaterialComponent>()?.GetMaterials());
        }
    }

    private float DegreesToRadians(float degrees)
    {
        return degrees * (  3.1415926535897931f / 180f);
    }
    
    private Matrix4 CreateModelMatrix()
    {
        if (_modelTransform != null)
            return Matrix4.CreateRotationX(DegreesToRadians(_modelTransform.Rotation.X)) *
                   Matrix4.CreateRotationY(DegreesToRadians(_modelTransform.Rotation.Y)) *
                   Matrix4.CreateRotationZ(DegreesToRadians(_modelTransform.Rotation.Z)) * 
                   Matrix4.CreateScale(_modelTransform.Scale) * Matrix4.CreateTranslation(_modelTransform.Location);
        return Matrix4.Identity;
    }
    

    public override void UpdateMouse(float gameTime)
    {
    }
}

class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}