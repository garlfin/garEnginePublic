using gESilk.engine.window;

namespace gESilk;

public static class Program
{
    public static Window? MainWindow;

    private static void Main()
    {
        MainWindow = new Window(1280, 720, "garEngine2");
        MainWindow.Run();
    }
}