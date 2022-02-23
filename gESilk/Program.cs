using gESilk.engine.window;

namespace gESilk;

public static class Program
{
    public static Application? MainWindow;

    private static void Main()
    {
        MainWindow = new Application(1280, 720, "gE2");
        MainWindow.Run();
    }
}