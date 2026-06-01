using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Writers;

public class WordFileWriter : IFileWriter<byte[]>
{
    private readonly IExceptionHandler? _exceptionHandler;

    public WordFileWriter(IExceptionHandler? exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task WriteAsync(string filePath, byte[] data, object? options = null)
    {
        try
        {
            if (!Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase) &&
                !Path.GetExtension(filePath).Equals(".doc", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.ChangeExtension(filePath, ".docx");
            }

            await File.WriteAllBytesAsync(filePath, data);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }
}