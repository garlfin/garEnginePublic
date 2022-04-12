using System.Runtime.InteropServices;
using System.Xml;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.misc;

public static class FontLoader
{
    public static Font LoadFont(string jsonFile, string imageFile)
    {
        Font font = new Font();

        font.Program = Globals.FontProgram;

        XmlDocument document = new XmlDocument();
        document.Load(jsonFile);

        var root = document.DocumentElement;

        int width = 0;
        int height = 0;

        List<Character> characters = new List<Character>();
        foreach (XmlElement childNode in root.ChildNodes)
        {
            var elementName = childNode.Name;
            if (elementName == "atlas")
            {
                font.TexAtlas = new Atlas();
                font.TexAtlas.DistanceRange = float.Parse(childNode["distanceRange"].InnerText);
                font.TexAtlas.Size = float.Parse(childNode["size"].InnerText);
                width = font.TexAtlas.TexWidth = Int32.Parse(childNode["width"].InnerText);
                height = font.TexAtlas.TexHeight = Int32.Parse(childNode["height"].InnerText);

                byte[] imageData = File.ReadAllBytes(imageFile);
                GCHandle pinnedArray = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                font.TexAtlas.TexAtlas = new TextureFromIntPtr(font.TexAtlas.TexWidth, font.TexAtlas.TexHeight, pointer,
                    PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
                pinnedArray.Free();
            }
            else if (elementName == "name") font.Name = childNode.InnerText;
            else if (elementName == "metrics")
            {
                font.FontMetrics = new Metrics();
                font.FontMetrics.LineHeight = float.Parse(childNode["lineHeight"].InnerText);
                font.FontMetrics.Ascender = float.Parse(childNode["ascender"].InnerText);
                font.FontMetrics.Descender = float.Parse(childNode["descender"].InnerText);
            }
            else if (elementName == "glyphs")
            {
                Character character = new Character();

                character.UnicodeValue = Int32.Parse(childNode.ChildNodes[0].InnerText);
                character.StringValue = Convert.ToChar(character.UnicodeValue);
                character.Advance = float.Parse(childNode.ChildNodes[1].InnerText);

                if (character.UnicodeValue == 32)
                {
                    characters.Add(character);
                }
                else
                {
                    XmlNode bounds = childNode.ChildNodes[2];
                    float left = float.Parse(bounds.ChildNodes[0].InnerText);
                    float bottom = float.Parse(bounds.ChildNodes[1].InnerText);
                    float right = float.Parse(bounds.ChildNodes[2].InnerText);
                    float top = float.Parse(bounds.ChildNodes[3].InnerText);

                    character.PlaneBounds = new Vector3[4];

                    character.PlaneBounds[0] = new Vector3(left, top, 0f);
                    character.PlaneBounds[1] = new Vector3(left, bottom, 0f);
                    character.PlaneBounds[2] = new Vector3(right, bottom, 0f);
                    character.PlaneBounds[3] = new Vector3(right, top, 0f);

                    bounds = childNode.ChildNodes[3];
                    left = float.Parse(bounds.ChildNodes[0].InnerText);
                    bottom = float.Parse(bounds.ChildNodes[1].InnerText);
                    right = float.Parse(bounds.ChildNodes[2].InnerText);
                    top = float.Parse(bounds.ChildNodes[3].InnerText);

                    character.AtlasBounds = new Vector3[4];

                    character.AtlasBounds[0] = new Vector3(left / width, top / height, 0f);
                    character.AtlasBounds[1] = new Vector3(left / width, bottom / height, 0f);
                    character.AtlasBounds[2] = new Vector3(right / width, bottom / height, 0f);
                    character.AtlasBounds[3] = new Vector3(right / width, top / height, 0f);

                    characters.Add(character);
                }
            }
        }

        font.Characters = characters.ToArray();

        return font;
    }
}

public struct Font
{
    public ShaderProgram Program;
    public Atlas TexAtlas;
    public string Name;
    public Metrics FontMetrics;
    public Character[] Characters;
}

public struct Atlas
{
    private string _type;
    public float DistanceRange;
    public float Size;
    public int TexWidth;
    public int TexHeight;
    private string _yOrigin;
    public TextureFromIntPtr TexAtlas;

    public Atlas(string type = "", float distanceRange = default, float size = default, int texWidth = default,
        int texHeight = default, string yOrigin = "", TextureFromIntPtr texAtlas = null)
    {
        _type = type;
        DistanceRange = distanceRange;
        Size = size;
        TexWidth = texWidth;
        TexHeight = texHeight;
        _yOrigin = yOrigin;
        TexAtlas = texAtlas;
    }
}

public struct Metrics
{
    private float _emSize;
    public float LineHeight;
    public float Ascender;
    public float Descender;
    public float UnderlineY;
    public float UnderlineThickness;

    public Metrics(float emSize = default, float lineHeight = default, float ascender = default,
        float descender = default, float underlineY = default, float underlineThickness = default)
    {
        _emSize = emSize;
        LineHeight = lineHeight;
        Ascender = ascender;
        Descender = descender;
        UnderlineY = underlineY;
        UnderlineThickness = underlineThickness;
    }
}

public struct Character
{
    public int UnicodeValue;
    public char StringValue;
    public float Advance;
    public Vector3[] PlaneBounds;
    public Vector3[] AtlasBounds;

    public Character()
    {
        UnicodeValue = 0;
        StringValue = 'a';
        Advance = 0f;
        PlaneBounds = new Vector3[4];
        AtlasBounds = new Vector3[4];
    }

    public bool isCharacter(char character)
    {
        return StringValue == character;
    }

    public bool IsCharacter(int character)
    {
        return StringValue == character;
    }
}