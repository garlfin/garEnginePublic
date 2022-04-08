using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Transform : Component
{
    public Vector3 Location = Vector3.Zero;

    public Matrix4 Model;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;

    public Vector3 Front
    {
        get
        {
            Vector3 front;
            front.X = MathF.Cos(Rotation.Y) * MathF.Cos(Rotation.X);
            front.Y = MathF.Sin(Rotation.Y);
            front.Z = MathF.Cos(Rotation.Y) * MathF.Sin(Rotation.X);
            return front.Normalized();
        }
    }
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
    public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Front));
    
    public Transform()
    {
        TransformSystem.Register(this);
    }

    public override void Update(float gameTime)
    {
        Model = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z)) * Matrix4.CreateTranslation(Location);
    }
}

internal class TransformSystem : BaseSystem<Transform>
{
}