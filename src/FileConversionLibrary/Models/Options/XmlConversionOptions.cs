namespace FileConversionLibrary.Models.Options;

public class XmlConversionOptions : ConversionOptions
{
    public string OutputFormat { get; set; } = "Elements";
    public bool UseCData { get; set; } = true;
    public bool IncludeTimestamp { get; set; } = false;
    public bool IncludeRowNumbers { get; set; } = false;
    public string NamingConvention { get; set; } = "Original";
    public bool AddComments { get; set; } = false;
    public bool IncludeMetadata { get; set; } = false;
}