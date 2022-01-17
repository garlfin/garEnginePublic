using Silk.NET.Maths;

namespace gESilk.engine.components;

public class Transform : Component
{
    public Vector3D<float> Location { get; set; } = Vector3D<float>.Zero;
    public Vector3D<float> Rotation { get; set; } = Vector3D<float>.Zero;
    public Vector3D<float> Scale { get; set; } = Vector3D<float>.One;

    protected Transform()
    {
        TransformSystem.Register(this);
    }
}

abstract class TransformSystem : BaseSystem<Transform> { }