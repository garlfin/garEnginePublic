using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class ModelRenderer : Component
{
    private readonly Mesh _mesh;
    private Transform _modelTransform;

    public ModelRenderer(Mesh mesh)
    {
        ModelRendererSystem.Register(this);
        _mesh = mesh;
    }

    public override void Update(float gameTime)
    {
        _mesh.Render(Entity.GetComponent<MaterialComponent>()?.GetMaterials()!,
            _modelTransform != null ? CreateModelMatrix() : Matrix4.Identity);
    }

    private Matrix4 CreateModelMatrix()
    {
        return Matrix4.CreateRotationX(_modelTransform.Rotation.X) *
               Matrix4.CreateRotationY(_modelTransform.Rotation.Y) *
               Matrix4.CreateRotationZ(_modelTransform.Rotation.Z) * Matrix4.CreateScale(_modelTransform.Scale) *
               Matrix4.CreateTranslation(_modelTransform.Location);
    }


    public override void Update(bool isShadow)
    {
        _modelTransform = Entity.GetComponent<Transform>() ?? null;
        _mesh.Render(Globals.DepthMaterial,
            _modelTransform != null ? CreateModelMatrix() : Matrix4.Identity);
    }

    public override void UpdateMouse()
    {
    }
}

internal class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}