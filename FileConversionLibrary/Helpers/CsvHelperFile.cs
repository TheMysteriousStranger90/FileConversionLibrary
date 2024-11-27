using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace FileConversionLibrary.Helpers;

public static class CsvHelperFile
{
    public static async Task<(string[] Headers, List<string[]> Rows)> ReadCsvAsync(string csvFilePath, char delimiter)
    {
        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter.ToString(),
            BadDataFound = null,
        });

        var records = new List<string[]>();

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.Context.Reader.HeaderRecord?.Select(h => h.Trim()).ToArray();

        if (headers == null || headers.Length == 0)
        {
            throw new Exception("Failed to read CSV headers.");
        }

        while (await csv.ReadAsync())
        {
            var row = new string[headers.Length];
            for (var i = 0; i < headers.Length; i++)
            {
                row[i] = csv.GetField(i)?.Trim();
            }
            records.Add(row);
        }

        return (headers, records);
    }

    public static string MakeValidYamlKey(string key)
    {
        var words = key
            .Split(new[] { ' ', '-', '_', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());
    
        return string.Join("", words).Replace("/", "_");
    }
}