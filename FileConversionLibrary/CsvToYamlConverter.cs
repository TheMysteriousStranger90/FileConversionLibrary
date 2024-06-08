using YamlDotNet.Serialization;

namespace FileConversionLibrary;

public class CsvToYamlConverter
{
    public void ConvertCsvToYaml(string csvFilePath, string yamlOutputPath, char delimiter = ',')
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"File not found: {csvFilePath}");
            }

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

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(csvData);
            File.WriteAllText(yamlOutputPath, yaml);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
