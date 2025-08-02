using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Writers;

public class CsvFileWriter : IFileWriter<string>
{
    private readonly IExceptionHandler? _exceptionHandler;

    public CsvFileWriter(IExceptionHandler? exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task WriteAsync(string filePath, string data, object? options = null)
    {
        try
        {
            if (!Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.ChangeExtension(filePath, ".csv");
            }
            
            await File.WriteAllTextAsync(filePath, data);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }
}