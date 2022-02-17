using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Transform : Component
{
    public Vector3 Location = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;

    public Transform()
    {
        TransformSystem.Register(this);
    }
}

internal class TransformSystem : BaseSystem<Transform>
{
}