using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public abstract class BaseCamera : Component
{
    protected BasicCamera _camera;
    protected float _clipEnd;
    protected float _clipStart;
    protected float _fov;
    public Matrix4 View, Projection;


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
    }
}

internal class CameraSystem : BaseSystem<Camera>
{
    public static BaseCamera CurrentCamera;
}