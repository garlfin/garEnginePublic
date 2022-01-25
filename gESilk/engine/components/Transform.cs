using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Transform : Component
{
    public Vector3 Location { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public Transform()
    {
        TransformSystem.Register(this);
    }
}

class TransformSystem : BaseSystem<Transform> { }