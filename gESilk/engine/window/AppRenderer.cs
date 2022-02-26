using System.Diagnostics;
using System.Drawing;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static gESilk.engine.Globals;

namespace gESilk.engine.window;

public partial class Application
{
    private readonly int _width, _height, _mBloomComputeWorkGroupSize;
    private readonly HashSet<string> OpenGLExtensions = new();
    private bool _alreadyClosed, _firstRender;
    private ComputeProgram _bloomProgram;
    private readonly EmptyTexture[] _bloomRTs = new EmptyTexture[3];
    private readonly BloomSettings _bloomSettings = new();
    private Vector2i _bloomTexSize;
    private Entity _entity, _ssaoEntity, _blurEntity, _finalShadingEntity;
    private int _mips;
    private RenderBuffer _renderBuffer;
    private RenderTexture _renderTexture;
    public RenderTexture _shadowTex;
    private RenderTexture _renderNormal, _renderPos, _ssaoTex, _blurTex;
    private FrameBuffer _shadowMap, _ssaoMap, _blurMap;
    private EngineState _state;
    private double _time;
    private readonly GameWindow _window;
    public CubemapTexture Skybox;

    public Application(int width, int height, string name)
    {
        _firstRender = true;
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

    protected virtual void OnClosing()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        Console.WriteLine("Closing... Deleting assets");
        AssetManager.Delete();
        Console.WriteLine("Done :)");
    }

    protected virtual void BakeCubemaps()
    {
        UpdateShadow();
        TransformSystem.Update(0f);
        _state = EngineState.RenderShadowState;
        _shadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);
        _state = EngineState.GenerateCubemapState;
        CubemapCaptureManager.Update(0f);
    }

    protected void OnRender(FrameEventArgs args)
    {
        CameraSystem.Update(0f);
        TransformSystem.Update(0f);
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


    protected virtual void RenderBloom()
    {
        // Bloom
        _bloomProgram.Use();
        _bloomProgram.SetUniform("Params",
            new Vector4(_bloomSettings.Threshold, _bloomSettings.Threshold - _bloomSettings.Knee,
                _bloomSettings.Knee * 2f, 0.25f / _bloomSettings.Knee));
        _bloomProgram.SetUniform("LodAndMode", new Vector2(0, (int)BloomMode.BloomModePrefilter));
        _bloomRTs[0].Use(0, TextureAccess.WriteOnly);
        _renderTexture.Use(1);
        _bloomProgram.SetUniform("u_Texture", 1);
        _renderTexture.Use(2);
        _bloomProgram.SetUniform("u_BloomTexture", 2);
        _bloomProgram.Dispatch(_bloomTexSize.X, _bloomTexSize.Y);
        int currentMip;
        for (currentMip = 1; currentMip < _mips; currentMip++)
        {
            var mipSize = _bloomRTs[0].GetMipSize(currentMip);

            _bloomProgram.SetUniform("LodAndMode", new Vector2(currentMip - 1f, (int)BloomMode.BloomModeDownsample));


            // Ping 

            _bloomRTs[1].Use(0, TextureAccess.WriteOnly, currentMip);

            _bloomRTs[0].Use(1);
            _bloomProgram.SetUniform("u_Texture", 1);

            _bloomProgram.Dispatch((int)mipSize.X, (int)mipSize.Y);


            // Pong 

            _bloomProgram.SetUniform("LodAndMode", new Vector2(currentMip, (int)BloomMode.BloomModeDownsample));

            _bloomRTs[0].Use(0, TextureAccess.WriteOnly, currentMip);

            _bloomRTs[1].Use(1);
            _bloomProgram.SetUniform("u_Texture", 1);

            _bloomProgram.Dispatch((int)mipSize.X, (int)mipSize.Y);
        }

        // First Upsample

        _bloomRTs[2].Use(0, TextureAccess.WriteOnly, _mips - 1);

        //currentMip--;

        _bloomProgram.SetUniform("LodAndMode", new Vector2(_mips - 2, (int)BloomMode.BloomModeUpsampleFirst));

        _bloomRTs[0].Use(1);
        _bloomProgram.SetUniform("u_Texture", 1);

        var currentMipSize = _bloomRTs[2].GetMipSize(_mips - 1);

        _bloomProgram.Dispatch((int)currentMipSize.X, (int)currentMipSize.Y);

        for (currentMip = _mips - 2; currentMip >= 0; currentMip--)
        {
            currentMipSize = _bloomRTs[2].GetMipSize(currentMip);
            _bloomRTs[2].Use(0, TextureAccess.WriteOnly, currentMip);
            _bloomProgram.SetUniform("LodAndMode", new Vector2(currentMip, (int)BloomMode.BloomModeUpsample));

            _bloomRTs[0].Use(1);
            _bloomProgram.SetUniform("u_Texture", 1);

            _bloomRTs[2].Use(2);
            _bloomProgram.SetUniform("u_BloomTexture", 2);

            _bloomProgram.Dispatch((int)currentMipSize.X, (int)currentMipSize.Y);
        }
    }

    protected virtual void InitRenderer()
    {
        if (Debugger.IsAttached) GlDebug.Init();

        LoadExtensions();

        _bloomTexSize = new Vector2i(_width, _height) / 2;
        _bloomTexSize += new Vector2i(_mBloomComputeWorkGroupSize - _bloomTexSize.X % _mBloomComputeWorkGroupSize,
            _mBloomComputeWorkGroupSize - _bloomTexSize.Y % _mBloomComputeWorkGroupSize);
        _mips = Texture.GetMipLevelCount(_width, _height) - 4;

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

        for (var i = 0; i < 3; i++)
            _bloomRTs[i] = new EmptyTexture(_bloomTexSize.X, _bloomTexSize.Y, PixelInternalFormat.Rgba16f, _mips);


        _bloomProgram = new ComputeProgram("../../../resources/shader/bloom.glsl");

        var renderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/plane.dae");

        GL.ClearColor(Color.White);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.TextureCubeMapSeamless);

        _window.CursorGrabbed = true;

        var rand = new Random();

        var data = new Vector3[64];

        for (var i = 0; i < 64; i++)
        {
            var sample = new Vector3((float)(rand.NextDouble() * 2.0 - 1.0), (float)(rand.NextDouble() * 2.0 - 1.0),
                (float)rand.NextDouble());
            sample.Normalize();
            sample *= (float)rand.NextDouble();
            var scale = i / 64f;
            scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
            sample *= scale;
            data[i] = sample;
        }


        var framebufferShader = new ShaderProgram("../../../resources/shader/finalcomposite.glsl");

        _finalShadingEntity = new Entity();
        _finalShadingEntity.AddComponent(new MaterialComponent(renderPlaneMesh,
            new Material(framebufferShader, this, DepthFunction.Always)));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("screenTexture", _renderTexture, 0));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("ao", _blurTex, 1));
        _finalShadingEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("bloom", _bloomRTs[2], 2));
        _finalShadingEntity.AddComponent(new FbRenderer(renderPlaneMesh));


        const int shadowSize = 1024 * 4;
        _shadowMap = new FrameBuffer(shadowSize, shadowSize);
        _shadowTex = new RenderTexture(shadowSize, shadowSize, PixelInternalFormat.DepthComponent,
            PixelFormat.DepthComponent, PixelType.Float, true);
        _shadowTex.BindToBuffer(_shadowMap, FramebufferAttachment.DepthAttachment, true);

        var framebufferShaderSsao = new ShaderProgram("../../../resources/shader/SSAO.glsl");


        _ssaoEntity = new Entity();
        _ssaoEntity.AddComponent(new MaterialComponent(renderPlaneMesh,
            new Material(framebufferShaderSsao, this, DepthFunction.Always)));
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


        var blurShader = new ShaderProgram("../../../resources/shader/blur.glsl");

        _blurEntity = new Entity();
        _blurEntity.AddComponent(new MaterialComponent(renderPlaneMesh,
            new Material(blurShader, this, DepthFunction.Always)));
        _blurEntity.GetComponent<MaterialComponent>()?.GetMaterial(0)
            .AddSetting(new TextureSetting("ssaoInput", _ssaoTex, 0));
        _blurEntity.AddComponent(new FbRenderer(renderPlaneMesh));

        _blurMap = new FrameBuffer(_width, _height);

        _blurTex.BindToBuffer(_blurMap, FramebufferAttachment.ColorAttachment0);
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
}