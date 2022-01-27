using gESilk.engine.render.assets;

namespace gESilk.engine.misc;

public class AssetManager<T> where T : Asset
{
    private static readonly List<T> Components = new();

    public static void Register(T asset)
    {
        Components.Add(asset);
    }

    public static void Delete()
    {
        foreach (var component in Components) component.Delete();
    }
}