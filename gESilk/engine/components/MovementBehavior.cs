using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

#pragma warning disable CS8602

namespace gESilk.engine.components;

public class MovementBehavior : Behavior
{
    private readonly float _cameraSpeed;
    private readonly float _sensitivity;

    public MovementBehavior(float sensitivity = 1f, float cameraSpeed = 4f)
    {
        _sensitivity = sensitivity * 0.1f;
        _cameraSpeed = cameraSpeed;
    }

    public override void Update(float gameTime)
    {
        var entityTransform = Owner.GetComponent<Transform>();
        var camera = Owner.GetComponent<Camera>().GetBasicCamera();

        var input = Owner.Application.GetWindow().KeyboardState.GetSnapshot();
        if (input.IsKeyDown(Keys.W)) entityTransform.Location += camera.Front * _cameraSpeed * gameTime; // Forward
        if (input.IsKeyDown(Keys.S)) entityTransform.Location -= camera.Front * _cameraSpeed * gameTime; // Backwards
        if (input.IsKeyDown(Keys.A)) entityTransform.Location -= camera.Right * (_cameraSpeed / 2) * gameTime; // Left
        if (input.IsKeyDown(Keys.D)) entityTransform.Location += camera.Right * (_cameraSpeed / 2) * gameTime; // Right
        if (input.IsKeyDown(Keys.Space)) entityTransform.Location += camera.Up * _cameraSpeed * gameTime; // Up
        if (input.IsKeyDown(Keys.C)) entityTransform.Location -= camera.Up * _cameraSpeed * gameTime; // Down
    }

    public override void UpdateMouse(MouseMoveEventArgs args)
    {
        var entityTransform = Owner.GetComponent<Transform>();
        entityTransform.Rotation.Y += args.DeltaX * _sensitivity;
        entityTransform.Rotation.X =
            Math.Clamp(entityTransform.Rotation.X - args.DeltaY * _sensitivity, -90,
                90); // Reversed since y-coordinates range from bottom to top
    }
}