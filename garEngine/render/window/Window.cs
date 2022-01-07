using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace garEngine.render.window;

public class Window
{
    private int _sizeX;
    private int _sizeY;
    private string _title;
    private int _fps;
    private GameWindowSettings gws;
    private NativeWindowSettings nws;
    public GameWindow _window;


    public Window(int sizeX, int sizeY, string title, int fps = 60)
    {
        _sizeX = sizeX;
        _sizeY = sizeY;
        _title = title;
        _fps = fps;

        gws = GameWindowSettings.Default;
        // Setup
        gws.RenderFrequency = _fps;
        gws.UpdateFrequency = _fps;
        gws.IsMultiThreaded = true;
        
        nws = NativeWindowSettings.Default;
        // Setup
        nws.APIVersion = Version.Parse("4.6");
        nws.Size = new Vector2i(_sizeX, _sizeY);
        nws.Title = _title;
        nws.IsEventDriven = false;
        _window = new MyWindow(gws, nws);
    }
}