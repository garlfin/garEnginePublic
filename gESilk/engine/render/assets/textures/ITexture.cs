namespace gESilk.engine.render.assets.textures;

public class ITexture : Asset
{
    protected int _slot;
    protected int _id;
    
    public ITexture()
    {
    }

    public virtual int Use()
    {
        return 0;
    }

    public virtual void Use(int slot)
    {
        
    }

    public override void Delete()
    {
    }
}