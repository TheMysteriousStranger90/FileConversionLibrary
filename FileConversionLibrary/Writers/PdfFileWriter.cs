using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Writers;

public class PdfFileWriter : IFileWriter<byte[]>
{
    private readonly IExceptionHandler _exceptionHandler;

    public PdfFileWriter(IExceptionHandler exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task WriteAsync(string filePath, byte[] data, object options = null)
    {
        try
        {
            await File.WriteAllBytesAsync(filePath, data);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }
}