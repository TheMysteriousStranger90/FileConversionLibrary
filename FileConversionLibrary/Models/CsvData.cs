namespace FileConversionLibrary.Models;

public class CsvData
{
    public string[] Headers { get; set; }
    public List<string[]> Rows { get; set; }
}