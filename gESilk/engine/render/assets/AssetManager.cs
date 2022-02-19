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
        foreach (Asset component in Components) component.Delete();
    }

    public static void Remove(Asset asset)
    {
        for (int index = 0; index < Components.Count; index++)
        {
            Asset item = Components[index];
            if (asset == item)
            {
                Components.RemoveAt(index);
            }
        }
    }
}