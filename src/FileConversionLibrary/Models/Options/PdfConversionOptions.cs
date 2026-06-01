namespace FileConversionLibrary.Models.Options;

public class PdfConversionOptions : ConversionOptions
{
    public float FontSize { get; set; } = 10f;
    public bool AddBorders { get; set; } = true;
    public bool AlternateRowColors { get; set; } = false;
    public string Title { get; set; } = "Data Export";
    public bool IncludeTimestamp { get; set; } = true;
    public bool IncludeRowNumbers { get; set; } = false;
    public bool LandscapeOrientation { get; set; } = false;
    public string FontFamily { get; set; } = "Helvetica";
}