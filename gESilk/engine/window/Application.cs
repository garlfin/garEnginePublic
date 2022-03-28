using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace gESilk.engine.window;

public partial class Application
{
    public Material SkyboxMaterial;

    protected virtual void OnLoad()
    {
        InitRenderer();

        var basePath = "../../../resources/cubemap/";


        Skybox = new CubemapTexture("../../../resources/texture/autumn.exr", this);

        var skybox = new Entity(this);

        skybox.AddComponent(new CubemapRenderer(this));

        SkyboxMaterial = new Material(new ShaderProgram("../../../resources/shader/skybox.glsl"), this);
        SkyboxMaterial.AddSetting(new TextureSetting("skybox", Skybox));

        MapLoader.LoadMap("../../../resources/maps/test.map", this);

        var previousLight = LightSystem.CurrentLight;

        _state = EngineState.RenderPointShadowState;
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