namespace gESilk.engine.render.assets;

public struct BloomSettings
{
    public bool Enabled = true;
    public float Threshold = 0.8f;
    public float Knee = 0.1f;
    public float UpsampleScale = 1.0f;
    public float Intensity = 1.0f;
}

public enum BloomMode
{
    BLOOM_MODE_PREFILTER = 0,
    BLOOM_MODE_DOWNSAMPLE = 1,
    BLOOM_MODE_UPSAMPLE_FIRST = 2,
    BLOOM_MODE_UPSAMPLE = 3
}