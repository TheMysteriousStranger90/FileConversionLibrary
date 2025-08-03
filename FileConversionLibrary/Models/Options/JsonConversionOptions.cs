namespace FileConversionLibrary.Models.Options;

public class JsonConversionOptions : ConversionOptions
{
    public bool ConvertValues { get; set; } = true;
    public bool UseIndentation { get; set; } = true;
    public bool IncludeRowNumbers { get; set; } = false;
    public bool GroupByColumn { get; set; } = false;
    public string? GroupByColumnName { get; set; }
    public bool CreateNestedObjects { get; set; } = false;
    public string NestedSeparator { get; set; } = ".";
    public bool PreserveEmptyValues { get; set; } = false;
    public string? DateFormat { get; set; }
    public bool ConvertArrays { get; set; } = false;
    public string ArrayDelimiter { get; set; } = ";";
}