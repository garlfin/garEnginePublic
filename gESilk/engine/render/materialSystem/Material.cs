using gESilk.engine.components;
using gESilk.engine.render.assets;
using gESilk.engine.render.materialSystem.settings;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.materialSystem;

public class Material
{
    private readonly Application _application;
    private readonly DepthFunction _function;
    private readonly CachedUniform<Vector3> _lightPos;
    private readonly CachedUniform<Matrix4> _lightProj;
    private readonly CachedUniform<Matrix4> _lightView;
    private readonly CachedUniform<Matrix4> _model;
    private readonly ShaderProgram _program;
    private readonly CachedUniform<Matrix4> _projection;
    private readonly List<ShaderSetting> _settings = new();
    private readonly CachedUniform<int> _shadowMap;
    private readonly CachedUniform<int> _skybox;
    private readonly CachedUniform<int> _brdfLUT;
    private readonly CachedUniform<int> _irradiance;
    private readonly CachedUniform<Matrix4> _view;
    private readonly CachedUniform<Vector3> _viewPos;
    private readonly CachedUniform<Vector3> _cubemapLoc;
    private readonly CachedUniform<Vector3> _cubemapScale;
    private readonly CachedUniform<int> _cubemapGlobal;
    private CullFaceMode _cullFaceMode;


    public Material(ShaderProgram program, Application application, DepthFunction function = DepthFunction.Less,
        CullFaceMode cullFaceMode = CullFaceMode.Back)
    {
        _program = program;
        _application = application;
        _function = function;
        _cullFaceMode = cullFaceMode;

        _model = new CachedUniform<Matrix4>(_program, "model");
        _view = new CachedUniform<Matrix4>(_program, "view");
        _projection = new CachedUniform<Matrix4>(_program, "projection");
        _viewPos = new CachedUniform<Vector3>(_program, "viewPos");
        _lightProj = new CachedUniform<Matrix4>(_program, "lightProjection");
        _lightView = new CachedUniform<Matrix4>(_program, "lightView");
        _skybox = new CachedUniform<int>(_program, "skyBox");
        _shadowMap = new CachedUniform<int>(_program, "shadowMap");
        _lightPos = new CachedUniform<Vector3>(_program, "lightPos");
        _cubemapLoc = new CachedUniform<Vector3>(_program, "cubemapLoc");
        _cubemapScale = new CachedUniform<Vector3>(_program, "cubemapScale");
        _cubemapGlobal = new CachedUniform<int>(_program, "skyboxGlobal");
        _irradiance = new CachedUniform<int>(_program, "irradianceTex");
        _brdfLUT = new CachedUniform<int>(_program, "brdfLUT");
    }

    public void SetCullMode(CullFaceMode mode)
    {
        _cullFaceMode = mode;
    }

    public DepthFunction GetDepthFunction()
    {
        return _function;
    }


    public void Use(bool clearTranslation, Matrix4 model, CubemapCapture? cubemap, DepthFunction? function = null,
        bool doCull = true)
    {
        GL.DepthFunc(function ?? _function);
        _program.Use();
        if (!doCull) GL.Disable(EnableCap.CullFace);
        GL.CullFace(_cullFaceMode);

        if (_application.State() is EngineState.GenerateBdrfState) return;

        _model.Use(model);

        var state = _application.State();
        if (state == EngineState.RenderShadowState)
        {
            _view.Use(LightSystem.ShadowView);
            _projection.Use(LightSystem.ShadowProjection);
            GL.Disable(EnableCap.CullFace);
        }
        else
        {
            _view.Use(clearTranslation
                ? CameraSystem.CurrentCamera.View.ClearTranslation()
                : CameraSystem.CurrentCamera.View);
            _projection.Use(CameraSystem.CurrentCamera.Projection);
        }

        _viewPos.Use(CameraSystem.CurrentCamera.Owner.GetComponent<Transform>().Model.ExtractTranslation());
        _lightProj.Use(LightSystem.ShadowProjection);
        _lightView.Use(LightSystem.ShadowView);
        _shadowMap.Use(_application.ShadowTex.Use(TextureSlotManager.GetUnit()));
        _lightPos.Use(LightSystem.SunPos);

        var currentCubemap = cubemap ?? CubemapCaptureManager.GetNearest(model.ExtractTranslation());

        _cubemapLoc.Use(currentCubemap.Owner.GetComponent<Transform>().Location);
        _cubemapScale.Use(currentCubemap.Owner.GetComponent<Transform>().Scale);
        _cubemapGlobal.Use(_application.Skybox.Use(TextureSlotManager.GetUnit()));
        _skybox.Use(
            (state is EngineState.GenerateCubemapState ? _application.Skybox : currentCubemap.Get()).Use(
                TextureSlotManager.GetUnit()));
        _irradiance.Use(
            (state is EngineState.GenerateCubemapState
                ? _application.Skybox.Irradiance
                : currentCubemap.GetIrradiance()).Use(TextureSlotManager.GetUnit()));
        if (_application.State() is EngineState.RenderState or EngineState.GenerateCubemapState
            or EngineState.IterationCubemapState)
        {
            _program.SetUniform("lightsCount", LightSystem.Components.Count);
            for (var index = 0; index < LightSystem.Components.Count; index++)
            {
                var light = LightSystem.Components[index];
                _program.SetUniform($"lights[{index}].Color", light.Color);
                _program.SetUniform($"lights[{index}].Position", light.Owner.GetComponent<Transform>().Location);
                _program.SetUniform($"lights[{index}].intensity", light.Power / 50);
                _program.SetUniform($"lights[{index}].radius", light.Radius);
            }
        }

        _program.SetUniform("stage", (int)_application.State());
        _brdfLUT.Use(_application.bdrfLUT.Use(TextureSlotManager.GetUnit()));
        foreach (var setting in _settings) setting.Use(_program);
    }

    public void Cleanup()
    {
        TextureSlotManager.ResetUnit();
        GL.Enable(EnableCap.CullFace);
        foreach (var setting in _settings) setting.Cleanup(_program);
    }

    public void AddSetting(ShaderSetting setting)
    {
        _settings.Add(setting);
    }

    public void EditSetting(int index, ShaderSetting setting)
    {
        _settings[index] = setting;
    }
}