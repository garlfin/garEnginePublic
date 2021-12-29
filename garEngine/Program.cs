using garEngine.render;

namespace garEngine;

public static class Program
{
    public static void Main(string[] args)
    {
        // Window

        Window window = new Window(1280, 720, "GarEngine Window");
        window._window.Run();
    }
}
