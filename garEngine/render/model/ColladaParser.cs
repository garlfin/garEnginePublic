using System.Xml;
using System.Xml.Linq;

namespace garEngine.render.model;

public class ColladaParser
{
    public List<float> vert = new List<float>();
    public List<float> uv = new List<float>();
    public List<float> normal = new List<float>();
    public List<uint> triangle = new List<uint>();
    
    private XDocument _xmlDocument;

    public ColladaParser(string file)
    {
         _xmlDocument = XDocument.Load(file);
         var mesh = _xmlDocument.Root;
         XNamespace ns = mesh.Name.Namespace;
         mesh = _xmlDocument.Root.Element(ns + "library_geometries").Element(ns + "geometry");
         string meshname = mesh.Attribute("id").Value;
         var meshDescendants = mesh.Element(ns+"mesh").Descendants();
         foreach (var descendant in meshDescendants)
         {
             if (descendant.Attribute("id") != null && descendant.Attribute("id").Value == $"{meshname}-positions")
             {
                 foreach (var number in descendant.Element(ns+"float_array").Value
                              .Split(" ", StringSplitOptions.RemoveEmptyEntries))
                 {
                     vert.Add(float.Parse(number));
                 }
             }
             else if (descendant.Attribute("id") != null && descendant.Attribute("id").Value == $"{meshname}-map-0")
             {
                 foreach (var number in descendant.Element(ns+"float_array").Value
                              .Split(" ", StringSplitOptions.RemoveEmptyEntries))
                 {
                     uv.Add(float.Parse(number));
                 }
             }
             else if (descendant.Attribute("id") != null && descendant.Attribute("id").Value == $"{meshname}-normals")
             {
                 foreach (var number in descendant.Element(ns+"float_array").Value
                              .Split(" ", StringSplitOptions.RemoveEmptyEntries))
                 {
                     normal.Add(float.Parse(number));
                 }
             }
             else if (descendant.Name == ns+"triangles")
             {
                 foreach (var number in descendant.Element(ns+"p").Value
                              .Split(" ", StringSplitOptions.RemoveEmptyEntries).Where((x, i) => i % 3 == 0 || i==0))
                 {
                     triangle.Add(uint.Parse(number));
                 }
             }
         }
    }
}