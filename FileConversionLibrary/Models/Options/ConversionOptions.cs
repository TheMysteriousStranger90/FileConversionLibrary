namespace FileConversionLibrary.Models.Options;

public abstract class ConversionOptions
{
    public string SourceFormat { get; set; } = string.Empty;
    public string TargetFormat { get; set; } = string.Empty;
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}