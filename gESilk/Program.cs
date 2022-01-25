using gESilk.engine;
using gESilk.engine.assimp;
using gESilk.engine.render;
using gESilk.engine.window;
using OpenTK.Windowing.Desktop;

namespace gESilk;

public static class Program
{
    private static Window _window;
    
    static void Main(string[] args)
    {
        _window = new(1280, 720, "garEngineSilk");
        _window.Run();
    }
}