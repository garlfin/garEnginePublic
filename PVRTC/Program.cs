namespace PVRTC;

public static class Program
{
    public static void Main(string[] args)
    {
        var file = File.Open("test.pvr", FileMode.Open);
        var reader = new BinaryReader(file);

        uint version = reader.ReadUInt32();
        uint flags = reader.ReadUInt32();
        Format format = (Format)reader.ReadUInt64();
        ColorSpace colorSpace = (ColorSpace)reader.ReadUInt32();
        ChannelType channelType = (ChannelType)reader.ReadUInt32();
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        uint depth = reader.ReadUInt32();
        uint surfaces = reader.ReadUInt32();
        uint faces = reader.ReadUInt32();
        uint mipCount = reader.ReadUInt32();
        uint metaDataSize = reader.ReadUInt32();
        Console.WriteLine(
            $"Version: {version} | Flags: {flags} | Format: {format} | Color Space: {colorSpace} | Channel Type: {channelType} | Width: {width} | Height: {height} | Depth: {depth} | Surfaces: {surfaces} | Faces: {faces} | Mip-Map Count: {mipCount}");
        if (depth + surfaces + faces + mipCount > 4) throw new Exception("Only one texture supported.");
        reader.ReadBytes((int)metaDataSize);

        reader.Close();
        file.Close();
    }

    public enum Format
    {
        PVRTC_2BPP_RGB = 0,
        PVRTC_2BPP_RGBA = 1,
        PVRTC_4BPP_RGB = 2,
        PVRTC_4BPP_RGBA = 3,
    }

    public enum ColorSpace
    {
        Linear = 0,
        sRGB = 1
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
}