using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using System.Globalization;
using System.Text;
using CsvHelper.Configuration;

namespace FileConversionLibrary.Readers;

public class CsvFileReader : IFileReader<CsvData>
{
    private readonly IExceptionHandler _exceptionHandler;

    public CsvFileReader(IExceptionHandler exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }


    public async Task<CsvData> ReadWithAutoDetectDelimiterAsync(string filePath)
    {
        try
        {
            var firstLines = File.ReadLines(filePath).Take(5).ToList();
            if (firstLines.Count == 0)
                throw new Exception("Empty CSV file");

            var possibleDelimiters = new[] { ',', ';', '\t', '|' };

            var delimiterCounts = possibleDelimiters.ToDictionary(
                d => d,
                d => firstLines.Select(line => line.Count(c => c == d)).ToList());

            char detectedDelimiter = ',';
            int bestConsistency = -1;

            foreach (var kvp in delimiterCounts)
            {
                if (kvp.Value.Sum() == 0) continue;

                var consistentCount = kvp.Value.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Count();

                if (consistentCount > bestConsistency)
                {
                    bestConsistency = consistentCount;
                    detectedDelimiter = kvp.Key;
                }
            }

            return await ReadAsync(filePath, detectedDelimiter);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private async Task<CsvData> ReadAsync(string filePath, object options = null)
    {
        var delimiter = options is char ? (char)options : ',';

        try
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8);
            using var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(),
                BadDataFound = null,
                Escape = '"',
                Quote = '"',
                IgnoreBlankLines = true,
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim
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

            return new CsvData { Headers = headers, Rows = records };
        }
        catch (CsvHelper.MissingFieldException)
        {
            return ReadWithCustomParser(filePath, delimiter);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private CsvData ReadWithCustomParser(string filePath, char delimiter)
    {
        try
        {
            var contents = File.ReadAllText(filePath);
            var headers = new List<string>();
            var rows = new List<string[]>();

            var lines = contents.Split('\n');
            if (lines.Length == 0)
                throw new Exception("Empty CSV file");

            var headerLine = lines[0].TrimEnd('\r');
            headers.AddRange(ParseCsvLine(headerLine, delimiter));

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = ParseCsvLine(line, delimiter);
                rows.Add(fields.ToArray());
            }

            _exceptionHandler?.Handle(new Exception($"Used custom CSV parser for {filePath}"));
            return new CsvData { Headers = headers.ToArray(), Rows = rows };
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private List<string> ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var fieldBuilder = new StringBuilder();
        bool isEscaped = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (isEscaped && i + 1 < line.Length && line[i + 1] == '"')
                {
                    fieldBuilder.Append('"');
                    i++;
                }
                else
                {
                    isEscaped = !isEscaped;
                }

                continue;
            }

            if (isEscaped)
            {
                fieldBuilder.Append(c);
                continue;
            }

            if (c == delimiter)
            {
                fields.Add(fieldBuilder.ToString().Trim());
                fieldBuilder.Clear();
                continue;
            }

            fieldBuilder.Append(c);
        }

        fields.Add(fieldBuilder.ToString().Trim());

        return fields;
    }
}