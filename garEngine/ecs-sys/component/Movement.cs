using System.Data;
using System.Text.Json.Serialization.Metadata;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace garEngine.ecs_sys.component;

public class Movement : Component
{
    private float speed = 50;
    private KeyboardState input;
    public override void Update(float gameTime)
    {
        //input = 
        //entity.GetComponent<Transform>().Location 
    }
}