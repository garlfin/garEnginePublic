using System.Numerics;
using gESilk.engine.api;
using Silk.NET.Assimp;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGL;


namespace gESilk.engine.window;

public class Window
{
    private IView _Window;
    private WindowOptions _Options = WindowOptions.Default;
    private ViewOptions _ViewOptions = ViewOptions.Default;
    private bool isGL = true;
    private API gl;

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
        _Window.Load += OnLoad;
        _Window.Update += OnUpdate;
        _Window.Render += OnRender;
        try
        {
            _Window.Run();
        } catch (GlfwException)
        {
            isGL = false;
            Console.WriteLine("Falling back to OpenGLES");
            _ViewOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Compatability, ContextFlags.Default, new APIVersion(3, 0));
            _Window = Silk.NET.Windowing.Window.GetView(_ViewOptions);
            _Window.Load += OnLoad;
            _Window.Update += OnUpdate;
            _Window.Render += OnRender;
            _Window.Run();
        }
    }

    public virtual void OnRender(double obj)
    {
        
    }
    
    public virtual void OnUpdate(double obj)
    {
        
    }
    
    public virtual void OnLoad()
    {
        if (isGL)
        {
            gl = new GLAPI();
        }
        else
        {
            gl = new GLESAPI();
        }
        gl.GetApi(_Window);
        
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