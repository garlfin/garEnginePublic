using System.ComponentModel;
using System.Numerics;
using garEngine.ecs_sys.component;
using garEngine.ecs_sys.entity;
using garEngine.ecs_sys.system;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Drawing;
using Assimp;
using garEngine.render.model;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Camera = garEngine.ecs_sys.component.Camera;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using TextureTarget = OpenTK.Graphics.OpenGL4.TextureTarget;
using Vector3 = OpenTK.Mathematics.Vector3;


namespace garEngine.render;

public class MyWindow : GameWindow
{
    
    private ShaderProgram _shaderProgram;
    private Matrix4 _currentWindowProjection = Matrix4.Zero;
    private float _deltaTime = 0;
    private Texture _myTexture;
    private Texture _normalMap;
    private Texture _heightMap;
    private int _depthMap;
    private int _shadowFrameBuffer;
    public MyWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }
    
    protected override void OnLoad()
    {
        GL.Enable(EnableCap.CullFace);
        CursorGrabbed = true;
        GL.ClearColor(1f,1f,1f, 1f);
        GL.Enable(EnableCap.DepthTest);
        
        WorldSettings.genDepthShader();
        WorldSettings.ShadowBuffer(1024, 1024);

        _shaderProgram = ShaderLoader.LoadShaderProgram("../../../resources/shader/default.vert", "../../../resources/shader/default.frag");
        _myTexture = new Texture("../../../resources/texture/brick_albedo.tif");
        _normalMap = new Texture("../../../resources/texture/brick_normal.tif");
        _shaderProgram.ShaderSettingTexes.Add(new ShaderSettingTex{uniformName = "albedo", value = _myTexture});
        _shaderProgram.ShaderSettingTexes.Add(new ShaderSettingTex {uniformName = "normalMap", value = _normalMap });
        RenderView._Window = this;
        List<string> paths = new List<string>()
        {
            "negx","negy","negz","posx","posy","posz"
        };
        WorldSettings.LoadCubemap(WorldSettings.PathHelper(paths));
        ShaderProgram skyBoxShader = ShaderLoader.LoadShaderProgram("../../../resources/shader/skybox.vert", "../../../resources/shader/skybox.frag");
        WorldSettings.shader = skyBoxShader;
        WorldSettings.genVao();


        AssimpLoaderTest.MeshStruct cubeObject = new AssimpLoaderTest("../../../resources/model/teapot.obj").getMesh(0);
        AssimpLoaderTest.MeshStruct sphereObject = new AssimpLoaderTest("../../../resources/model/plane.dae", PostProcessSteps.None | PostProcessSteps.Triangulate | PostProcessSteps.FindInvalidData).getMesh(0);
        
        Entity entity1 = new Entity();
        entity1.AddComponent(new Transform());
        ModelRenderer modelRenderer = new ModelRenderer(cubeObject, entity1, _shaderProgram);
        entity1.AddComponent(modelRenderer);

        Entity entity2 = new Entity();
        entity2.AddComponent(new Transform());
        entity2.GetComponent<Transform>().Location = new Vector3(0, -5, 0);
        entity2.GetComponent<Transform>().Scale = new Vector3(20);
        ModelRenderer modelRenderer2 = new ModelRenderer(sphereObject, entity2, _shaderProgram);
        entity2.AddComponent(modelRenderer2);
        
        Entity camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new Camera(30f, 0.1f, 1000f, 0.3f));
        camera.GetComponent<Camera>().setCurrentCamera();
        
        Console.WriteLine("");
        base.OnLoad();
        
    }
    

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
       
        if (this.KeyboardState.IsKeyDown(Keys.Escape))
        {
            this.OnClosing(new CancelEventArgs());
            this.Close();
        }
        CameraSystem.Update((float)args.Time);
        RenderView.Update();
        base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        WorldSettings.Render();
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        ModelRendererSystem.Update((float)args.Time);
        
        WorldSettings.renderSkybox();

        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
        Console.WriteLine(args.Time);
        
        
        
        
        SwapBuffers();
        base.OnRenderFrame(args);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        WorldSettings.Delete();
        ModelRendererSystem.Close();
        _myTexture.Delete();
        Console.WriteLine("Done! Closing :)");
        base.OnClosing(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        CameraSystem.UpdateMouse();
        RenderView.Update();
    }
}