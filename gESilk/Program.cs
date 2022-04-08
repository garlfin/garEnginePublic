using gESilk.engine.window;

namespace gESilk;

public static class Program
{
    public static Application? MainWindow { get; private set; }

    private static void Main(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            args[i] = args[i].ToLower();
        }
        MainWindow = new Application(1280, 720, "gE2");
        MainWindow.MotionBlur = !args.Contains("-noblur");
        MainWindow.Run();
    }
}