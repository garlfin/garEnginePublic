using static gESilk.engine.Globals;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render;
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
    private FrameBuffer _frameBuffer;
    private RenderTexture _renderTexture;
    private int _width, _height;

    public Window(int width, int height, string name)
    {
        _width = width;
        _height = height;
        var gws = GameWindowSettings.Default;
        // Setup
        gws.RenderFrequency = 144;
        gws.UpdateFrequency = 144;
        gws.IsMultiThreaded = true;

        var nws = NativeWindowSettings.Default;
        // Setup
        nws.APIVersion = new Version(4,6);
        nws.Size = new Vector2i(width, height);
        nws.Title = name;
        nws.IsEventDriven = false;
        nws.WindowBorder = WindowBorder.Fixed;
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
        Console.WriteLine("Closing... Deleting assets");
        TextureManager.Delete();
        MeshManager.Delete();
        ShaderProgramManager.Delete();
        CubemapManager.Delete();
        Console.WriteLine("Done :)");
    }

    private protected virtual void OnRender(FrameEventArgs args)
    {
        CameraSystem.UpdateCamera();
        UpdateRender();
        
        _frameBuffer.Bind();
        
        GL.ColorMask(false, false,false,false);
        GL.DepthMask(true);
        ModelRendererSystem.Update(false);
        CubemapMManager.Update((float)args.Time);
        GL.ColorMask(true, true,true,true);
        GL.DepthMask(false);
        ModelRendererSystem.Update((float)args.Time);
        CubemapMManager.Update((float)args.Time);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        FBRendererSystem.Update(0f);
        
        if (!_alreadyClosed)
        {
            Console.Write("FPS: "+1.0/args.Time + new string(' ', Console.WindowWidth - args.Time.ToString().Length - 5));
            Console.SetCursorPosition(0, Console.CursorTop-1);
        }
        
        Globals.Window.SwapBuffers();
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
        
        _frameBuffer = new FrameBuffer(_width, _height);
        _renderTexture = new RenderTexture(_width, _height, 3);
        _renderTexture.BindToFramebuffer(_frameBuffer, FramebufferAttachment.ColorAttachment0);

        GL.ClearColor(System.Drawing.Color.Aqua);
        GL.Enable(EnableCap.DepthTest);
        
        Globals.Window.CursorGrabbed = true;
        
        //#if DEBUG
            GlDebug.Init();
        //#endif
        
        var loader = AssimpLoader.GetMeshFromFile("../../../cube.obj");
        var skyboxLoader = AssimpLoader.GetMeshFromFile("../../../cube.obj");
        skyboxLoader.IsSkybox(true);
        
        var program = new ShaderProgram("../../../shader/default.shader");
        var texture = new Texture("../../../brick_albedo.tif", 1);
        var normal = new Texture("../../../brick_normal.png", 2);
        
        Material material = new(program);
        material.AddSetting(new TextureSetting("albedo", texture));
        material.AddSetting(new TextureSetting("normalMap", normal));
        material.AddSetting(new Vec3Setting("lightPos", new Vector3(10, 10, 10)));
        
        var basePath = "../../../cubemap/";

        var paths = new List<string>
        {
            basePath + "negx.jpg", basePath + "negy.jpg", basePath + "negz.jpg", basePath + "posx.jpg",
            basePath + "posy.jpg", basePath + "posz.jpg"
        };


        var skyboxTexture = new CubemapTexture(paths, 0);
        var skyboxProgram = new ShaderProgram("../../../shader/skybox.shader");
        Material skyboxMaterial = new(skyboxProgram, DepthFunction.Lequal);
        
        skyboxMaterial.AddSetting(new CubemapSetting("skybox", skyboxTexture));

        var skybox = new Entity();
        skybox.AddComponent(new MaterialComponent(skyboxLoader, skyboxMaterial));
        skybox.AddComponent(new CubemapRenderer(skyboxLoader));

        Entity entity = new();
        entity.AddComponent(new Transform());
        entity.GetComponent<Transform>()!.Scale = new Vector3(20);
        entity.AddComponent(new MaterialComponent(loader, material));
        entity.AddComponent(new ModelRenderer(loader));

        var camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new Camera(30f, 0.1f, 1000f, 0.3f));
        camera.GetComponent<Camera>()?.Set();
        
        var framebufferShader = new ShaderProgram("../../../shader/framebuffer.shader");

        var renderPlane = new Entity();
        var renderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../plane.dae");
        renderPlane.AddComponent(new MaterialComponent(renderPlaneMesh, new Material(framebufferShader, DepthFunction.Always)));
        renderPlane.GetComponent<MaterialComponent>().GetMaterial(0).AddSetting(new RenderTexSetting("screenTexture", _renderTexture));
        renderPlane.AddComponent(new FBRenderer(renderPlaneMesh));
        
    }
}