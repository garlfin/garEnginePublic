using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Camera : Component
{
    private float _fov;
    private float _clipStart;
    private float _clipEnd;


    private readonly BasicCamera _camera;
    public Matrix4 View, Projection;

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

    public BasicCamera GetBasicCamera()
    {
        return _camera;
    }

    public void Set()
    {
        CameraSystem.CurrentCamera = this;
    }

    public override void Update(float gameTime)
    {
        var entityTransform = Entity.GetComponent<Transform>();
        _camera.Fov = _fov;
        _camera.Position = entityTransform.Location;
        _camera.Yaw = entityTransform.Rotation.Y;
        _camera.Pitch = entityTransform.Rotation.X;
        View = _camera.GetViewMatrix();
        Projection = _camera.GetProjectionMatrix();
    }
}

internal class CameraSystem : BaseSystem<Camera>
{
    public static Camera CurrentCamera;
}