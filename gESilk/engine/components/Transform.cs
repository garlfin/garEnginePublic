using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Transform : Component
{
    public Vector3 Location = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;

    public Matrix4 Model;

    public Transform()
    {
        TransformSystem.Register(this);
    }

    public override void Update(float gameTime)
    {
        Model = Matrix4.CreateTranslation(Location) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z)) * Matrix4.CreateScale(Scale);
    }
}

class TransformSystem : BaseSystem<Transform>
{
}