using garEngine.render;
using garEngine.render.model;

namespace garEngine;

public static class Program
{
    public static void Main(string[] args)
    {
        // Window

        Window window = new Window(1280, 720, "GarEngine Window");
        window._window.Run();
        //AssimpLoaderTest test = new AssimpLoaderTest("../../../resources/model/plane.dae");
    }
}
