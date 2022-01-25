using gESilk.engine.assimp;
using gESilk.engine.render;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private Mesh _mesh;
    private Transform _modelTransform;

    public ModelRenderer(Mesh mesh)
    {
        ModelRendererSystem.Register(this);
        _mesh = mesh;
    }

    public override void Update(float gameTime)
    {
        _modelTransform = Entity.GetComponent<Transform>() ?? throw new InvalidOperationException();
        _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials()!, CreateModelMatrix());
    }
    
    private Matrix4 CreateModelMatrix()
    {
        return Matrix4.CreateRotationX( _modelTransform.Rotation.X) * Matrix4.CreateRotationY( _modelTransform.Rotation.Y) *
               Matrix4.CreateRotationZ( _modelTransform.Rotation.Z) * Matrix4.CreateScale(_modelTransform.Scale) *
               Matrix4.CreateTranslation(_modelTransform.Location);
    }
    
    
    public override void Update(bool isShadow)
    {
    }

    public override void UpdateMouse()
    {
    }
}

class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}
