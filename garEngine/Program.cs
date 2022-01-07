using garEngine.render;
using garEngine.render.window;

namespace garEngine;

public static class Program
{
    public static void Main(string[] args)
    {
        // Window
        Window window = new Window(1280, 720, "GarEngine Window", 144);
        window._window.Run();
    }
}
