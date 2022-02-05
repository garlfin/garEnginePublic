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

    public Camera(float fov, float clipStart, float clipEnd, float sensitivity)
    {
        _sensitivity = sensitivity * 0.1f;
        Fov = fov;
        ClipStart = clipStart;
        ClipEnd = clipEnd;
        CameraSystem.Register(this);
    }

    public void Set()
    {
        CameraSystem.CurrentCamera = Entity;
    }

    public override void Update(float gameTime)
    {
        _entityTransform = Entity.GetComponent<Transform>();
        var input = Globals.Window.KeyboardState.GetSnapshot();
        if (input.IsKeyDown(Keys.W)) _entityTransform.Location += Globals.Camera.Front * _cameraSpeed * gameTime; // Forward
        if (input.IsKeyDown(Keys.S)) _entityTransform.Location -= Globals.Camera.Front * _cameraSpeed * gameTime; // Backwards
        if (input.IsKeyDown(Keys.A)) _entityTransform.Location -= Globals.Camera.Right * (_cameraSpeed / 2) * gameTime; // Left
        if (input.IsKeyDown(Keys.D))
            _entityTransform.Location += Globals.Camera.Right * (_cameraSpeed / 2) * gameTime; // Right
        if (input.IsKeyDown(Keys.Space)) _entityTransform.Location += Globals.Camera.Up * _cameraSpeed * gameTime; // Up
        if (input.IsKeyDown(Keys.C)) _entityTransform.Location -= Globals.Camera.Up * _cameraSpeed * gameTime; // Down
    }

    public override void UpdateMouse()
    {
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
            Globals.Camera.Yaw += deltaX * _sensitivity;
            Globals.Camera.Pitch -= deltaY * _sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
}

internal class CameraSystem : BaseSystem<Camera>
{
    public static Entity? CurrentCamera;

    public static void UpdateCamera()
    {
        Globals.Camera.Position = CurrentCamera.GetComponent<Transform>().Location;
        Globals.Camera.Fov = CurrentCamera.GetComponent<Camera>().Fov;
        Globals.Camera.AspectRatio = (float)1280 / 720;
        Globals.Camera.DepthFar = CurrentCamera.GetComponent<Camera>().ClipEnd;
        Globals.Camera.DepthNear = CurrentCamera.GetComponent<Camera>().ClipStart;
    }
}