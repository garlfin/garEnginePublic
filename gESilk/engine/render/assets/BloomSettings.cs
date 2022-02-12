namespace gESilk.engine.render.assets;

public struct BloomSettings
{
    public float Threshold = 1f;
    public float Knee = 0.1f;
    public float Intensity = 1.0f;
}

public enum BloomMode
{
    BloomModePrefilter = 0,
    BloomModeDownsample = 1,
    BloomModeUpsampleFirst = 2,
    BloomModeUpsample = 3
}