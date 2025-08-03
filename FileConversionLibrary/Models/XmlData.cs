using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace FileConversionLibrary.Models;

public class XmlData
{
    [Required]
    public XDocument Document { get; set; } = new XDocument();
    
    [Required]
    public string[] Headers { get; set; } = Array.Empty<string>();
    
    [Required]
    public List<string[]> Rows { get; set; } = new List<string[]>();

    [Required]
    public string RootElementName { get; set; } = string.Empty;
    
    [Required]
    public string XmlVersion { get; set; } = "1.0";
    
    [Required]
    public string Encoding { get; set; } = "UTF-8";
}