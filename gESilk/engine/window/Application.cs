using static gESilk.engine.Globals;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Camera = gESilk.engine.components.Camera;
using Material = gESilk.engine.render.materialSystem.Material;
using Texture = gESilk.engine.render.assets.textures.ImageTexture;



namespace gESilk.engine.window;

public partial class Application
{

    private void OnLoad()
    {
        
        InitRenderer();
        
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
        material.AddSetting(new TextureSetting("shadowMap", _shadowTex, 5));
        
        var woodMaterial = new Material(program);
        woodMaterial.AddSetting(new TextureSetting("albedo",
            new Texture("../../../resources/texture/rough_wood_diff_1k.jpg"), 1));
        woodMaterial.AddSetting(new TextureSetting("normalMap",
            new Texture("../../../resources/texture/rough_wood_nor_dx_1k.jpg"), 2));
        woodMaterial.AddSetting(new GlobalSunPosSetting("lightPos"));

        var basePath = "../../../resources/cubemap/";


        var paths = new List<string>
        {
            basePath + "posx.jpg", basePath + "negx.jpg", basePath + "posy.jpg", basePath + "negy.jpg",
            basePath + "posz.jpg", basePath + "negz.jpg"
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
        
        var renderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/plane.dae");
        
        var physicalPlane = new Entity();
        physicalPlane.AddComponent(new MaterialComponent(renderPlaneMesh, material));
        physicalPlane.AddComponent(new ModelRenderer(renderPlaneMesh));
        physicalPlane.AddComponent(new Transform());
        physicalPlane.GetComponent<Transform>().Rotation = new Vector3(-90f, 0, 0);
        physicalPlane.GetComponent<Transform>().Scale = new Vector3(10);


        var camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new MovementBehavior(0.3f));
        camera.AddComponent(new Camera(72f, 0.1f, 1000f));
        camera.GetComponent<Camera>()?.Set();
        
        SunPos = new Vector3(11.8569f, 26.5239f, 5.77871f);
    }

    private void OnUpdate(FrameEventArgs args)
    {
        _time += args.Time;
        if (_time > 12) // 360 / 30 = 12 : )
            _time = 0;

        //Logic stuff here
        _entity.GetComponent<Transform>()!.Rotation = (0f, (float)_time * 30, 0f);
        BehaviorSystem.Update((float)args.Time);

        if (!_window.IsKeyDown(Keys.Escape)) return;
        if (_alreadyClosed) return;
        
        _alreadyClosed = true;
        OnClosing();
        _window.Close();
    }

    private void OnMouseMove(MouseMoveEventArgs args)
    {
        BehaviorSystem.UpdateMouse(args);
    }
}