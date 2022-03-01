using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace gESilk.engine.window;

public partial class Application
{
    protected virtual void OnLoad()
    {
        InitRenderer();
        var skyboxLoader = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        skyboxLoader.IsSkybox(true);
        /*
        var loader = AssimpLoader.GetMeshFromFile("../../../resources/models/table.obj");

        var normalTex = new ImageTexture("../../../resources/texture/Diffuse_Normal.png", this);

        var program = new ShaderProgram("../../../resources/shader/default.glsl");

        var shinyMaterial = new Material(program, this);
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

        var woodMaterial = new Material(program, this);
        woodMaterial.AddSetting(new TextureSetting("roughnessTex",
            new ImageTexture("../../../resources/texture/rough_wood_rough_1k.jpg", this), 3));
        woodMaterial.AddSetting(new TextureSetting("albedo",
            new ImageTexture("../../../resources/texture/rough_wood_diff_1k.jpg", this), 1));
        woodMaterial.AddSetting(new TextureSetting("normalMap",
            new ImageTexture("../../../resources/texture/rough_wood_nor_dx_1k.jpg", this), 2));
        woodMaterial.AddSetting(new GlobalSunPosSetting("lightPos"));*/

        var basePath = "../../../resources/cubemap/";


        var paths = new List<string>
        {
            basePath + "posx.jpg", basePath + "negx.jpg", basePath + "posy.jpg", basePath + "negy.jpg",
            basePath + "posz.jpg", basePath + "negz.jpg"
        };

        Skybox = new CubemapTexture(paths);
        var skyboxProgram = new ShaderProgram("../../../resources/shader/skybox.glsl");
        Material skyboxMaterial = new(skyboxProgram, this, DepthFunction.Lequal, CullFaceMode.Front);
        skyboxMaterial.AddSetting(new TextureSetting("skybox", Skybox));

        var skybox = new Entity(this);
        skybox.AddComponent(new MaterialComponent(skyboxLoader, skyboxMaterial));
        skybox.AddComponent(new CubemapRenderer(skyboxLoader));

        /*_entity = new Entity();
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
        _entity = new Entity();
        _entity.AddComponent(new MaterialComponent(sphereMesh, shinyMaterial));
        _entity.AddComponent(new ModelRenderer(sphereMesh, this, false));
        _entity.AddComponent(new Transform());
        _entity.GetComponent<Transform>().Location = new Vector3(0, 5, 0);*/

        MapLoader.LoadMap("../../../resources/maps/test.map", this);

        BakeCubemaps();
    }

    protected virtual void OnUpdate(FrameEventArgs args)
    {
        _time += args.Time;
        // Logic stuff here
        // generally, nothing goes here. everything should be in a component but im really lazy and i dont want to make a component that just moves the sphere
        //_entity.GetComponent<Transform>()!.Location = ((float) Math.Sin(_time * 3.141 / 5) * 5, 5f, 0f);
        BehaviorSystem.Update((float)args.Time);

        if (!_window.IsKeyDown(Keys.Escape) || _alreadyClosed) return;
        _alreadyClosed = true;
        OnClosing();
        _window.Close();
    }

    protected virtual void OnMouseMove(MouseMoveEventArgs args)
    {
        BehaviorSystem.UpdateMouse(args);
    }
}