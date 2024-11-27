namespace FileConversionLibrary.Interfaces;

public interface IXmlConverter
{
    Task ConvertAsync(string xmlFilePath, string outputFilePath);
}