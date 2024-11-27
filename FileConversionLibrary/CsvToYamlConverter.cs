using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FileConversionLibrary;

public class CsvToYamlConverter : ICsvConverter
{
    public async Task ConvertAsync(string csvFilePath, string yamlOutputPath, char delimiter = ',')
    {
        try
        {
            Console.WriteLine($"Reading CSV file from: {csvFilePath}");
            var csvData = await CsvHelperFile.ReadCsvAsync(csvFilePath, delimiter);

            var yamlLines = new List<string>();
            
            yamlLines.Add($"- {string.Join(";", csvData.Headers)}");
            
            foreach (var row in csvData.Rows)
            {
                yamlLines.Add($"- {string.Join(";", row)}");
            }

            var yamlContent = string.Join(Environment.NewLine, yamlLines);

            Console.WriteLine($"Saving YAML file to: {yamlOutputPath}");
            await File.WriteAllTextAsync(yamlOutputPath, yamlContent);
            Console.WriteLine("YAML file saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}