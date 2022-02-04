using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private readonly Mesh _mesh;
    private Transform? _modelTransform;

    public ModelRenderer(Mesh mesh)
    {
        ModelRendererSystem.Register(this);
        _mesh = mesh;
    }

    private Matrix4 model;

    public override void Update(float gameTime)
    {
        _modelTransform = Entity?.GetComponent<Transform>();
        model = _modelTransform != null ? CreateModelMatrix() : Matrix4.Identity;
        _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials(), model);
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


    public override void Update(bool isShadow)
    {
        if (isShadow)
        {
            _modelTransform = Entity?.GetComponent<Transform>();
            model = _modelTransform != null ? CreateModelMatrix() : Matrix4.Identity;
        }
        _mesh.Render(Globals.DepthMaterial, model);
    }

    public override void UpdateMouse()
    {
    }
}

internal class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}