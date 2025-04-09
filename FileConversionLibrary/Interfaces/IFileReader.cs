namespace FileConversionLibrary.Interfaces;

public interface IFileReader<T>
{
    Task<T> ReadWithAutoDetectDelimiterAsync(string filePath);
}