namespace gESilk.engine.render.assets;

public static class AssetManager
{
    private static readonly List<Asset> Assets = new();

    public static void Register(Asset asset)
    {
        Assets.Add(asset);
    }

    public static void Delete()
    {
        foreach (var component in Assets) component.Delete();
    }

    public static void Remove(Asset asset)
    {
        // This line may be very problematic. TODO
        //asset.Delete();
        for (var index = 0; index < Assets.Count; index++)
        {
            var item = Assets[index];
            if (asset == item) Assets.RemoveAt(index);
        }
    }
}