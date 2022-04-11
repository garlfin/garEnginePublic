using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Windowing.Common;

namespace gESilk.engine.window;

public partial class Application
{
    public Material SkyboxMaterial;

    protected virtual void OnLoad()
    {
        InitRenderer();

        Globals.Roboto =
            FontLoader.LoadFont("../../../resources/font/roboto.xml", "../../../resources/font/roboto.bin");

        _testText = new FontRenderer(Globals.Roboto, "gE2 Demo");

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