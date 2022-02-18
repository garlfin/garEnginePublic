using gESilk.engine.window;

namespace gESilk;

public static class Program
{
    public static Window? MainWindow;
    
    static void Main()
    {
        MainWindow = new(1280, 720, "garEngine2");
        MainWindow.Run();
    }
}