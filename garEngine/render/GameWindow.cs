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
using garEngine.render.model;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace garEngine.render;

public class MyWindow : GameWindow
{
    
    private ShaderProgram _shaderProgram;
    private Matrix4 _currentWindowProjection = Matrix4.Zero;
    private float _deltaTime = 0;
    private Texture _myTexture;
    public MyWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }
    
    protected override void OnLoad()
    {
        CursorGrabbed = true;
        GL.ClearColor(ShaderLoader.HexToFloat(135), ShaderLoader.HexToFloat(206), ShaderLoader.HexToFloat(235), 1);
        GL.Enable(EnableCap.DepthTest);
        
        _shaderProgram = ShaderLoader.LoadShaderProgram("../../../resources/shader/default.vert", "../../../resources/shader/default.frag");
        _myTexture = new Texture("../../../resources/texture/large_square_pattern_01_diff_2k.jpg");
        
        RenderView._Window = this;
        List<string> paths = new List<string>()
        {
            "negx","negy","negz","posx","posy","posz"
        };
        WorldSettings.LoadCubemap(WorldSettings.PathHelper(paths));
        ShaderProgram _skyBoxShader = ShaderLoader.LoadShaderProgram("../../../resources/shader/skybox.vert", "../../../resources/shader/skybox.frag");
        WorldSettings.shader = _skyBoxShader;
        WorldSettings.genVao();


        AssimpLoaderTest cubeObject = new AssimpLoaderTest("../../../resources/model/teapot.obj");
        
        Entity entity1 = new Entity();
        entity1.AddComponent(new Transform());
        entity1.GetComponent<Transform>().Scale = new OpenTK.Mathematics.Vector3(1,1,1);
        ModelRenderer modelRenderer = new ModelRenderer(cubeObject, entity1, _myTexture, _shaderProgram);
        entity1.AddComponent(modelRenderer);

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
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Camera currentCameraObject = CameraSystem.currentCamera.GetComponent<Camera>();
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