using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;

namespace gESilk.engine.window;

public class Window
{
    private IWindow _Window;
    private WindowOptions _Options = WindowOptions.Default;

    public Window(int width, int height, string name)
    {
        _Options.Size = new(width, height);
        _Options.Title = name;
        _Options.API = GraphicsAPI.Default;
        _Options.FramesPerSecond = 0;
        _Window = Silk.NET.Windowing.Window.Create(_Options);
    }

    public void Run()
    {
        _Window.Run();
        _Window.Load += OnLoad;
        _Window.Update += OnUpdate;
        _Window.Render += OnRender;
    }

    public virtual void OnRender(double obj)
    {
        
    }
    
    public virtual void OnUpdate(double obj)
    {
        
    }
    
    public virtual void OnLoad()
    {
        IInputContext input = _Window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }
    }

    public virtual void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        switch (arg2)
        {
            case Key.Escape:
                _Window.Close();
                break;
        }
    }
    
}