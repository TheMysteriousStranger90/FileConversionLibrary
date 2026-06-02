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
    public string Title { get; set; } = "CSV Data Export";
    public bool IncludeTimestamp { get; set; } = true;
    public bool IncludeRowNumbers { get; set; } = false;
    public bool AddBorders { get; set; } = true;
    public bool AutoFitTable { get; set; } = true;
    public bool WrapText { get; set; } = true;
    public bool IncludeStatistics { get; set; } = true;
}