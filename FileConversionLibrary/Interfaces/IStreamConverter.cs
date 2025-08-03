using FileConversionLibrary.Models.Options;

namespace FileConversionLibrary.Interfaces;

public interface IStreamConverter
{
    Task<Stream> ConvertAsync(Stream input, ConversionOptions options);
    Task<byte[]> ConvertToBytesAsync(Stream input, ConversionOptions options);
    Task<string> ConvertToStringAsync(Stream input, ConversionOptions options);
}