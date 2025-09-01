namespace FileConversionLibrary.Interfaces;

public interface IFileReader<T>
{
    Task<T> ReadAsync(string filePath, object? options = null);
}