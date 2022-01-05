using System.ComponentModel;
using garEngine.ecs_sys.component;
using garEngine.ecs_sys.entity;
using garEngine.ecs_sys.system;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using garEngine.render.model;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static garEngine.render.model.AssimpLoaderTest;
using Camera = garEngine.ecs_sys.component.Camera;
using Vector3 = OpenTK.Mathematics.Vector3;


namespace garEngine.render;

public class MyWindow : GameWindow
{
    
    private ShaderProgram _shaderProgram;
    private Texture _myTexture;
    private Texture _normalMap;
    public MyWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }
    
    protected override void OnLoad()
    {
        
        // Settings
        GL.Enable(EnableCap.CullFace);
        GL.ClearColor(0f,0f,1f, 1f);
        GL.Enable(EnableCap.DepthTest);
        CursorGrabbed = true;
         
        // Cubemap paths
        List<string> paths = new List<string>()
        {
            "negx","negy","negz","posx","posy","posz"
        };
        
        WorldSettings.ShadowBuffer(2048, 2048);
        WorldSettings.LoadCubemap(WorldSettings.PathHelper(paths));
        
        ShaderProgram skyBoxShader = ShaderLoader.LoadShaderProgram("../../../resources/shader/skybox.vert", "../../../resources/shader/skybox.frag");
        ShaderProgram shadowDepthProgram = ShaderLoader.LoadShaderProgram("../../../resources/shader/shadowDepth.vert", "../../../resources/shader/depth.frag");
        ShaderProgram depthProgram = ShaderLoader.LoadShaderProgram("../../../resources/shader/depth.vert", "../../../resources/shader/depth.frag");

        
        WorldSettings.SetShadowDepthMaterial(new Material(shadowDepthProgram));
        WorldSettings.SetDepthMaterial(new Material(depthProgram));
        WorldSettings.SetSkyboxMaterial(new Material(skyBoxShader));
        WorldSettings.genVao();

        _shaderProgram = ShaderLoader.LoadShaderProgram("../../../resources/shader/default.vert", "../../../resources/shader/default.frag");
        _myTexture = new Texture("../../../resources/texture/brick_albedo.tif");
        _normalMap = new Texture("../../../resources/texture/brick_normal.tif");
        
        Material defaultShader = new(_shaderProgram);
        defaultShader.AddSetting("albedo", _myTexture);
        defaultShader.AddSetting("normalMap", _normalMap);
        
        
        RenderView._Window = this;
        
        
        MeshStruct cubeObject = new AssimpLoaderTest("../../../resources/model/teapot.obj").getMesh(0);
        MeshStruct sphereObject = new AssimpLoaderTest("../../../resources/model/plane.dae").getMesh(0);
        


        Entity entity1 = new Entity();
        entity1.AddComponent(new Transform());
        ModelRenderer modelRenderer = new ModelRenderer(cubeObject, entity1, defaultShader);
        entity1.AddComponent(modelRenderer);

        Entity entity2 = new Entity();
        entity2.AddComponent(new Transform());
        entity2.GetComponent<Transform>().Location = new Vector3(0, -10, 0);
        entity2.GetComponent<Transform>().Scale = new Vector3(20);
        ModelRenderer modelRenderer2 = new ModelRenderer(sphereObject, entity2, defaultShader);
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
        GL.DepthMask(true);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        WorldSettings.RenderShadow();
        
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