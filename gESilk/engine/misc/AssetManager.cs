using gESilk.engine.render.assets;

namespace gESilk.engine.misc;

public class AssetManager<T> where T : Asset
{
    private static List<T> Components = new();

    public static void Register(T asset)
    {
        Components.Add(asset);
    }

    public static void Delete()
    {
        foreach (var component in Components) component.Delete();
    }

    public static void Remove(T asset)
    {
        for (var index = 0; index < Components.Count; index++)
        {
            var item = Components[index];
            if (asset == item)
            {
                Components.RemoveAt(index);
            }
        }
    }
}