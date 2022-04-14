using gESilk.engine.components;

namespace gESilk.resources.Scripts;

public sealed class FPS : Behavior
{
    public override void UpdateRender(float gameTime)
    {
        Owner.GetComponent<TextRenderer>().Text = $"FPS: {Math.Round((1f / gameTime))}";
    }
}