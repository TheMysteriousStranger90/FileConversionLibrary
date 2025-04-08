using FileConversionLibrary.Converters;
using FileConversionLibrary.Enums;
using FileConversionLibrary.Exceptions;
using FileConversionLibrary.Factories;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary;

public class FileConverterFacade
{
    private readonly IFileReader<XmlData> _xmlReader;
    private readonly IFileWriter<string> _csvWriter;
    private readonly IFileReader<CsvData> _csvReader;
    private readonly IFileWriter<byte[]> _pdfWriter;
    private readonly IFileWriter<byte[]> _wordWriter;
    private readonly IFileWriter<string> _jsonWriter;
    private readonly IFileWriter<string> _xmlWriter;
    private readonly IFileWriter<string> _yamlWriter;
    private readonly ConverterFactory _converterFactory;
    private readonly IExceptionHandler _exceptionHandler;

    public FileConverterFacade(
        IFileReader<CsvData> csvReader,
        IFileWriter<string> jsonWriter,
        IFileWriter<string> xmlWriter,
        IFileWriter<string> yamlWriter,
        IFileWriter<byte[]> pdfWriter,
        IFileWriter<byte[]> wordWriter,
        IFileReader<XmlData> xmlReader,
        IFileWriter<string> csvWriter,
        ConverterFactory converterFactory,
        IExceptionHandler exceptionHandler)
    {
        _csvReader = csvReader;
        _jsonWriter = jsonWriter;
        _xmlWriter = xmlWriter;
        _yamlWriter = yamlWriter;
        _pdfWriter = pdfWriter;
        _wordWriter = wordWriter;
        _xmlReader = xmlReader;
        _csvWriter = csvWriter;
        _converterFactory = converterFactory;
        _exceptionHandler = exceptionHandler;
    }
    
    public async Task ConvertCsvToJsonAsync(string csvFilePath, string jsonOutputPath)
    {
        try
        {
            var csvData = await _csvReader.ReadWithAutoDetectDelimiterAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Json);
            var json = converter.Convert(csvData);

            await _jsonWriter.WriteAsync(jsonOutputPath, json);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {jsonOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToPdfAsync(string csvFilePath, string pdfOutputPath)
    {
        try
        {
            var csvData = await _csvReader.ReadWithAutoDetectDelimiterAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, byte[]>(OutputFormat.Pdf);
            var pdfData = converter.Convert(csvData);

            await _pdfWriter.WriteAsync(pdfOutputPath, pdfData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {pdfOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToWordAsync(string csvFilePath, string wordOutputPath)
    {
        try
        {
            var csvData = await _csvReader.ReadWithAutoDetectDelimiterAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, byte[]>(OutputFormat.Word);
            var wordData = converter.Convert(csvData);

            await _wordWriter.WriteAsync(wordOutputPath, wordData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {wordOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToXmlAsync(
        string csvFilePath,
        string xmlOutputPath,
        CsvToXmlConverter.XmlOutputFormat format = CsvToXmlConverter.XmlOutputFormat.Elements,
        bool useCData = true,
        bool useTabsForIndentation = false,
        int indentSize = 2)
    {
        try
        {
            var csvData = await _csvReader.ReadWithAutoDetectDelimiterAsync(csvFilePath);

            var converterOptions = new Dictionary<string, object>
            {
                ["format"] = format,
                ["useCData"] = useCData
            };

            var writerOptions = new Dictionary<string, object>
            {
                ["useIndent"] = true,
                ["useTabs"] = useTabsForIndentation,
                ["indentSize"] = indentSize
            };

            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Xml);
            var xml = converter.Convert(csvData, converterOptions);

            await _xmlWriter.WriteAsync(xmlOutputPath, xml, writerOptions);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {xmlOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToYamlAsync(string csvFilePath, string yamlOutputPath)
    {
        try
        {
            var csvData = await _csvReader.ReadWithAutoDetectDelimiterAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Yaml);
            var yaml = converter.Convert(csvData);

            await _yamlWriter.WriteAsync(yamlOutputPath, yaml);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {yamlOutputPath}", ex);
        }
    }
    
    public async Task ConvertXmlToCsvAsync(
        string xmlFilePath, 
        string csvOutputPath, 
        char delimiter = ',',
        bool includeAttributes = true,
        bool preserveCData = true,
        bool includeComments = false)
    {
        try
        {
            var readerOptions = new Dictionary<string, object>
            {
                ["includeAttributes"] = includeAttributes,
                ["preserveCData"] = preserveCData,
                ["includeComments"] = includeComments
            };
            
            var xmlData = await _xmlReader.ReadWithAutoDetectDelimiterAsync(xmlFilePath);
            
            var converterOptions = new Dictionary<string, object>
            {
                ["delimiter"] = delimiter,
                ["quoteValues"] = true
            };
        
            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Csv);
            var csv = converter.Convert(xmlData, converterOptions);
            
            await _csvWriter.WriteAsync(csvOutputPath, csv);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {csvOutputPath}", ex);
        }
    }
    
    // Добавьте новый метод:

    public async Task ConvertXmlToJsonAsync(
        string xmlFilePath, 
        string jsonOutputPath, 
        bool preserveStructure = false)
    {
        try
        {
            var xmlData = await _xmlReader.ReadWithAutoDetectDelimiterAsync(xmlFilePath);

            var converterOptions = new Dictionary<string, object>
            {
                ["useIndentation"] = true,
                ["preserveStructure"] = preserveStructure
            };
            
            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Json);
            var json = converter.Convert(xmlData, converterOptions);
            
            await _jsonWriter.WriteAsync(jsonOutputPath, json);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {jsonOutputPath}", ex);
        }
    }
}