using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using FileConversionLibrary.Readers;
using System.Text;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibrary.Converters;

public class StreamConverter : IStreamConverter
{
    private readonly IInMemoryConverter _inMemoryConverter;
    private readonly IExceptionHandler? _exceptionHandler;

    public StreamConverter(IInMemoryConverter inMemoryConverter, IExceptionHandler? exceptionHandler = null)
    {
        _inMemoryConverter = inMemoryConverter;
        _exceptionHandler = exceptionHandler;
    }

    public async Task<Stream> ConvertAsync(Stream input, ConversionOptions options)
    {
        try
        {
            var result = await ConvertToBytesAsync(input, options);
            return new MemoryStream(result);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public async Task<byte[]> ConvertToBytesAsync(Stream input, ConversionOptions options)
    {
        try
        {
            var data = await ReadDataFromStream(input, options.SourceFormat);

            return options.TargetFormat.ToLower() switch
            {
                "pdf" => ConvertToPdfBytes(data, options),
                "word" or "docx" => ConvertToWordBytes(data, options),
                _ => Encoding.UTF8.GetBytes(await ConvertToStringAsync(input, options))
            };
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public async Task<string> ConvertToStringAsync(Stream input, ConversionOptions options)
    {
        try
        {
            var data = await ReadDataFromStream(input, options.SourceFormat);

            return options.TargetFormat.ToLower() switch
            {
                "json" => ConvertToJsonString(data, options),
                "xml" => ConvertToXmlString(data, options),
                "yaml" or "yml" => ConvertToYamlString(data, options),
                "csv" => ConvertToCsvString(data, options),
                _ => throw new NotSupportedException(
                    $"Target format {options.TargetFormat} is not supported for string conversion")
            };
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private async Task<object> ReadDataFromStream(Stream input, string sourceFormat)
    {
        input.Position = 0;

        return sourceFormat.ToLower() switch
        {
            "csv" => await ReadCsvFromStream(input),
            "xml" => await ReadXmlFromStream(input),
            _ => throw new NotSupportedException($"Source format {sourceFormat} is not supported")
        };
    }

    private async Task<CsvData> ReadCsvFromStream(Stream input)
    {
        using var reader = new StreamReader(input, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, content);
            var csvReader = new CsvFileReader(_exceptionHandler);
            return await csvReader.ReadWithAutoDetectDelimiterAsync(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private async Task<XmlData> ReadXmlFromStream(Stream input)
    {
        using var reader = new StreamReader(input, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, content);
            var xmlReader = new XmlFileReader(_exceptionHandler);
            return await xmlReader.ReadWithAutoDetectDelimiterAsync(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private string ConvertToJsonString(object data, ConversionOptions options)
    {
        return data switch
        {
            CsvData csvData => _inMemoryConverter.ConvertCsvToJson(csvData, options as JsonConversionOptions),
            XmlData xmlData => _inMemoryConverter.ConvertXmlToJson(xmlData, options as JsonConversionOptions),
            _ => throw new ArgumentException("Unsupported data type for JSON conversion")
        };
    }

    private string ConvertToXmlString(object data, ConversionOptions options)
    {
        return data switch
        {
            CsvData csvData => _inMemoryConverter.ConvertCsvToXml(csvData, options as XmlConversionOptions),
            _ => throw new ArgumentException("XML conversion only supported from CSV data")
        };
    }

    private string ConvertToYamlString(object data, ConversionOptions options)
    {
        return data switch
        {
            CsvData csvData => _inMemoryConverter.ConvertCsvToYaml(csvData, options as YamlConversionOptions),
            XmlData xmlData => _inMemoryConverter.ConvertXmlToYaml(xmlData, options as YamlConversionOptions),
            _ => throw new ArgumentException("Unsupported data type for YAML conversion")
        };
    }

    private string ConvertToCsvString(object data, ConversionOptions options)
    {
        return data switch
        {
            XmlData xmlData => _inMemoryConverter.ConvertXmlToCsv(xmlData, options as CsvConversionOptions),
            _ => throw new ArgumentException("CSV conversion only supported from XML data")
        };
    }

    private byte[] ConvertToPdfBytes(object data, ConversionOptions options)
    {
        return data switch
        {
            CsvData csvData => _inMemoryConverter.ConvertCsvToPdf(csvData, options as PdfConversionOptions),
            XmlData xmlData => _inMemoryConverter.ConvertXmlToPdf(xmlData, options as PdfConversionOptions),
            _ => throw new ArgumentException("Unsupported data type for PDF conversion")
        };
    }

    private byte[] ConvertToWordBytes(object data, ConversionOptions options)
    {
        return data switch
        {
            CsvData csvData => _inMemoryConverter.ConvertCsvToWord(csvData, options as WordConversionOptions),
            XmlData xmlData => _inMemoryConverter.ConvertXmlToWord(xmlData, options as WordConversionOptions),
            _ => throw new ArgumentException("Unsupported data type for Word conversion")
        };
    }
}