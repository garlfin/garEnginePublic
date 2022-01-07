using System.ComponentModel;
using garEngine.ecs_sys.component;
using garEngine.ecs_sys.entity;
using garEngine.ecs_sys.system;
using garEngine.render.model;
using garEngine.render.utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Camera = garEngine.ecs_sys.component.Camera;
using Vector3 = OpenTK.Mathematics.Vector3;


namespace garEngine.render.window;

public class MyWindow : GameWindow
{
    
    private ShaderProgram _shaderProgram;
    private Texture _myTexture;
    private Texture _normalMap;
    private Framebuffer _framebuffer;
    public MyWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }
    
    protected override void OnLoad()
    {
        
        // Settings
        GL.Enable(EnableCap.CullFace | EnableCap.DepthTest);
        GL.ClearColor(0f,0f,1f, 1f);
        
        #if DEBUG
            GLDebug.Init();
        #endif

        CursorGrabbed = true;
         
        // Cubemap paths
        List<string> paths = new List<string>()
        {
            "negx","negy","negz","posx","posy","posz"
        };
        
        WorldSettings.ShadowBuffer(2048, 2048);
        WorldSettings.LoadCubemap(WorldSettings.PathHelper(paths));
        
        ShaderProgram skyBoxShader = ShaderLoader.LoadShaderProgram("resources/shader/skybox.vert", "resources/shader/skybox.frag");
        ShaderProgram shadowDepthProgram = ShaderLoader.LoadShaderProgram("resources/shader/shadowDepth.vert", "resources/shader/depth.frag");
        ShaderProgram depthProgram = ShaderLoader.LoadShaderProgram("resources/shader/depth.vert", "resources/shader/depth.frag");

        
        WorldSettings.SetShadowDepthMaterial(new Material(shadowDepthProgram));
        WorldSettings.SetDepthMaterial(new Material(depthProgram));
        WorldSettings.SetSkyboxMaterial(new Material(skyBoxShader));
        WorldSettings.genVao();
        
        _shaderProgram = ShaderLoader.LoadShaderProgram("resources/shader/default.vert", "resources/shader/default.frag");
        _myTexture = new Texture("resources/texture/brick_albedo.tif");
        _normalMap = new Texture("resources/texture/brick_normal.tif");
        
        Material defaultShader = new(_shaderProgram);
        defaultShader.AddSetting("albedo", _myTexture);
        defaultShader.AddSetting("normalMap", _normalMap);
        
        var framebufferShader = ShaderLoader.LoadShaderProgram("resources/shader/framebuffer.vert", "resources/shader/framebuffer.frag");
        var framebufferMaterial = new Material(framebufferShader);
        _framebuffer = new Framebuffer(framebufferMaterial);


        RenderView._Window = this;
        
        
        MeshObject cubeObject =new MeshObject(new AssimpLoaderTest("resources/model/teapot.obj"));
        MeshObject sphereObject = new MeshObject(new AssimpLoaderTest("resources/model/plane.dae"));

 

        Entity entity2 = new Entity();
        entity2.AddComponent(new Transform());
        entity2.GetComponent<Transform>().Location = new Vector3(0, -10, 0);
        entity2.GetComponent<Transform>().Scale = new Vector3(20);
        entity2.AddComponent(new MaterialComponent(sphereObject, defaultShader));
        entity2.AddComponent( new ModelRenderer(sphereObject));
        
        Entity entity1 = new Entity();
        entity1.AddComponent(new Transform());
        entity1.AddComponent(new MaterialComponent(cubeObject, defaultShader));
        entity1.AddComponent(new ModelRenderer(cubeObject));
        
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

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0,0, e.Width, e.Height);

    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.DepthMask(true);
        WorldSettings.RenderShadow();
        
       _framebuffer.Bind();

       GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.ColorMask(false, false,false,false);

        ModelRendererSystem.UpdateDepth(false);
        WorldSettings.renderSkyboxDepth();

        GL.DepthFunc(DepthFunction.Equal);
        GL.ColorMask(true, true, true, true);
        GL.DepthMask(false);

        ModelRendererSystem.Update((float)args.Time);
        WorldSettings.renderSkybox();
        
        _framebuffer.Render();
        
        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
        Console.WriteLine(args.Time);
        
        
        
        
        SwapBuffers();
        base.OnRenderFrame(args);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        WorldSettings.Delete();
        MaterialManager.Delete();
        MaterialManager.Delete();
        RenderTexManager.Delete();
        VertexArrayManager.Delete();
        FBManager.Delete();
        Console.WriteLine("Done! Closing :)");
        base.OnClosing(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        CameraSystem.UpdateMouse();
        RenderView.Update();
    }
}