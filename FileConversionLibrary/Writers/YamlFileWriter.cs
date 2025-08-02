using System.Text;
using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Writers;

public class YamlFileWriter : IFileWriter<string>
{
    private readonly IExceptionHandler? _exceptionHandler;

    public YamlFileWriter(IExceptionHandler? exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task WriteAsync(string filePath, string data, object? options = null)
    {
        try
        {
            if (!Path.GetExtension(filePath).Equals(".yaml", StringComparison.OrdinalIgnoreCase) &&
                !Path.GetExtension(filePath).Equals(".yml", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.ChangeExtension(filePath, ".yaml");
            }

            data = CleanupYamlOutput(data);

            await File.WriteAllTextAsync(filePath, data);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private string CleanupYamlOutput(string yamlContent)
    {
        if (string.IsNullOrEmpty(yamlContent))
            return yamlContent;

        var lines = yamlContent.Split('\n');
        var result = new StringBuilder();

        int emptyLineCount = 0;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                emptyLineCount++;
                if (emptyLineCount <= 2)
                {
                    result.AppendLine();
                }
            }
            else
            {
                emptyLineCount = 0;
                result.AppendLine(line);
            }
        }

        return result.ToString();
    }
}