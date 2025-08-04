using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using FileConversionLibrary.Factories;
using FileConversionLibrary.Enums;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibrary.Converters;

public class InMemoryConverter : IInMemoryConverter
{
    private readonly ConverterFactory _converterFactory;
    private readonly IExceptionHandler? _exceptionHandler;

    public InMemoryConverter(ConverterFactory converterFactory, IExceptionHandler? exceptionHandler = null)
    {
        _converterFactory = converterFactory;
        _exceptionHandler = exceptionHandler;
    }

    public string ConvertCsvToJson(CsvData data, JsonConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Json);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public byte[] ConvertCsvToPdf(CsvData data, PdfConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<CsvData, byte[]>(OutputFormat.Pdf);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public byte[] ConvertCsvToWord(CsvData data, WordConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<CsvData, byte[]>(OutputFormat.Word);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public string ConvertCsvToXml(CsvData data, XmlConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Xml);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public string ConvertCsvToYaml(CsvData data, YamlConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Yaml);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public string ConvertXmlToCsv(XmlData data, CsvConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Csv);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public string ConvertXmlToJson(XmlData data, JsonConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Json);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public byte[] ConvertXmlToPdf(XmlData data, PdfConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<XmlData, byte[]>(OutputFormat.Pdf);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public byte[] ConvertXmlToWord(XmlData data, WordConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<XmlData, byte[]>(OutputFormat.Word);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    public string ConvertXmlToYaml(XmlData data, YamlConversionOptions? options = null)
    {
        try
        {
            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Yaml);
            var optionsDict = ConvertToOptionsDict(options);
            return converter.Convert(data, optionsDict);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private Dictionary<string, object>? ConvertToOptionsDict(ConversionOptions? options)
    {
        if (options == null) return null;

        var dict = new Dictionary<string, object>();

        var properties = options.GetType().GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == nameof(ConversionOptions.CustomProperties)) continue;
            
            var value = prop.GetValue(options);
            if (value != null)
            {
                dict[ToCamelCase(prop.Name)] = value;
            }
        }

        foreach (var kvp in options.CustomProperties)
        {
            dict[kvp.Key] = kvp.Value;
        }

        return dict.Count > 0 ? dict : null;
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            return input;

        return char.ToLower(input[0]) + input.Substring(1);
    }
}