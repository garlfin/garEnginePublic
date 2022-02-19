using gESilk.engine.misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = gESilk.engine.window.Window;
using static gESilk.Program;

namespace gESilk.engine.components;

public class Camera : Component
{
    public float Fov { get; set; } // Radians (Vertical not horizontal)
    public float ClipStart { get; set; }
    public float ClipEnd { get; set; }
    
   
    
    private Vector2 _lastPos = Vector2.Zero;
    
    private bool _firstMove = true;


   

   

    private BasicCamera _camera;
    public Matrix4 View, Projection;

    public Camera(float fov, float clipStart, float clipEnd)
    {
        Fov = fov;
        ClipStart = clipStart;
        ClipEnd = clipEnd;
        CameraSystem.Register(this);
        _camera = new BasicCamera(Vector3.Zero, (float) 1280/720);
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
        Transform? _entityTransform = Entity.GetComponent<Transform>();
        _camera.Fov = Fov;
        _camera.Position = _entityTransform.Location;
        _camera.Yaw = _entityTransform.Rotation.Y;
        _camera.Pitch = _entityTransform.Rotation.X;
        View = _camera.GetViewMatrix();
        Projection = _camera.GetProjectionMatrix();
    }

}



internal class CameraSystem : BaseSystem<Camera>
{
    public static Camera CurrentCamera;
}