using static gESilk.engine.Globals;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Texture = gESilk.engine.render.assets.Texture;

namespace gESilk.engine.window;

public class Window
{
    private bool _alreadyClosed;

    public Window(int width, int height, string name)
    {
        var gws = GameWindowSettings.Default;
        // Setup
        gws.RenderFrequency = 144;
        gws.UpdateFrequency = 144;
        gws.IsMultiThreaded = true;

        var nws = NativeWindowSettings.Default;
        // Setup
        nws.APIVersion = Version.Parse("4.6");
        nws.Size = new Vector2i(width, height);
        nws.Title = name;
        nws.IsEventDriven = false;
        Globals.Window = new GameWindow(gws, nws);
    }

    public void Run()
    {
        Globals.Window.Load += OnLoad;
        Globals.Window.UpdateFrame += OnUpdate;
        Globals.Window.RenderFrame += OnRender;
        Globals.Window.MouseMove += OnMouseMove;
        Globals.Window.Run();
    }

    private protected virtual void OnMouseMove(MouseMoveEventArgs args)
    {
        CameraSystem.UpdateMouse();
    }

    private protected virtual void OnClosing()
    {
        Console.WriteLine();
        Console.WriteLine("Closing :)");
        TextureManager.Delete();
        MeshManager.Delete();
        ShaderProgramManager.Delete();
    }

    private protected virtual void OnRender(FrameEventArgs args)
    {
        CameraSystem.UpdateCamera();
        UpdateRender();

        GL.Clear((ClearBufferMask)16640); // Precalculated enum for Color and Depth Buffer Bit
        ModelRendererSystem.Update((float)args.Time);
        Globals.Window.SwapBuffers();

        if (!_alreadyClosed)
        {
            Console.Write("FPS: "+1.0/args.Time + new string(' ', Console.WindowWidth - args.Time.ToString().Length - 5));
            Console.SetCursorPosition(0, Console.CursorTop-1);
        }
    }

    private protected virtual void OnUpdate(FrameEventArgs args)
    {
        CameraSystem.Update((float)args.Time);
        if (Globals.Window.IsKeyDown(Keys.Escape))
        {
            if (_alreadyClosed) return;
            _alreadyClosed = true;
            OnClosing();
            Globals.Window.Close();
        }
    }

    private protected virtual void OnLoad()
    {
        GL.ClearColor(System.Drawing.Color.Aqua);
        GL.Enable(EnableCap.DepthTest);
        
        Globals.Window.CursorGrabbed = true;
        
        #if DEBUG
        GlDebug.Init();
        #endif
        
        var loader = AssimpLoader.GetMeshFromFile("../../../cube.obj");

        var program = new ShaderProgram("../../../default.shader");

        var texture = new Texture("../../../sponza_column_a_diff.png", 1);
        Material material = new(program);
        material.AddSetting(new TextureSetting("albedo", texture));

        var basePath = "../../../cubemap/";

        var paths = new List<string>()
        {
            basePath + "negx.jpg", basePath + "negy.jpg", basePath + "negz.jpg", basePath + "posx.jpg",
            basePath + "posy.jpg", basePath + "posz.jpg"
        };


        var skyboxTexture = new CubemapTexture(paths, 0);
        var skyboxProgram = new ShaderProgram("../../../skybox.shader");
        Material skyboxMaterial = new(skyboxProgram);
        skyboxMaterial.AddSetting(new CubemapSetting("skybox", skyboxTexture));
        skyboxMaterial.AddSetting(new DepthFuncSetting("", DepthFunction.Lequal));

        var skybox = new Entity();
        skybox.AddComponent(new MaterialComponent(loader, skyboxMaterial));
        skybox.AddComponent(new ModelRenderer(loader));

        Entity entity = new();
        entity.AddComponent(new Transform());
        entity.GetComponent<Transform>()!.Scale = new Vector3(20);
        entity.AddComponent(new MaterialComponent(loader, material));
        entity.AddComponent(new ModelRenderer(loader));

        var camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new Camera(30f, 0.1f, 1000f, 0.3f));
        camera.GetComponent<Camera>()?.Set();
    }
}