using System.Diagnostics;
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

public sealed class Window
{
    private bool _alreadyClosed;
    private RenderBuffer _renderBuffer;
    private FrameBuffer _shadowMap, _ssaoMap, _blurMap;
    private RenderTexture _renderTexture, _shadowTex, _renderNormal, _renderPos, _ssaoTex, _blurTex;
    private readonly int _width, _height;
    private double _time;
    private Entity entity, _ssaoEntity, _blurEntity, _finalShadingEntity;

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

    private void OnMouseMove(MouseMoveEventArgs args)
    {
        CameraSystem.UpdateMouse();
    }

    private void OnClosing()
    {
        Console.WriteLine();
        Console.WriteLine("Closing... Deleting assets");
        AssetManager.Delete();
        Console.WriteLine("Done :)");
    }

    private void OnRender(FrameEventArgs args)
    {
        if (_time > 12)
        {
            _time = 0;
        }
        
        //Logic stuff here
        entity.GetComponent<Transform>().Rotation = (0f, (float) _time * 30 , 0f);
        
        
        _time += args.Time;
        CameraSystem.UpdateCamera();
        UpdateRender(true);
        
        _shadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(true);
        
        _renderBuffer.Bind();
        UpdateRender();
        ModelRendererSystem.Update((float)args.Time);
        CubemapMManager.Update((float)args.Time);
        
        _ssaoMap.Bind();
        _ssaoEntity.GetComponent<FBRenderer>().Update(0f);

        _blurMap.Bind();
        _blurEntity.GetComponent<FBRenderer>().Update(0f);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _finalShadingEntity.GetComponent<FBRenderer>().Update(0f);
        
        if (!_alreadyClosed)
        {
            Console.Write("FPS: "+1.0/args.Time + new string(' ', Console.WindowWidth - args.Time.ToString().Length - 5));
            Console.SetCursorPosition(0, Console.CursorTop-1);
        }
        
        Globals.Window.SwapBuffers();
    }

    private void OnUpdate(FrameEventArgs args)
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
    
    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    private void OnLoad()
    {
        
        if (Debugger.IsAttached)  GlDebug.Init();
        
        _renderBuffer = new RenderBuffer(_width, _height);
        _renderTexture = new RenderTexture(_width, _height, 1);
        _renderNormal = new RenderTexture(_width, _height, 2,PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, false, TextureWrapMode.ClampToEdge, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        _renderPos = new RenderTexture(_width, _height, 3, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, false, TextureWrapMode.ClampToEdge, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        _renderTexture.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment0);
        _renderNormal.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment1);
        _renderPos.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment2);
        _blurTex = new RenderTexture(_width, _height, 2,PixelInternalFormat.R8, PixelFormat.Red, PixelType.Float, false, TextureWrapMode.ClampToEdge);
        
        GL.ClearColor(System.Drawing.Color.White);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.TextureCubeMapSeamless);
        
        Globals.Window.CursorGrabbed = true;
        

        var loader = AssimpLoader.GetMeshFromFile("../../../hut.obj");
        var skyboxLoader = AssimpLoader.GetMeshFromFile("../../../cube.obj");
        skyboxLoader.IsSkybox(true);
        
        var program = new ShaderProgram("../../../resources/shader/default.shader");
        var texture = new Texture("../../../brick_albedo.tif", 1);
        var normal = new Texture("../../../brick_normal.png", 2);
        
        Material material = new(program);
        material.AddSetting(new TextureSetting("albedo", texture));
        material.AddSetting(new TextureSetting("normalMap", normal));
        material.AddSetting(new GlobalSunPosSetting("lightPos"));
        
        Material woodMaterial = new Material(program);
        woodMaterial.AddSetting(new TextureSetting("albedo", new Texture("../../../resources/texture/rough_wood_diff_1k.jpg", 1)));
        woodMaterial.AddSetting(new TextureSetting("normalMap", new Texture("../../../resources/texture/rough_wood_nor_dx_1k.jpg", 2)));
        woodMaterial.AddSetting(new GlobalSunPosSetting("lightPos"));

        var basePath = "../../../cubemap/";
        

        var paths = new List<string>
        {
            basePath + "negx.jpg", basePath + "negy.jpg", basePath + "negz.jpg", basePath + "posx.jpg",
            basePath + "posy.jpg", basePath + "posz.jpg"
        };


        var skyboxTexture = new CubemapTexture(paths, 0);
        var skyboxProgram = new ShaderProgram("../../../resources/shader/skybox.shader");
        material.AddSetting(new CubemapSetting("skyBox", skyboxTexture));
        Material skyboxMaterial = new(skyboxProgram, DepthFunction.Lequal, CullFaceMode.Front);
        
        
        skyboxMaterial.AddSetting(new CubemapSetting("skybox", skyboxTexture));

        var skybox = new Entity();
        skybox.AddComponent(new MaterialComponent(skyboxLoader, skyboxMaterial));
        skybox.AddComponent(new CubemapRenderer(skyboxLoader));
        
        entity = new Entity();
        entity.AddComponent(new Transform());
        entity.AddComponent(new MaterialComponent(loader, woodMaterial));
        entity.AddComponent(new ModelRenderer(loader));
        entity.AddComponent(new Transform());
        

        var camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new Camera(30f, 0.1f, 1000f, 0.3f));
        camera.GetComponent<Camera>()?.Set();
        

        var rand = new Random();

        List<Vector3> data = new List<Vector3>();

        for (int i = 0; i < 64; i++)
        {
            Vector3 sample = new Vector3((float)(rand.NextDouble() * 2.0 - 1.0), (float)(rand.NextDouble() * 2.0 - 1.0), (float)rand.NextDouble());
            sample.Normalize();
            sample *= (float)rand.NextDouble();
            float scale = (float)i / 64f;
            scale = Lerp(0.1f, 1.0f, scale*scale);
            sample *= scale;
            data.Add(sample);
        }
        
        
        var framebufferShader = new ShaderProgram("../../../resources/shader/framebuffer.shader");

        _finalShadingEntity = new Entity();
        var renderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../plane.dae");
        _finalShadingEntity.AddComponent(new MaterialComponent(renderPlaneMesh, new Material(framebufferShader, DepthFunction.Always)));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new RenderTexSetting("screenTexture", _renderTexture));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new RenderTexSetting("ao", _blurTex));
        _finalShadingEntity.AddComponent(new FBRenderer(renderPlaneMesh));

        var physicalPlane = new Entity();
        
        physicalPlane.AddComponent(new MaterialComponent(renderPlaneMesh, material));
        physicalPlane.AddComponent(new ModelRenderer(renderPlaneMesh));
        physicalPlane.AddComponent(new Transform());
        physicalPlane.GetComponent<Transform>().Rotation = new Vector3(-90f, 0, 0);
        physicalPlane.GetComponent<Transform>().Scale = new Vector3(10);

        int shadowSize = 1024 * 4;
        _shadowMap = new FrameBuffer(shadowSize, shadowSize); 
        _shadowTex = new RenderTexture(shadowSize, shadowSize, 4, PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, PixelType.Float, true);
        _shadowTex.BindToBuffer(_shadowMap, FramebufferAttachment.DepthAttachment, true);
        material.AddSetting(new RenderTexSetting("shadowMap", _shadowTex));

  
        
        var framebufferShaderSsao = new ShaderProgram("../../../resources/shader/SSAO.shader");

      
        _ssaoEntity = new Entity();
        _ssaoEntity.AddComponent(new MaterialComponent(renderPlaneMesh, new Material(framebufferShaderSsao, DepthFunction.Always)));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new RenderTexSetting("screenTextureNormal", _renderNormal));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new RenderTexSetting("screenTexturePos", _renderPos));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new Vec3ArraySetting("Samples", data.ToArray()));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new NoiseTexSetting("NoiseTex", new NoiseTexture(5)));
        _ssaoEntity.AddComponent(new FBRenderer(renderPlaneMesh));
        
        _ssaoMap = new FrameBuffer(_width, _height); 
        _ssaoTex = new RenderTexture(_width, _height, 1,PixelInternalFormat.R8, PixelFormat.Red, PixelType.Float, false, TextureWrapMode.ClampToEdge);
        _ssaoTex.BindToBuffer(_ssaoMap, FramebufferAttachment.ColorAttachment0);
        

        var blurShader = new ShaderProgram("../../../resources/shader/blur.shader");
        
        _blurEntity = new Entity();
        _blurEntity.AddComponent(new MaterialComponent(renderPlaneMesh, new Material(blurShader, DepthFunction.Always)));
        _blurEntity.GetComponent<MaterialComponent>()?.GetMaterial(0).AddSetting(new RenderTexSetting("ssaoInput", _ssaoTex));
        _blurEntity.AddComponent(new FBRenderer(renderPlaneMesh));
        
        _blurMap = new FrameBuffer(_width, _height); 
       
        _blurTex.BindToBuffer(_blurMap, FramebufferAttachment.ColorAttachment0);
        
        SunPos = new Vector3(11.8569f, 26.5239f, 5.77871f);

    }
}
