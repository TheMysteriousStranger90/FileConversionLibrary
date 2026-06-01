namespace FileConversionLibrary.Models.Options;

public class WordConversionOptions : ConversionOptions
{
    public bool UseTable { get; set; } = true;
    public bool AddHeaderRow { get; set; } = true;
    public bool FormatAsHierarchy { get; set; } = false;
    public string FontFamily { get; set; } = "Calibri";
    public int FontSize { get; set; } = 11;
    public bool AlternateRowColors { get; set; } = false;
    public string PageOrientation { get; set; } = "Portrait";
}