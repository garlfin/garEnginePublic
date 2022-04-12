using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using gESilk.resources.Scripts;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace gESilk.engine.window;

public partial class Application
{
    public Material SkyboxMaterial;

    public Vector3 InverseScreen
    {
        get;
        private set;
    }

    protected virtual void OnLoad()
    {
        InitRenderer();

        InverseScreen = new Vector3((float) _height / _width, 1, 1);

        Globals.Roboto =
            FontLoader.LoadFont("../../../resources/font/roboto.xml", "../../../resources/font/roboto.bin");

        _testText = new Entity(this);
        _testText.AddComponent(new Transform());
        _testText.GetComponent<Transform>().Location = new Vector3(10, 10, 0).PixelToScreen(_width, _height);
        _testText.GetComponent<Transform>().Scale = new Vector3(0.05f);
        _testText.AddComponent(new TextRenderer(Globals.Roboto, "gE2 Demo"));
        _testText.AddComponent(new FPS());

        Skybox = new CubemapTexture("../../../resources/texture/autumn.exr", this);

        var skybox = new Entity(this);

        skybox.AddComponent(new CubemapRenderer(this));

        SkyboxMaterial = new Material(new ShaderProgram("../../../resources/shader/skybox.glsl"), this);
        SkyboxMaterial.AddSetting(new TextureSetting("skybox", Skybox));

        MapLoader.LoadMap("../../../resources/maps/test.map", this);

        var previousLight = LightSystem.CurrentLight;

        TransformSystem.Update(0f);

        _state = EngineState.RenderLinearShadowState;
        foreach (var light in LightSystem.Components)
        {
            light.UpdateShadowMatrices();
        }

        previousLight.Set();

        _state = EngineState.GenerateCubemapState;
        BakeCubemaps();
        _state = EngineState.IterationCubemapState;
        BakeCubemaps();
        foreach (var cubemap in CubemapCaptureManager.Components) cubemap.DeleteOldAssets();
    }

    protected virtual void OnUpdate(FrameEventArgs args)
    {
        _time += args.Time;
        BehaviorSystem.Update((float)args.Time);
    }

    protected virtual void OnMouseMove(MouseMoveEventArgs args)
    {
        BehaviorSystem.UpdateMouse(args);
    }
}