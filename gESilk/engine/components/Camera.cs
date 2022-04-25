using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Camera : BaseCamera
{
    public Camera()
    {
        _camera = new BasicCamera(Vector3.Zero, (float)1280 / 720)
        {
            DepthFar = _clipEnd,
            DepthNear = _clipStart
        };
    }

    public float Fov
    {
        get => _fov;
        set => _fov = value;
    }

    public float ClipStart
    {
        get => _clipStart;
        set => _clipStart = value;
    }

    public float ClipEnd
    {
        get => _clipEnd;
        set => _clipEnd = value;
    }

    public override void Update(float gameTime)
    {
        var entityTransform = Owner.GetComponent<Transform>();
        _camera.Fov = _fov;
        _camera.Position = entityTransform.Location;
        _camera.Yaw = entityTransform.Rotation.Y;
        _camera.Pitch = entityTransform.Rotation.X;
        View = _camera.GetViewMatrix();
        Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), (float)1280 / 720,
            _clipStart, _clipEnd);
    }

    public override void Activate()
    {
        CameraSystem.Register(this);
        Set();
    }
}