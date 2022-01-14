using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static gESilk.engine.Globals;
using System.Windows.Forms;

namespace gESilk.engine.window;

public class Window
{
    private IView _Window;
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
        try
        {
            _Window.Load += OnLoad;
            _Window.Update += OnUpdate;
            _Window.Render += OnRender;
            _Window.Run();
        } catch (GlfwException)
        {
            MessageBox.Show("Error starting OpenGL Window. This machine may not support OpenGL", "OpenGL Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private protected virtual void OnRender(double obj)
    {
        gl.Clear((ClearBufferMask) 16640); // Precalculated enum for Color and Depth Buffer Bit
    }
    
    private protected virtual void OnUpdate(double obj)
    {
        
    }
    
    private protected virtual void OnLoad()
    {
        Globals.gl = GL.GetApi(_Window);
        
        gl.ClearColor(System.Drawing.Color.Aqua);
        
        
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