namespace FileConversionLibrary.Interfaces;

public interface IPdfGenerator
{
    byte[] ConvertGeneratePdf(object data);
}