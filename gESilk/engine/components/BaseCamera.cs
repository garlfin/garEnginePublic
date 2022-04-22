using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public abstract class BaseCamera : Component
{
    protected BasicCamera _camera;
    protected float _clipEnd = 100f;
    protected float _clipStart = 0.1f;
    protected float _fov = 43f;
    public Matrix4 Projection, PreviousView = Matrix4.Identity;
    public Matrix4[] View = new Matrix4[6];

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

    public virtual void UpdatePreviousMatrix()
    {
        PreviousView = View[0];
    }
}

internal class CameraSystem : BaseSystem<Camera>
{
    public static BaseCamera CurrentCamera;
}