using System.ComponentModel.DataAnnotations;

namespace FileConversionLibrary.Models;

public class CsvData
{
    [Required]
    public string[] Headers { get; set; } = Array.Empty<string>();
    
    [Required]
    public List<string[]> Rows { get; set; } = new List<string[]>();
}