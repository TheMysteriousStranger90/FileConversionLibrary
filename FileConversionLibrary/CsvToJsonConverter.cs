using Newtonsoft.Json;

namespace FileConversionLibrary;

public class CsvToJsonConverter
{
    public void ConvertCsvToJson(string csvFilePath, string jsonOutputPath, char delimiter = ',')
    {
        try
        {
            var csvContent = File.ReadAllLines(csvFilePath);
            var headers = csvContent[0].Split(delimiter);

            var csvData = new List<Dictionary<string, string>>();

            for (var i = 1; i < csvContent.Length; i++)
            {
                var row = csvContent[i].Split(delimiter);
                var rowData = new Dictionary<string, string>();

                for (var j = 0; j < headers.Length; j++)
                {
                    rowData.Add(headers[j], row[j]);
                }

                csvData.Add(rowData);
            }

            var json = JsonConvert.SerializeObject(csvData, Formatting.Indented);
            File.WriteAllText(jsonOutputPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}