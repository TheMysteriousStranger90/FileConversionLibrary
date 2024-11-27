using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using Newtonsoft.Json;

namespace FileConversionLibrary;

public class CsvToJsonConverter : ICsvConverter
{
    public async Task ConvertAsync(string csvFilePath, string jsonOutputPath, char delimiter = ',')
    {
        try
        {
            var csvData = await CsvHelperFile.ReadCsvAsync(csvFilePath, delimiter);
            if (csvData.Headers == null || csvData.Rows == null)
            {
                throw new Exception("Failed to read CSV data.");
            }

            var json = JsonConvert.SerializeObject(csvData, Formatting.Indented);
            
            Console.WriteLine($"Saving JSON file to: {jsonOutputPath}");
            await File.WriteAllTextAsync(jsonOutputPath, json);
            Console.WriteLine("JSON file saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}