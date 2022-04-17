namespace PVRTC;

public static class Program
{
    public static void Main(string[] args)
    {
        var file = File.Open("test.pvr", FileMode.Open);
        long length = file.Length;
        var reader = new BinaryReader(file);

        reader.ReadUInt32();
        reader.ReadUInt32();
        Format format = (Format)reader.ReadUInt64();
        reader.ReadUInt32();
        ChannelType channelType = (ChannelType)reader.ReadUInt32();
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        uint mipCount = reader.ReadUInt32();
        uint metaDataSize = reader.ReadUInt32();
        Console.WriteLine(
            $"Format: {format} | Channel Type: {channelType} | Width: {width} | Height: {height}");
        if (mipCount > 1) throw new Exception("No mips supported");
        reader.ReadBytes((int)metaDataSize);
        byte[] imageData = reader.ReadBytes((int)(length - metaDataSize - 52));


        reader.Close();
        file.Close();
    }

    private enum Format
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
}