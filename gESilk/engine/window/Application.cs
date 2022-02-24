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


namespace gESilk.engine.window;

public partial class Application
{
    private void OnLoad()
    {
        InitRenderer();

        var cubemapTest = new Entity();
        cubemapTest.AddComponent(new Transform());
        cubemapTest.GetComponent<Transform>().Location = new Vector3(0, 4, 0);
        cubemapTest.AddComponent(new CubemapCapture(new EmptyCubemapTexture(512)));


        var loader = AssimpLoader.GetMeshFromFile("../../../resources/models/table.obj");
        var skyboxLoader = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        skyboxLoader.IsSkybox(true);

        var normalTex = new ImageTexture("../../../resources/texture/Diffuse_Normal.png", this);

        var program = new ShaderProgram("../../../resources/shader/default.shader");

        Material shinyMaterial = new Material(program, this);
        shinyMaterial.AddSetting(new TextureSetting("albedo",
            new ImageTexture("../../../resources/texture/white.png", this), 1));
        shinyMaterial.AddSetting(new TextureSetting("roughnessTex",
            new ImageTexture("../../../resources/texture/black.png", this), 2));
        shinyMaterial.AddSetting(new FloatSetting("normalStrength", 0));
        shinyMaterial.AddSetting(new FloatSetting("metallic", 1f));
        shinyMaterial.AddSetting(new TextureSetting("normalMap", normalTex, 3));

        Material material = new(program, this);
        material.AddSetting(new TextureSetting("albedo",
            new ImageTexture("../../../resources/texture/Diffuse.png", this), 1));
        material.AddSetting(new TextureSetting("normalMap", normalTex, 2));
        material.AddSetting(new GlobalSunPosSetting("lightPos"));
        material.AddSetting(new TextureSetting("roughnessTex",
            new ImageTexture("../../../resources/texture/Diffuse_Roughness.png", this), 3));
        material.AddSetting(new TextureSetting("shadowMap", _shadowTex, 5));

        var woodMaterial = new Material(program, this);
        woodMaterial.AddSetting(new TextureSetting("roughnessTex",
            new ImageTexture("../../../resources/texture/rough_wood_rough_1k.jpg", this), 3));
        woodMaterial.AddSetting(new TextureSetting("albedo",
            new ImageTexture("../../../resources/texture/rough_wood_diff_1k.jpg", this), 1));
        woodMaterial.AddSetting(new TextureSetting("normalMap",
            new ImageTexture("../../../resources/texture/rough_wood_nor_dx_1k.jpg", this), 2));
        woodMaterial.AddSetting(new GlobalSunPosSetting("lightPos"));

        var basePath = "../../../resources/cubemap/";


        var paths = new List<string>
        {
            basePath + "posx.jpg", basePath + "negx.jpg", basePath + "posy.jpg", basePath + "negy.jpg",
            basePath + "posz.jpg", basePath + "negz.jpg"
        };


        Skybox = new CubemapTexture(paths);
        var skyboxProgram = new ShaderProgram("../../../resources/shader/skybox.shader");

        Material skyboxMaterial = new(skyboxProgram, this, DepthFunction.Lequal, CullFaceMode.Front);
        skyboxMaterial.AddSetting(new TextureSetting("skybox", Skybox, 0));

        var skybox = new Entity();
        skybox.AddComponent(new MaterialComponent(skyboxLoader, skyboxMaterial));
        skybox.AddComponent(new CubemapRenderer(skyboxLoader));

        _entity = new Entity();
        _entity.AddComponent(new Transform());
        _entity.AddComponent(new MaterialComponent(loader, material));
        _entity.AddComponent(new ModelRenderer(loader, this));
        _entity.AddComponent(new Transform());
        _entity.GetComponent<Transform>().Location = new Vector3(0, 3.25f, 0);
        _entity.GetComponent<Transform>().Scale = new Vector3(0.5f);

        var renderPlaneMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/plane.dae");

        var physicalPlane = new Entity();
        physicalPlane.AddComponent(new MaterialComponent(renderPlaneMesh, woodMaterial));
        physicalPlane.AddComponent(new ModelRenderer(renderPlaneMesh, this));
        physicalPlane.AddComponent(new Transform());
        physicalPlane.GetComponent<Transform>().Rotation = new Vector3(-90f, 0, 0);
        physicalPlane.GetComponent<Transform>().Scale = new Vector3(5);

        var sphereMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/sphere.obj");
        physicalPlane = new Entity();
        physicalPlane.AddComponent(new MaterialComponent(sphereMesh, shinyMaterial));
        physicalPlane.AddComponent(new ModelRenderer(sphereMesh, this, false));
        physicalPlane.AddComponent(new Transform());
        physicalPlane.GetComponent<Transform>().Location = new Vector3(0, 5, 0);


        var camera = new Entity();
        camera.AddComponent(new Transform());
        camera.AddComponent(new MovementBehavior(this, sensitivity: 0.3f));
        camera.AddComponent(new Camera(43f, 0.1f, 1000f));
        camera.GetComponent<Camera>()?.Set();

        SunPos = new Vector3(11.8569f, 26.5239f, 5.77871f);

        _state = EngineState.RenderShadowState;
        ModelRendererSystem.Update(0f);
        _state = EngineState.GenerateCubemapState;
        TransformSystem.Update(0f);
        CubemapCaptureManager.Update(0f);
    }

    private void OnUpdate(FrameEventArgs args)
    {
        _time += args.Time;
        // Logic stuff here
        // generally, nothing goes here. everything should be in a component but im really lazy and i dont want to make a component that just spins the hut
        //_entity.GetComponent<Transform>()!.Location = ((float)Math.Sin(_time * 3.141 / 5) * 5, 1f, 0f);
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