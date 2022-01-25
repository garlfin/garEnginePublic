using static gESilk.engine.Globals;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Texture = gESilk.engine.render.Texture;

namespace gESilk.engine.window;

public class Window
{
    private GameWindowSettings gws;
    private NativeWindowSettings nws;
    private GameWindow _window;

    private double prevTime;
    private double _deltaTime = 0.0;

    public Window(int width, int height, string name)
    {
        gws = GameWindowSettings.Default;
        // Setup
        gws.RenderFrequency = 144;
        gws.UpdateFrequency = 144;
        gws.IsMultiThreaded = true;
        
        nws = NativeWindowSettings.Default;
        // Setup
        nws.APIVersion = Version.Parse("4.6");
        nws.Size = new Vector2i(width, height);
        nws.Title = name;
        nws.IsEventDriven = false;
        _window = new GameWindow(gws, nws);
    }

    public void Run()
    {
            _window.Load += OnLoad;
            _window.UpdateFrame += OnUpdate;
            _window.RenderFrame += OnRender;
            _window.Run();
    }

    private protected virtual void OnRender(FrameEventArgs args)
    {
        GL.Clear((ClearBufferMask) 16640); // Precalculated enum for Color and Depth Buffer Bit
        ModelRendererSystem.Update((float) args.Time);
        _window.SwapBuffers();
        
    }
    
    private protected virtual void OnUpdate(FrameEventArgs args)
    {
        _deltaTime = args.Time;
        
    }

    private protected virtual void OnLoad()
    {
        GL.ClearColor(System.Drawing.Color.Aqua);
        GL.Enable(EnableCap.DepthTest);
        
        Mesh loader = AssimpLoader.GetMeshFromFile("../../../cube.obj");

        ShaderProgram program = new ShaderProgram("../../../default.shader");

        Texture texture = new Texture("../../../sponza_column_a_diff.png", 1);
        Material material = new(program);
        material.AddSetting(new ShaderSetting<Texture>("albedo", texture));

        Entity entity = new();
        entity.AddComponent(new Transform());
        entity.AddComponent(new MaterialComponent(loader, material));
        entity.AddComponent(new ModelRenderer(loader));

        Entity camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new Camera(30f, 0.1f, 1000f, 0.3f));
    }
    
    
}