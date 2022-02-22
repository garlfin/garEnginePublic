using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Camera : BaseCamera
{
    public Camera(float fov, float clipStart, float clipEnd)
    {
        _fov = fov;
        _clipStart = clipStart;
        _clipEnd = clipEnd;
        CameraSystem.Register(this);
        _camera = new BasicCamera(Vector3.Zero, (float)1280 / 720)
        {
            DepthFar = clipEnd,
            DepthNear = clipStart
        };
    }

    public override void Update(float gameTime)
    {
        var entityTransform = Entity.GetComponent<Transform>();
        _camera.Fov = _fov;
        _camera.Position = entityTransform.Location;
        _camera.Yaw = entityTransform.Rotation.Y;
        _camera.Pitch = entityTransform.Rotation.X;
        View = _camera.GetViewMatrix();
        Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), (float)1280 / 720,
            _clipStart, _clipEnd);
    }
}