using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Writers;

public class JsonFileWriter : IFileWriter<string>
{
    private readonly IExceptionHandler _exceptionHandler;

    public JsonFileWriter(IExceptionHandler exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task WriteAsync(string filePath, string data, object options = null)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, data);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }
}