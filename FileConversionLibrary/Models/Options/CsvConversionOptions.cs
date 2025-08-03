namespace FileConversionLibrary.Models.Options;

public class CsvConversionOptions : ConversionOptions
{
    public char Delimiter { get; set; } = ',';
    public bool QuoteValues { get; set; } = true;
}