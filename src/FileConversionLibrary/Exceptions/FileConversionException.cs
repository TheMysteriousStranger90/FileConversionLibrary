namespace FileConversionLibrary.Exceptions;

public class FileConversionException : Exception
{
    public FileConversionException(string message) : base(message) { }
    public FileConversionException(string message, Exception innerException) : base(message, innerException) { }
}