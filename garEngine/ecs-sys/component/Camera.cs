using garEngine.ecs_sys.system;
using OpenTK.Mathematics;
using garEngine.render;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace garEngine.ecs_sys.component;

public class Camera : Component
{
    public float fov { get; set; } = 4; // Radians (Vertical not horizontal)
    public float clipStart { get; set; } = 0.1f;
    public float clipEnd { get; set; } = 1000f;
    
    private Vector2 _lastPos = Vector2.Zero;
    private bool _firstMove = true;
    

    private float _cameraSpeed = 25;

    private Transform _entityTransform;
    
    private float _sensitivity = 1f;
    
    public Camera(float fov_, float clipStart_, float clipEnd_, float sensitivity)
    {
        _sensitivity = sensitivity * 0.1f;
        fov = fov_;
        clipStart = clipStart_;
        clipEnd_ = clipEnd_;
        CameraSystem.Register(this);
    }

    public void setCurrentCamera()
    {
        CameraSystem.currentCamera = this.entity;
    }
    public override void Update(float gameTime)
    {
        _entityTransform = this.entity.GetComponent<Transform>();
        var input = RenderView._Window.KeyboardState.GetSnapshot();
        if (input.IsKeyDown(Keys.W))
        {
            _entityTransform.Location += RenderView._camera.Front * _cameraSpeed * gameTime; // Forward
            
        }
        if (input.IsKeyDown(Keys.S))
        {
            _entityTransform.Location -= RenderView._camera.Front * _cameraSpeed * gameTime; // Backwards
        }
        if (input.IsKeyDown(Keys.A))
        {
            _entityTransform.Location -= RenderView._camera.Right * (_cameraSpeed/2) * gameTime; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            _entityTransform.Location += RenderView._camera.Right * (_cameraSpeed/2) * gameTime; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            _entityTransform.Location += RenderView._camera.Up * _cameraSpeed * gameTime; // Up
        }
        if (input.IsKeyDown(Keys.C))
        {
            _entityTransform.Location -= RenderView._camera.Up * _cameraSpeed * gameTime; // Down
        }
    }

    public override void UpdateMouse()
    {
        var mouse = RenderView._Window.MousePosition;
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
            RenderView._camera.Yaw += deltaX * _sensitivity;
            RenderView._camera.Pitch -= deltaY * _sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
        //if (RenderView._Window.IsFocused) // check to see if the window is focused  
        //{
        //    RenderView._Window..SetPosition(X + Width/2f, Y + Height/2f);
        //}
}