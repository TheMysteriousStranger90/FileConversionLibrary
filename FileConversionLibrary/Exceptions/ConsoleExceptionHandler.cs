using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Exceptions;

public class ConsoleExceptionHandler : IExceptionHandler
{
    public void Handle(Exception exception)
    {
        Console.WriteLine($"Error: {exception.Message}");
        if (exception.InnerException != null)
        {
            Console.WriteLine($"Inner error: {exception.InnerException.Message}");
        }
    }
}