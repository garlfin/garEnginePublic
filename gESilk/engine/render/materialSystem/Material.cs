using gESilk.engine.components;
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
        _skybox = new CachedUniform<int>(_program, "localCubemap.cubemap");
        _shadowMap = new CachedUniform<int>(_program, "shadowMap");
        _lightPos = new CachedUniform<Vector3>(_program, "lightPos");
        _cubemapLoc = new CachedUniform<Vector3>(_program, "localCubemap.Position");
        _cubemapScale = new CachedUniform<Vector3>(_program, "localCubemap.Scale");
        _cubemapGlobal = new CachedUniform<int>(_program, "skyboxGlobal");
        _irradiance = new CachedUniform<int>(_program, "localCubemap.irradiance");
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
        _program.Use();

        // Various Setup
        GL.DepthFunc(function ?? _function);
        if (doCull) GL.CullFace(_cullFaceMode);
        else GL.Disable(EnableCap.CullFace);

        var state = _application.AppState;

        _model.Use(model);
        if (state is EngineState.RenderShadowState or EngineState.RenderLinearShadowState)
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

        // Point lights need this information too
        _lightPos.Use(LightSystem.CurrentLight.Owner.GetComponent<Transform>().Model.ExtractTranslation());

        if (state is EngineState.RenderDepthState or EngineState.RenderShadowState
            or EngineState.RenderLinearShadowState) return;

        _viewPos.Use(CameraSystem.CurrentCamera.Owner.GetComponent<Transform>().Model.ExtractTranslation());
        _lightProj.Use(LightSystem.ShadowProjection);
        _lightView.Use(LightSystem.ShadowView);
        _shadowMap.Use(_application.ShadowTex.Use(TextureSlotManager.GetUnit()));

        var currentCubemap = cubemap ?? CubemapCaptureManager.GetNearest(model.ExtractTranslation());

        _cubemapLoc.Use(currentCubemap.Owner.GetComponent<Transform>().Location);
        _cubemapScale.Use(currentCubemap.Owner.GetComponent<Transform>().Scale);
        _cubemapGlobal.Use(_application.Skybox.Use(TextureSlotManager.GetUnit()));

        // In my head it makes more sense to use if instead of ternary operator
        if (state is EngineState.GenerateCubemapState)
        {
            _skybox.Use(_application.Skybox.Use(TextureSlotManager.GetUnit()));
            _irradiance.Use(_application.Skybox.Irradiance.Use(TextureSlotManager.GetUnit()));
        }
        else
        {
            _skybox.Use(currentCubemap.Get().Use(TextureSlotManager.GetUnit()));
            _irradiance.Use(currentCubemap.GetIrradiance().Use(TextureSlotManager.GetUnit()));
        }

        _program.SetUniform("lightsCount", LightSystem.Components.Count);
        var currentUnit = TextureSlotManager.GetUnit();
        for (var index = 0; index < 10; index++) // 10 lights is max
        {
            if (index < LightSystem.Components.Count)
            {
                var light = LightSystem.Components[index];
                _program.SetUniform($"lights[{index}].Color", light.Color);
                _program.SetUniform($"lights[{index}].Position", light.Owner.GetComponent<Transform>().Location);
                _program.SetUniform($"lights[{index}].intensity", light.Power / 25);
                _program.SetUniform($"lights[{index}].radius", light.Radius);
                _program.SetUniform($"lights[{index}].shadowMap",
                    light.GetShadowMap().Use(TextureSlotManager.GetUnit()));
                currentUnit = TextureSlotManager.GetUnit();
            }
            else // Fill in the rest of the slots with empty textures cause opengl was crying about it
            {
                _program.SetUniform($"lights[{index}].Color", Vector3.Zero);
                _program.SetUniform($"lights[{index}].Position", Vector3.Zero);
                _program.SetUniform($"lights[{index}].intensity", 1f);
                _program.SetUniform($"lights[{index}].radius", 1f);
                GL.ActiveTexture(TextureUnit.Texture0 + currentUnit);
                GL.BindTexture(TextureTarget.TextureCubeMap, 0);
                _program.SetUniform($"lights[{index}].shadowMap", currentUnit);
            }
        }

        var closestInOrder = CubemapCaptureManager.ReturnClosestInOrder(model.ExtractTranslation());
        for (int i = 0; i < 4; i++)
        {
            currentCubemap = closestInOrder[i].Item2;
            _program.SetUniform($"irradiances[{i}].Position", currentCubemap.Owner.GetComponent<Transform>().Location);
            _program.SetUniform($"irradiances[{i}].Scale", currentCubemap.Owner.GetComponent<Transform>().Scale);
            _program.SetUniform($"irradiances[{i}].irradiance",
                currentCubemap.GetIrradiance().Use(TextureSlotManager.GetUnit()));
        }

        _program.SetUniform("stage", (int)_application.AppState);
        _brdfLUT.Use(_application.BrdfLut.Use(TextureSlotManager.GetUnit()));
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