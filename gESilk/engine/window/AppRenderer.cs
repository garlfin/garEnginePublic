using System.Drawing;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace gESilk.engine.window;

public partial class Application
{
    private readonly EmptyTexture[] _bloomRTs = new EmptyTexture[3];
    private readonly BloomSettings _bloomSettings = new();
    private readonly int _width, _height, _mBloomComputeWorkGroupSize;
    private readonly GameWindow _window;
    private readonly HashSet<string> _openGlExtensions = new();
    private bool _alreadyClosed, _firstRender;
    private ComputeProgram _bloomProgram;
    private Vector2i _bloomTexSize;
    private Entity _entity, _ssaoEntity, _blurEntity, _finalShadingEntity;
    private int _mips;
    private RenderBuffer _renderBuffer;
    private RenderTexture _renderNormal, _renderPos, _ssaoTex, _blurTex;
    private RenderTexture _renderTexture;
    private FrameBuffer _postProcessingBuffer;
    private EngineState _state;
    private double _time;
    public FrameBuffer ShadowMap;
    public RenderTexture ShadowTex;
    public CubemapTexture Skybox;
    public RenderTexture BrdfLut;
    private ShaderProgram _irradianceCalculation, _specularCalculation, _pongProgram;
    public Mesh RenderPlaneMesh;
    private ShaderProgram _framebufferShader;
    private ShaderProgram _framebufferShaderSsao;
    private ShaderProgram _blurShader;
    private NoiseTexture _noiseTexture;
    private Vector3[] _data;

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
        CubemapCaptureManager.Update(0f);
    }


    protected void OnRender(FrameEventArgs args)
    {
        CameraSystem.Update(0f);
        TransformSystem.Update(0f);
        LightSystem.UpdateShadow();

        _state = EngineState.RenderShadowState;
        ShadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);

        _state = EngineState.RenderDepthState;
        _renderBuffer.Bind(ClearBufferMask.DepthBufferBit);
        
        GL.DepthMask(true);
        GL.ColorMask(false, false, false, false);
        
        ModelRendererSystem.Update(0f);
        
        GL.ColorMask(true, true, true, true);
        GL.DepthMask(false);

        _state = EngineState.RenderState;
        ModelRendererSystem.Update(0f);
        CubemapMManager.Update(0f);

        _state = EngineState.PostProcessState;
        RenderBloom();

        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        _framebufferShaderSsao.Use();
        _framebufferShaderSsao.SetUniform("screenTexture", _renderTexture.Use(0));
        _framebufferShaderSsao.SetUniform("screenTextureNormal", _renderNormal.Use(1));
        _framebufferShaderSsao.SetUniform("screenTexturePos", _renderPos.Use(2));
        _framebufferShaderSsao.SetUniform("NoiseTex", _noiseTexture.Use(3));
        _framebufferShaderSsao.SetUniform("projection", CameraSystem.CurrentCamera.Projection);
        for (var i = 0; i < _data.Length; i++) _framebufferShaderSsao.SetUniform($"Samples[{i}]", _data[i]);

        _ssaoTex.BindToBuffer(_postProcessingBuffer, FramebufferAttachment.ColorAttachment0);
        _postProcessingBuffer.Bind(null);
        RenderPlaneMesh.Render();

        _blurShader.Use();
        _blurShader.SetUniform("ssaoInput", _ssaoTex.Use(0));

        _blurTex.BindToBuffer(_postProcessingBuffer, FramebufferAttachment.ColorAttachment0);
        RenderPlaneMesh.Render();

        _framebufferShader.Use();
        _framebufferShader.SetUniform("screenTexture", _renderTexture.Use(0));
        _framebufferShader.SetUniform("ao", _blurTex.Use(1));
        _framebufferShader.SetUniform("bloom", _bloomRTs[2].Use(2));

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        RenderPlaneMesh.Render();

        foreach (var camera in CameraSystem.Components)
        {
            camera.UpdatePreviousMatrix();
        }

        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

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
        GlDebug.Init();

        LoadExtensions();

        GL.ClearColor(Color.White);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.TextureCubeMapSeamless);

        _window.CursorGrabbed = true;

        RenderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/plane.dae");

        var prevState = _state;
        _state = EngineState.GenerateBrdfState;

        BrdfLut = new RenderTexture(512, 512, PixelInternalFormat.Rg16f, PixelFormat.Rg, PixelType.Float, false,
            TextureWrapMode.ClampToEdge);

        var bdrfRenderBuffer = new RenderBuffer(512, 512);
        BrdfLut.BindToBuffer(bdrfRenderBuffer, FramebufferAttachment.ColorAttachment0);

        var brdfShader = new ShaderProgram("../../../resources/shader/bdrfLUT.glsl");

        bdrfRenderBuffer.Bind();
        brdfShader.Use();

        RenderPlaneMesh.Render();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        GL.Viewport(0, 0, 1280, 720);

        AssetManager.Remove(bdrfRenderBuffer);
        bdrfRenderBuffer.Delete();
        AssetManager.Remove(brdfShader);
        brdfShader.Delete();

        _state = prevState;

        _bloomTexSize = new Vector2i(_width, _height) / 2;
        _bloomTexSize += new Vector2i(_mBloomComputeWorkGroupSize - _bloomTexSize.X % _mBloomComputeWorkGroupSize,
            _mBloomComputeWorkGroupSize - _bloomTexSize.Y % _mBloomComputeWorkGroupSize);

        _mips = Texture.GetMipLevelCount(_width, _height) - 4;
        for (var i = 0; i < 3; i++)
            _bloomRTs[i] = new EmptyTexture(_bloomTexSize.X, _bloomTexSize.Y, PixelInternalFormat.Rgba16f, _mips);

        _noiseTexture = new NoiseTexture();

        _data = new Vector3[64];

        var rand = new Random();
        for (var i = 0; i < 64; i++)
        {
            var sample = new Vector3((float)(rand.NextDouble() * 2.0 - 1.0), (float)(rand.NextDouble() * 2.0 - 1.0),
                (float)rand.NextDouble());
            sample.Normalize();
            sample *= (float)rand.NextDouble();
            var scale = i / 64f;
            scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
            sample *= scale;
            _data[i] = sample;
        }

        _framebufferShader = new ShaderProgram("../../../resources/shader/finalcomposite.glsl");
        _framebufferShaderSsao = new ShaderProgram("../../../resources/shader/SSAO.glsl");
        _blurShader = new ShaderProgram("../../../resources/shader/blur.glsl");
        _bloomProgram = new ComputeProgram("../../../resources/shader/bloom.glsl");
        _irradianceCalculation = new ShaderProgram("../../../resources/shader/irradiance.glsl");
        _specularCalculation = new ShaderProgram("../../../resources/shader/prefilter.glsl");
        _pongProgram = new ShaderProgram("../../../resources/shader/prefilterpong.glsl");


        _renderBuffer = new RenderBuffer(_width, _height);

        _renderTexture = new RenderTexture(_width, _height);
        _renderNormal = new RenderTexture(_width, _height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba,
            PixelType.Float, false, TextureWrapMode.ClampToEdge, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        _renderPos = new RenderTexture(_width, _height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba,
            PixelType.Float, false, TextureWrapMode.ClampToEdge, TextureMinFilter.Nearest, TextureMagFilter.Nearest);

        _renderTexture.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment0);
        _renderNormal.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment1);
        _renderPos.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment2);


        _postProcessingBuffer = new FrameBuffer(_width, _height);
        _ssaoTex = new RenderTexture(_width, _height, PixelInternalFormat.R8, PixelFormat.Red, PixelType.Float,
            false, TextureWrapMode.ClampToEdge);

        _blurTex = new RenderTexture(_width, _height, PixelInternalFormat.R8, PixelFormat.Red, PixelType.Float,
            false, TextureWrapMode.ClampToEdge);

        const int shadowSize = 1024 * 4;
        ShadowMap = new FrameBuffer(shadowSize, shadowSize);
        ShadowTex = new RenderTexture(shadowSize, shadowSize, PixelInternalFormat.DepthComponent,
            PixelFormat.DepthComponent, PixelType.Float, true);
        ShadowTex.BindToBuffer(ShadowMap, FramebufferAttachment.DepthAttachment, true);
    }

    private void LoadExtensions()
    {
        var count = GL.GetInteger(GetPName.NumExtensions);
        for (var i = 0; i < count; i++)
        {
            var extension = GL.GetString(StringNameIndexed.Extensions, i);
            _openGlExtensions.Add(extension);
        }
    }

    public bool HasExtension(string name)
    {
        return _openGlExtensions.Contains(name);
    }

    public GameWindow GetWindow()
    {
        return _window;
    }

    public EngineState State()
    {
        return _state;
    }

    public void State(EngineState state)
    {
        _state = state;
    }

    public ShaderProgram GetIrradianceProgram()
    {
        return _irradianceCalculation;
    }

    public ShaderProgram GetSpecularProgram()
    {
        return _specularCalculation;
    }

    public ShaderProgram GetPongProgram()
    {
        return _pongProgram;
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