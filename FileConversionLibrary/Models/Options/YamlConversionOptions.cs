namespace FileConversionLibrary.Models.Options;

public class YamlConversionOptions : ConversionOptions
{
    public string Structure { get; set; } = "Array";
    public string NamingConvention { get; set; } = "Original";
    public bool ConvertDataTypes { get; set; } = true;
    public bool IncludeComments { get; set; } = true;
    public bool IncludeMetadata { get; set; } = true;
    public int IndentSize { get; set; } = 2;
    public bool SortKeys { get; set; } = false;
}