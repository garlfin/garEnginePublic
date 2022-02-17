using gESilk.engine.misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace gESilk.engine.components;

public class Camera : Component
{
    public float Fov { get; set; } // Radians (Vertical not horizontal)
    public float ClipStart { get; set; }
    public float ClipEnd { get; set; }
    
    private readonly float _cameraSpeed = 4;
    
    private Vector2 _lastPos = Vector2.Zero;
    
    private bool _firstMove = true;


    private Transform _entityTransform;

    private float _sensitivity;

    private BasicCamera _camera;
    public Matrix4 View, Projection;

    public Camera(float fov, float clipStart, float clipEnd, float sensitivity)
    {
        _sensitivity = sensitivity * 0.1f;
        Fov = fov;
        ClipStart = clipStart;
        ClipEnd = clipEnd;
        CameraSystem.Register(this);
        _camera = new BasicCamera(Vector3.Zero, (float) 1280/720);
    }

    public void Set()
    {
        CameraSystem.CurrentCamera = this;
    }

    public override void Update(float gameTime)
    {
       _entityTransform = Entity.GetComponent<Transform>();
        var input = Globals.Window.KeyboardState.GetSnapshot();
        if (input.IsKeyDown(Keys.W)) _entityTransform.Location += _camera.Front * _cameraSpeed * gameTime; // Forward
        if (input.IsKeyDown(Keys.S)) _entityTransform.Location -= _camera.Front * _cameraSpeed * gameTime; // Backwards
        if (input.IsKeyDown(Keys.A)) _entityTransform.Location -= _camera.Right * (_cameraSpeed / 2) * gameTime; // Left
        if (input.IsKeyDown(Keys.D)) _entityTransform.Location += _camera.Right * (_cameraSpeed / 2) * gameTime; // Right
        if (input.IsKeyDown(Keys.Space)) _entityTransform.Location += _camera.Up * _cameraSpeed * gameTime; // Up
        if (input.IsKeyDown(Keys.C)) _entityTransform.Location -= _camera.Up * _cameraSpeed * gameTime; // Down

        _camera.Fov = Fov;
        _camera.Position = _entityTransform.Location;
        

    }

    public void UpdateMatrices()
    {
        View = _camera.GetViewMatrix();
        Projection = _camera.GetProjectionMatrix();
    }

    public override void UpdateMouse()
    {
        _entityTransform = Entity.GetComponent<Transform>();
        var mouse = Globals.Window.MousePosition;
        if (_firstMove) // This bool variable is initially set to true.
        {
            _lastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = new Vector2(mouse.X, mouse.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            _entityTransform.Rotation.Y += deltaX * _sensitivity;
            _entityTransform.Rotation.X -= deltaY * _sensitivity; // Reversed since y-coordinates range from bottom to top
            _camera.Yaw = _entityTransform.Rotation.Y;
            _camera.Pitch = _entityTransform.Rotation.X;
        }
    }
}



internal class CameraSystem : BaseSystem<Camera>
{
    public static Camera CurrentCamera;

    public static void UpdateCamera()
    {
        foreach (var camera in Components)
        {
            camera.UpdateMatrices();
        }
    }
    
}