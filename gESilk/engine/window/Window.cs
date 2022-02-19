using System.Diagnostics;
using System.Drawing;
using static gESilk.engine.Globals;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Camera = gESilk.engine.components.Camera;
using Material = gESilk.engine.render.materialSystem.Material;
using Texture = gESilk.engine.render.assets.textures.ImageTexture;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;


namespace gESilk.engine.window;

public sealed class Window
{
    private bool _alreadyClosed;
    private RenderBuffer _renderBuffer;
    private FrameBuffer _shadowMap, _ssaoMap, _blurMap;
    private RenderTexture _renderTexture, _shadowTex, _renderNormal, _renderPos, _ssaoTex, _blurTex;
    private readonly int _width, _height, _mBloomComputeWorkGroupSize;
    private double _time;
    private int _mips;
    private ComputeProgram _program;
    private Entity _entity, _ssaoEntity, _blurEntity, _finalShadingEntity;
    private EmptyTexture[] _bloomRTs = new EmptyTexture[3];
    private BloomSettings _bloomSettings = new();
    private Vector2i _bloomTexSize;
    private GameWindow _window;
    private EngineState _state;
    private readonly HashSet<string> OpenGLExtensions = new();

    public Window(int width, int height, string name)
    {
        _width = width;
        _height = height;
        _mBloomComputeWorkGroupSize = 16;
        var gws = GameWindowSettings.Default;
        // Setup
        gws.RenderFrequency = 144;
        gws.UpdateFrequency = 144;
        gws.IsMultiThreaded = true;

        var nws = NativeWindowSettings.Default;
        // Setup
        nws.APIVersion = new Version(4, 6);
        nws.Size = new Vector2i(width, height);
        nws.Title = name;
        nws.IsEventDriven = false;
        nws.WindowBorder = WindowBorder.Fixed;
        _window = new GameWindow(gws, nws);
    }
    
    private void LoadExtensions()
    {
        var count = GL.GetInteger(GetPName.NumExtensions);
        for (var i = 0; i < count; i++)
        {
            var extension = GL.GetString(StringNameIndexed.Extensions, i);
            OpenGLExtensions.Add(extension);
        }
    }

    public bool HasExtension(string name)
    {
        return OpenGLExtensions.Contains(name);
    }

    public GameWindow GetWindow()
    {
        return _window;
    }

    public EngineState State()
    {
        return _state;
    }

    public void Run()
    {
        _window.Load += OnLoad;
        _window.UpdateFrame += OnUpdate;
        _window.RenderFrame += OnRender;
        _window.MouseMove += OnMouseMove;
        _window.Run();
    }


    private void OnClosing()
    {
        Console.WriteLine("Closing... Deleting assets");
        AssetManager.Delete();
        Console.WriteLine("Done :)");
    }

    private static float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    private void OnLoad()
    {
        if (Debugger.IsAttached) GlDebug.Init();

        //_controller = new(_width, _height);
        LoadExtensions();
        
        _renderBuffer = new RenderBuffer(_width, _height);
        _renderTexture = new RenderTexture(_width, _height);
        _renderNormal = new RenderTexture(_width, _height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba,
            PixelType.Float, false, TextureWrapMode.ClampToEdge, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        _renderPos = new RenderTexture(_width, _height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba,
            PixelType.Float, false, TextureWrapMode.ClampToEdge, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        _renderTexture.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment0);
        _renderNormal.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment1);
        _renderPos.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment2);
        _blurTex = new RenderTexture(_width, _height, PixelInternalFormat.R8, PixelFormat.Red, PixelType.Float,
            false, TextureWrapMode.ClampToEdge);
        //var _depthPassTex = new RenderTexture(_width, _height, PixelInternalFormat.DepthComponent,
        //PixelFormat.DepthComponent);
        //_depthPassTex.BindToBuffer(_renderBuffer, FramebufferAttachment.DepthAttachment);

        _bloomTexSize = new Vector2i(_width, _height) / 2;
        _bloomTexSize += new Vector2i(_mBloomComputeWorkGroupSize - _bloomTexSize.X % _mBloomComputeWorkGroupSize,
            _mBloomComputeWorkGroupSize - _bloomTexSize.Y % _mBloomComputeWorkGroupSize);
        _mips = render.assets.textures.Texture.GetMipLevelCount(_width, _height) - 4;


        for (var i = 0; i < 3; i++)
            _bloomRTs[i] = new EmptyTexture(_bloomTexSize.X, _bloomTexSize.Y, PixelInternalFormat.Rgba16f, _mips);


        _program = new ComputeProgram("../../../resources/shader/bloom.shader");
        GL.ClearColor(Color.White);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.TextureCubeMapSeamless);

        _window.CursorGrabbed = true;


        var loader = AssimpLoader.GetMeshFromFile("../../../resources/models/hut.obj");
        var skyboxLoader = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        skyboxLoader.IsSkybox(true);

        var program = new ShaderProgram("../../../resources/shader/default.shader");
        var texture = new Texture("../../../resources/texture/brick_albedo.tif");
        var normal = new Texture("../../../resources/texture/brick_normal.png");

        Material material = new(program);
        material.AddSetting(new TextureSetting("albedo", texture, 1));
        material.AddSetting(new TextureSetting("normalMap", normal, 2));
        material.AddSetting(new GlobalSunPosSetting("lightPos"));
        material.AddSetting(new FloatSetting("roughness", 0));

        var woodMaterial = new Material(program);
        woodMaterial.AddSetting(new TextureSetting("albedo",
            new Texture("../../../resources/texture/rough_wood_diff_1k.jpg"), 1));
        woodMaterial.AddSetting(new TextureSetting("normalMap",
            new Texture("../../../resources/texture/rough_wood_nor_dx_1k.jpg"), 2));
        woodMaterial.AddSetting(new GlobalSunPosSetting("lightPos"));

        var basePath = "../../../resources/cubemap/";


        var paths = new List<string>
        {
            basePath + "negx.jpg", basePath + "negy.jpg", basePath + "negz.jpg", basePath + "posx.jpg",
            basePath + "posy.jpg", basePath + "posz.jpg"
        };


        var skyboxTexture = new CubemapTexture(paths);
        var skyboxProgram = new ShaderProgram("../../../resources/shader/skybox.shader");
        material.AddSetting(new TextureSetting("skyBox", skyboxTexture, 0));
        woodMaterial.AddSetting(new TextureSetting("skyBox", skyboxTexture, 0));
        Material skyboxMaterial = new(skyboxProgram, DepthFunction.Lequal, CullFaceMode.Front);


        skyboxMaterial.AddSetting(new TextureSetting("skybox", skyboxTexture, 0));

        var skybox = new Entity();
        skybox.AddComponent(new MaterialComponent(skyboxLoader, skyboxMaterial));
        skybox.AddComponent(new CubemapRenderer(skyboxLoader));

        _entity = new Entity();
        _entity.AddComponent(new Transform());
        _entity.AddComponent(new MaterialComponent(loader, woodMaterial));
        _entity.AddComponent(new ModelRenderer(loader));
        _entity.AddComponent(new Transform());


        var camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new MovementBehavior(0.3f));
        camera.AddComponent(new Camera(72f, 0.1f, 1000f));
        camera.GetComponent<Camera>()?.Set();

        var rand = new Random();

        var data = new Vector3[64];

        for (var i = 0; i < 64; i++)
        {
            var sample = new Vector3((float)(rand.NextDouble() * 2.0 - 1.0), (float)(rand.NextDouble() * 2.0 - 1.0),
                (float)rand.NextDouble());
            sample.Normalize();
            sample *= (float)rand.NextDouble();
            var scale = i / 64f;
            scale = Lerp(0.1f, 1.0f, scale * scale);
            sample *= scale;
            data[i] = sample;
        }


        var framebufferShader = new ShaderProgram("../../../resources/shader/finalcomposite.shader");

        _finalShadingEntity = new Entity();
        var renderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/plane.dae");
        _finalShadingEntity.AddComponent(new MaterialComponent(renderPlaneMesh,
            new Material(framebufferShader, DepthFunction.Always)));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("screenTexture", _renderTexture, 0));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("ao", _blurTex, 1));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("bloom", _bloomRTs[2], 2));
        _finalShadingEntity.AddComponent(new FbRenderer(renderPlaneMesh));

        var physicalPlane = new Entity();

        physicalPlane.AddComponent(new MaterialComponent(renderPlaneMesh, material));
        physicalPlane.AddComponent(new ModelRenderer(renderPlaneMesh));
        physicalPlane.AddComponent(new Transform());
        physicalPlane.GetComponent<Transform>().Rotation = new Vector3(-90f, 0, 0);
        physicalPlane.GetComponent<Transform>().Scale = new Vector3(10);

        const int shadowSize = 1024 * 4;
        _shadowMap = new FrameBuffer(shadowSize, shadowSize);
        _shadowTex = new RenderTexture(shadowSize, shadowSize, PixelInternalFormat.DepthComponent,
            PixelFormat.DepthComponent, PixelType.Float, true);
        _shadowTex.BindToBuffer(_shadowMap, FramebufferAttachment.DepthAttachment, true);
        material.AddSetting(new TextureSetting("shadowMap", _shadowTex, 5));

        var framebufferShaderSsao = new ShaderProgram("../../../resources/shader/SSAO.shader");


        _ssaoEntity = new Entity();
        _ssaoEntity.AddComponent(new MaterialComponent(renderPlaneMesh,
            new Material(framebufferShaderSsao, DepthFunction.Always)));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("screenTextureNormal", _renderNormal, 0));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("screenTexturePos", _renderPos, 1));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new Vec3ArraySetting("Samples", data.ToArray()));
        _ssaoEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("NoiseTex", new NoiseTexture(), 2));
        _ssaoEntity.AddComponent(new FbRenderer(renderPlaneMesh));

        _ssaoMap = new FrameBuffer(_width, _height);
        _ssaoTex = new RenderTexture(_width, _height, PixelInternalFormat.R8, PixelFormat.Red, PixelType.Float,
            false, TextureWrapMode.ClampToEdge);
        _ssaoTex.BindToBuffer(_ssaoMap, FramebufferAttachment.ColorAttachment0);


        var blurShader = new ShaderProgram("../../../resources/shader/blur.shader");

        _blurEntity = new Entity();
        _blurEntity.AddComponent(new MaterialComponent(renderPlaneMesh,
            new Material(blurShader, DepthFunction.Always)));
        _blurEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("ssaoInput", _ssaoTex, 0));
        _blurEntity.AddComponent(new FbRenderer(renderPlaneMesh));

        _blurMap = new FrameBuffer(_width, _height);

        _blurTex.BindToBuffer(_blurMap, FramebufferAttachment.ColorAttachment0);


        SunPos = new Vector3(11.8569f, 26.5239f, 5.77871f);
    }

    private void OnRender(FrameEventArgs args)
    {
        CameraSystem.Update(0f);
        UpdateShadow();

        _state = EngineState.RenderShadowState;
        _shadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);

        _state = EngineState.RenderDepthState;
        _renderBuffer.Bind();
        GL.ColorMask(false, false, false, false);
        ModelRendererSystem.Update(0f);
        GL.ColorMask(true, true, true, true);
        GL.DepthMask(false);

        _state = EngineState.RenderState;
        ModelRendererSystem.Update(0f);
        CubemapMManager.Update(0f);

        _state = EngineState.PostProcessState;
        RenderBloom();
        _ssaoMap.Bind(null);
        _ssaoEntity.GetComponent<FbRenderer>().Update(0f);

        _blurMap.Bind(null);
        _blurEntity.GetComponent<FbRenderer>().Update(0f);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _finalShadingEntity.GetComponent<FbRenderer>().Update(0f);

        if (_alreadyClosed) GL.Clear(ClearBufferMask.ColorBufferBit);

        _window.SwapBuffers();
    }

    private void RenderBloom()
    {
        // Bloom
        _program.Use();
        _program.SetUniform("Params",
            new Vector4(_bloomSettings.Threshold, _bloomSettings.Threshold - _bloomSettings.Knee,
                _bloomSettings.Knee * 2f, 0.25f / _bloomSettings.Knee));
        _program.SetUniform("LodAndMode", new Vector2(0, (int)BloomMode.BloomModePrefilter));
        _bloomRTs[0].Use(0, TextureAccess.WriteOnly);
        _renderTexture.Use(1);
        _program.SetUniform("u_Texture", 1);
        _renderTexture.Use(2);
        _program.SetUniform("u_BloomTexture", 2);
        _program.Dispatch(_bloomTexSize.X, _bloomTexSize.Y);
        int currentMip;
        for (currentMip = 1; currentMip < _mips; currentMip++)
        {
            var mipSize = _bloomRTs[0].GetMipSize(currentMip);

            _program.SetUniform("LodAndMode", new Vector2(currentMip - 1f, (int)BloomMode.BloomModeDownsample));


            // Ping 

            _bloomRTs[1].Use(0, TextureAccess.WriteOnly, currentMip);

            _bloomRTs[0].Use(1);
            _program.SetUniform("u_Texture", 1);

            _program.Dispatch((int)mipSize.X, (int)mipSize.Y);


            // Pong 

            _program.SetUniform("LodAndMode", new Vector2(currentMip, (int)BloomMode.BloomModeDownsample));

            _bloomRTs[0].Use(0, TextureAccess.WriteOnly, currentMip);

            _bloomRTs[1].Use(1);
            _program.SetUniform("u_Texture", 1);

            _program.Dispatch((int)mipSize.X, (int)mipSize.Y);
        }

        // First Upsample

        _bloomRTs[2].Use(0, TextureAccess.WriteOnly, _mips - 1);

        //currentMip--;

        _program.SetUniform("LodAndMode", new Vector2(_mips - 2, (int)BloomMode.BloomModeUpsampleFirst));

        _bloomRTs[0].Use(1);
        _program.SetUniform("u_Texture", 1);

        var currentMipSize = _bloomRTs[2].GetMipSize(_mips - 1);

        _program.Dispatch((int)currentMipSize.X, (int)currentMipSize.Y);

        for (currentMip = _mips - 2; currentMip >= 0; currentMip--)
        {
            currentMipSize = _bloomRTs[2].GetMipSize(currentMip);
            _bloomRTs[2].Use(0, TextureAccess.WriteOnly, currentMip);
            _program.SetUniform("LodAndMode", new Vector2(currentMip, (int)BloomMode.BloomModeUpsample));

            _bloomRTs[0].Use(1);
            _program.SetUniform("u_Texture", 1);

            _bloomRTs[2].Use(2);
            _program.SetUniform("u_BloomTexture", 2);

            _program.Dispatch((int)currentMipSize.X, (int)currentMipSize.Y);
        }
    }

    private void OnUpdate(FrameEventArgs args)
    {
        _time += args.Time;
        if (_time > 12) // 360 / 30 = 12 : )
            _time = 0;

        //Logic stuff here
        _entity.GetComponent<Transform>().Rotation = (0f, (float)_time * 30, 0f);
        BehaviorSystem.Update((float)args.Time);

        if (_window.IsKeyDown(Keys.Escape))
        {
            if (_alreadyClosed) return;
            _alreadyClosed = true;
            OnClosing();
            _window.Close();
        }
    }

    private void OnMouseMove(MouseMoveEventArgs args)
    {
        BehaviorSystem.UpdateMouse(args);
    }
}