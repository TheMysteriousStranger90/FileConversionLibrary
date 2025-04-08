using System.Xml.Linq;

namespace FileConversionLibrary.Models;

public class XmlData
{
    public XDocument Document { get; set; }
    public string[] Headers { get; set; }
    public List<string[]> Rows { get; set; }
}