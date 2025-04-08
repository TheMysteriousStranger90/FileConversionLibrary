using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using Newtonsoft.Json;

namespace FileConversionLibrary.Converters;

public class CsvToJsonConverter : IConverter<CsvData, string>
{
    public string Convert(CsvData input, object options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid CSV data");
        }
        
        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include
        };
        
        return JsonConvert.SerializeObject(input, jsonSettings);
    }
}