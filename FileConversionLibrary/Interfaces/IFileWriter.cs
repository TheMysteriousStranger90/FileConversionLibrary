namespace FileConversionLibrary.Interfaces;

public interface IFileWriter<T>
{
    Task WriteAsync(string filePath, T data, object options = null);
}