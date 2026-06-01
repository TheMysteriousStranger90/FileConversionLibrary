using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace FileConversionLibrary.Models;

public class XmlData
{
    public XDocument Document { get; set; } = new XDocument();
    
    [Obsolete("Consider using Document property for better XML handling. This property is maintained for backward compatibility.")]
    public string[] Headers { get; set; } = Array.Empty<string>();
    
    [Obsolete("Consider using Document property for better XML handling. This property is maintained for backward compatibility.")]
    public List<string[]> Rows { get; set; } = new List<string[]>();

    [Required]
    public string RootElementName { get; set; } = string.Empty;
    
    [Required]
    public string XmlVersion { get; set; } = "1.0";
    
    [Required]
    public string Encoding { get; set; } = "UTF-8";
}