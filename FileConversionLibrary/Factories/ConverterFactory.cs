using FileConversionLibrary.Converters;
using FileConversionLibrary.Enums;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Factories;

public class ConverterFactory
{
    public IConverter<TInput, TOutput> GetConverter<TInput, TOutput>(OutputFormat format = OutputFormat.Json)
    {
        if (typeof(TInput) == typeof(CsvData) && typeof(TOutput) == typeof(string))
        {
            if (format == OutputFormat.Xml)
            {
                return (IConverter<TInput, TOutput>)(object)new CsvToXmlConverter();
            }
            else if (format == OutputFormat.Yaml)
            {
                return (IConverter<TInput, TOutput>)(object)new CsvToYamlConverter();
            }
            else
            {
                return (IConverter<TInput, TOutput>)(object)new CsvToJsonConverter();
            }
        }
    
        if (typeof(TInput) == typeof(CsvData) && typeof(TOutput) == typeof(byte[]) && format == OutputFormat.Pdf)
        {
            return (IConverter<TInput, TOutput>)(object)new CsvToPdfConverter();
        }
    
        if (typeof(TInput) == typeof(CsvData) && typeof(TOutput) == typeof(byte[]) && format == OutputFormat.Word)
        {
            return (IConverter<TInput, TOutput>)(object)new CsvToWordConverter();
        }
    
        throw new NotSupportedException($"Converter from {typeof(TInput)} to {typeof(TOutput)} with format {format} is not supported");
    }
}