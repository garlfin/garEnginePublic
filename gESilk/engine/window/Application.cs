using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static gESilk.engine.Globals;
using Texture = gESilk.engine.render.assets.textures.ImageTexture;


namespace gESilk.engine.window;

public partial class Application
{
    private void OnLoad()
    {
        InitRenderer();

        var cubemapTest = new Entity();
        cubemapTest.AddComponent(new Transform());
        cubemapTest.GetComponent<Transform>().Location = new Vector3(10, 0, 0);
        cubemapTest.AddComponent(new CubemapCapture(new EmptyCubemapTexture(512)));

        cubemapTest = new Entity();
        cubemapTest.AddComponent(new Transform());
        cubemapTest.GetComponent<Transform>().Location = new Vector3(-10, 0, 0);
        cubemapTest.AddComponent(new CubemapCapture(new EmptyCubemapTexture(512)));

        var loader = AssimpLoader.GetMeshFromFile("../../../resources/models/sphere.obj");
        var skyboxLoader = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        skyboxLoader.IsSkybox(true);

        var program = new ShaderProgram("../../../resources/shader/default.shader");
        var texture = new Texture("../../../resources/texture/brick_albedo.tif");
        var normal = new Texture("../../../resources/texture/brick_normal.png");

        Material material = new(program);
        material.AddSetting(new TextureSetting("albedo", texture, 1));
        material.AddSetting(new TextureSetting("normalMap", normal, 2));
        material.AddSetting(new GlobalSunPosSetting("lightPos"));
        material.AddSetting(new FloatSetting("roughness", 0.4f));
        material.AddSetting(new TextureSetting("shadowMap", _shadowTex, 5));

        var woodMaterial = new Material(program);
        woodMaterial.AddSetting(new FloatSetting("roughness", 0.8f));
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


        Skybox = new CubemapTexture(paths);
        var skyboxProgram = new ShaderProgram("../../../resources/shader/skybox.shader");
        //material.AddSetting(new TextureSetting("skyBox", skyboxTexture, 0));
        //woodMaterial.AddSetting(new TextureSetting("skyBox", skyboxTexture, 0));

        Material skyboxMaterial = new(skyboxProgram, DepthFunction.Lequal, CullFaceMode.Front);
        skyboxMaterial.AddSetting(new TextureSetting("skybox", Skybox, 0));

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
        camera.AddComponent(new Camera(43f, 0.1f, 1000f));
        camera.GetComponent<Camera>()?.Set();

        SunPos = new Vector3(11.8569f, 26.5239f, 5.77871f);


        _state = EngineState.GenerateCubemapState;
        TransformSystem.Update(0f);
        CubemapCaptureManager.Update(0f);
    }

    private void OnUpdate(FrameEventArgs args)
    {
        _time += args.Time;
        // Logic stuff here
        // generally, nothing goes here. everything should be in a component but im really lazy and i dont want to make a component that just spins the hut
        _entity.GetComponent<Transform>()!.Location = ((float)Math.Sin(_time * 3.141 / 5) * 5, 1f, 0f);
        BehaviorSystem.Update((float)args.Time);


        if (!_window.IsKeyDown(Keys.Escape) || _alreadyClosed) return;
        _alreadyClosed = true;
        OnClosing();
        _window.Close();
    }

    private void OnMouseMove(MouseMoveEventArgs args)
    {
        BehaviorSystem.UpdateMouse(args);
    }
}