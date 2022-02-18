using gESilk.engine.misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace gESilk.engine.components;
using static Program;

public class MovementBehavior : Behavior
{
  
   
    private readonly float _sensitivity;
    private float _cameraSpeed;
    private bool _firstMove;
    private Vector2 _lastPos;

    public MovementBehavior(float sensitivity = 1f, float cameraSpeed = 4f)
    {
        _sensitivity = sensitivity * 0.1f;
        _cameraSpeed = cameraSpeed;
    }

    public override void Update(float gameTime)
    {
        var entityTransform = Entity.GetComponent<Transform>();
        var camera = Entity.GetComponent<Camera>().GetBasicCamera();
        
        var input = MainWindow.GetWindow().KeyboardState.GetSnapshot();
        if (input.IsKeyDown(Keys.W)) entityTransform.Location += camera.Front * _cameraSpeed * gameTime; // Forward
        if (input.IsKeyDown(Keys.S)) entityTransform.Location -= camera.Front * _cameraSpeed * gameTime; // Backwards
        if (input.IsKeyDown(Keys.A)) entityTransform.Location -= camera.Right * (_cameraSpeed / 2) * gameTime; // Left
        if (input.IsKeyDown(Keys.D)) entityTransform.Location += camera.Right * (_cameraSpeed / 2) * gameTime; // Right
        if (input.IsKeyDown(Keys.Space)) entityTransform.Location += camera.Up * _cameraSpeed * gameTime; // Up
        if (input.IsKeyDown(Keys.C)) entityTransform.Location -= camera.Up * _cameraSpeed * gameTime; // Down
    }

    public override void UpdateMouse(float gameTime)
    {
        var entityTransform = Entity.GetComponent<Transform>();
        var mouse = MainWindow.GetWindow().MousePosition;
        
        if (_firstMove) // This bool variable is initially set to true.
        {
            _lastPos = mouse;
            _firstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = mouse;
            
            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            entityTransform.Rotation.Y += deltaX * _sensitivity;
            entityTransform.Rotation.X -= deltaY * _sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
}