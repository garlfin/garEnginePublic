using gESilk.engine.assimp;
using gESilk.engine.render;
using gESilk.engine.window;

namespace gESilk;

public static class Program
{
    private static GameWindow _window;
    
    static void Main(string[] args)
    {
        Mesh loader = AssimpLoader.GetMeshFromFile("../../../cube.obj");

        Material material = new();
        material.AddSetting(new ShaderSetting<int>("blah", 10));
        material.Use();

        //_window = new(1280, 720, "garEngineSilk");
        //_window.Run();

    }
}