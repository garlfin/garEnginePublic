using System.Numerics;
using garEngine.ecs_sys.system;

namespace garEngine.ecs_sys.component;

public class Transform : Component
{
    public OpenTK.Mathematics.Vector3 Location { get; set; } = OpenTK.Mathematics.Vector3.Zero;
    public OpenTK.Mathematics.Vector3 Rotation { get; set; } = OpenTK.Mathematics.Vector3.Zero;
    public OpenTK.Mathematics.Vector3 Scale { get; set; } = OpenTK.Mathematics.Vector3.One;
    public Transform()
    {
        TransformSystem.Register(this);
}
}