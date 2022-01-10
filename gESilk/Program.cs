using gESilk.engine.window;

namespace gESilk;

public static class Program
{
    private static GameWindow _window;
    
    static void Main(string[] args)
    {

        _window = new(1280, 720, "garEngineSilk");
        _window.Run();

    }
}