namespace FileConversionLibrary.Interfaces;

public interface ICsvConverter
{
    Task ConvertAsync(string csvFilePath, string outputFilePath, char delimiter = ',');
}