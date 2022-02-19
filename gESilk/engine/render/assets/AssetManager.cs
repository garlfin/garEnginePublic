namespace gESilk.engine.render.assets;

public class AssetManager
{
    private static List<Asset> Components = new();

    public static void Register(Asset asset)
    {
        Components.Add(asset);
    }

    public static void Delete()
    {
        foreach (var component in Components) component.Delete();
    }

    public static void Remove(Asset asset)
    {
        for (var index = 0; index < Components.Count; index++)
        {
            var item = Components[index];
            if (asset == item) Components.RemoveAt(index);
        }
    }
}