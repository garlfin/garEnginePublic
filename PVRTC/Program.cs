namespace PVRTC;

public static class Program
{
    public static Image GetImageFromFile(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();
        Image image = new Image();
        var file = File.Open(path, FileMode.Open);
        long length = file.Length;
        var reader = new BinaryReader(file);

        reader.ReadUInt32();
        reader.ReadUInt32();
        image.Format = (Format) reader.ReadUInt64();
        reader.ReadUInt32();
        image.Type = (ChannelType) reader.ReadUInt32();
        image.Width = reader.ReadUInt32();
        image.Height = reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        uint mipCount = reader.ReadUInt32();
        uint metaDataSize = reader.ReadUInt32();
        if (mipCount > 1) throw new Exception("No mips supported");
        reader.ReadBytes((int) metaDataSize);
        image.ImageData = reader.ReadBytes((int) (length - metaDataSize - 52));


        reader.Close();
        file.Close();

        return image;
    }
}
public struct Image
{
    public Format Format;
    public ChannelType Type;
    public uint Width, Height;
    public byte[] ImageData;

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Format);
        writer.Write((byte) Type);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(ImageData.Length);
        writer.Write(ImageData);
    }
}

public enum Format
{
    BC1 = 7,
    BC2 = 9,
    BC3 = 11,
    BC4 = 12,
    BC5 = 13
}

public enum ChannelType
{
    UByteN = 0,
    ByteN = 1,
    UByte = 2,
    Byte = 3,
    UShortN = 4,
    ShortN = 5,
    UShort = 6,
    Short = 7,
    UIntN = 8,
    IntN = 9,
    UInt = 10,
    Int = 11,
    Float = 12
}