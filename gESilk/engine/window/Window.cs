using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static gESilk.engine.Globals;
using System.Windows.Forms;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.render;
using Texture = gESilk.engine.render.Texture;

namespace gESilk.engine.window;

public class Window
{
    private IView _window;
    private WindowOptions _options = WindowOptions.Default;

    private double prevTime;
    private double _deltaTime = 0.0;

    public Window(int width, int height, string name)
    {
        _options.Size = new(width, height);
        _options.Title = name;
        _options.API = GraphicsAPI.Default;
        _options.FramesPerSecond = 0;
        _window = Silk.NET.Windowing.Window.Create(_options);
    }

    public void Run()
    {
        try
        {
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Run();
        } catch (GlfwException)
        {
            MessageBox.Show("Error starting OpenGL Window. This machine may not support OpenGL", "OpenGL Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private protected virtual void OnRender(double obj)
    {
        gl.Clear((ClearBufferMask) 16640); // Precalculated enum for Color and Depth Buffer Bit
        ModelRendererSystem.Update(0.0f);
        
    }
    
    private protected virtual void OnUpdate(double obj)
    {
        _deltaTime = DeltaTime();
        
    }

    private protected virtual double DeltaTime()
    {
        double delta = prevTime - _window.Time;
        prevTime = _window.Time;
        return delta;
    }
    
    private protected virtual void OnLoad()
    {
        gl = GL.GetApi(_window);
        
        gl.ClearColor(System.Drawing.Color.Aqua);
        gl.Enable(EnableCap.DepthTest);
        
        Mesh loader = AssimpLoader.GetMeshFromFile("../../../cube.obj");

        ShaderProgram program = new ShaderProgram("../../../default.shader");

        Texture texture = new Texture("../../../cube.obj", 1);
        Material material = new(program);
        material.AddSetting(new ShaderSetting<Texture>("albedo", texture));

        Entity entity = new();
        entity.AddComponent(new ModelRenderer(loader));
        entity.AddComponent(new MaterialComponent(loader, material));

        IInputContext input = _window.CreateInput();
        foreach (var t in input.Keyboards)
        {
            t.KeyDown += KeyDown;
        }
    }

    public virtual void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        switch (arg2)
        {
            case Key.Escape:
                _window.Close();
                break;
        }
    }
    
}