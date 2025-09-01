using FileConversionLibrary.Converters;
using FileConversionLibrary.Enums;
using FileConversionLibrary.Exceptions;
using FileConversionLibrary.Factories;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using FileConversionLibrary.Models.Options;
using FileConversionLibrary.Readers;
using FileConversionLibrary.Writers;

namespace FileConversionLibrary;

public class FileConverter
{
    private readonly IExceptionHandler? _exceptionHandler;
    private readonly IFileReader<XmlData> _xmlReader;
    private readonly IFileWriter<string> _csvWriter;
    private readonly IFileReader<CsvData> _csvReader;
    private readonly IFileWriter<string> _jsonWriter;
    private readonly IFileWriter<string> _xmlWriter;
    private readonly IFileWriter<string> _yamlWriter;
    private readonly IFileWriter<byte[]> _pdfWriter;
    private readonly IFileWriter<byte[]> _wordWriter;
    private readonly ConverterFactory _converterFactory;
    private readonly IStreamConverter _streamConverter;
    private readonly IInMemoryConverter _inMemoryConverter;
    
    private static FileConverter? _instance;

    public FileConverter()
    {
        _exceptionHandler = new ConsoleExceptionHandler();
        _csvReader = new CsvFileReader(_exceptionHandler);
        _jsonWriter = new JsonFileWriter(_exceptionHandler);
        _xmlWriter = new XmlFileWriter(_exceptionHandler);
        _yamlWriter = new YamlFileWriter(_exceptionHandler);
        _pdfWriter = new PdfFileWriter(_exceptionHandler);
        _wordWriter = new WordFileWriter(_exceptionHandler);
        _xmlReader = new XmlFileReader(_exceptionHandler);
        _csvWriter = new CsvFileWriter(_exceptionHandler);
        _converterFactory = new ConverterFactory();
        
        _inMemoryConverter = new InMemoryConverter(_converterFactory, _exceptionHandler);
        _streamConverter = new StreamConverter(_inMemoryConverter, _exceptionHandler);
    }

    public FileConverter(
        IFileReader<CsvData> csvReader,
        IFileWriter<string> jsonWriter,
        IFileWriter<string> xmlWriter,
        IFileWriter<string> yamlWriter,
        IFileWriter<byte[]> pdfWriter,
        IFileWriter<byte[]> wordWriter,
        IFileReader<XmlData> xmlReader,
        IFileWriter<string> csvWriter,
        ConverterFactory converterFactory,
        IExceptionHandler? exceptionHandler)
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

    public static FileConverter GetInstance()
    {
        return _instance ??= new FileConverter();
    }

    public async Task ConvertCsvToJsonAsync(string csvFilePath, string jsonOutputPath, JsonConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(jsonOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var csvData = await _csvReader.ReadAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Json);
            var json = converter.Convert(csvData, options);

            await _jsonWriter.WriteAsync(jsonOutputPath, json, options);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {jsonOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToPdfAsync(string csvFilePath, string pdfOutputPath, PdfConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(pdfOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var csvData = await _csvReader.ReadAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, byte[]>(OutputFormat.Pdf);
            var pdfData = converter.Convert(csvData, options);

            await _pdfWriter.WriteAsync(pdfOutputPath, pdfData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {pdfOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToWordAsync(string csvFilePath, string wordOutputPath, WordConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(wordOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var csvData = await _csvReader.ReadAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, byte[]>(OutputFormat.Word);
            var wordData = converter.Convert(csvData, options);

            await _wordWriter.WriteAsync(wordOutputPath, wordData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {wordOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToXmlAsync(string csvFilePath, string xmlOutputPath, XmlConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(xmlOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var csvData = await _csvReader.ReadAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Xml);
            var xml = converter.Convert(csvData, options);

            await _xmlWriter.WriteAsync(xmlOutputPath, xml, options);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {xmlOutputPath}", ex);
        }
    }

    public async Task ConvertCsvToYamlAsync(string csvFilePath, string yamlOutputPath, YamlConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(yamlOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var csvData = await _csvReader.ReadAsync(csvFilePath);

            var converter = _converterFactory.GetConverter<CsvData, string>(OutputFormat.Yaml);
            var yaml = converter.Convert(csvData, options);

            await _yamlWriter.WriteAsync(yamlOutputPath, yaml);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {csvFilePath} to {yamlOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToCsvAsync(string xmlFilePath, string csvOutputPath, CsvConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Input XML file not found: {xmlFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(csvOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var xmlData = await _xmlReader.ReadAsync(xmlFilePath, options);

            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Csv);
            var csv = converter.Convert(xmlData, options);

            await _csvWriter.WriteAsync(csvOutputPath, csv);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {csvOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToJsonAsync(string xmlFilePath, string jsonOutputPath, JsonConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Input XML file not found: {xmlFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(jsonOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var xmlData = await _xmlReader.ReadAsync(xmlFilePath, options);

            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Json);
            var json = converter.Convert(xmlData, options);

            await _jsonWriter.WriteAsync(jsonOutputPath, json, options);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {jsonOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToPdfAsync(string xmlFilePath, string pdfOutputPath, PdfConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Input XML file not found: {xmlFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(pdfOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var xmlData = await _xmlReader.ReadAsync(xmlFilePath, options);

            var converter = _converterFactory.GetConverter<XmlData, byte[]>(OutputFormat.Pdf);
            var pdfData = converter.Convert(xmlData, options);

            await _pdfWriter.WriteAsync(pdfOutputPath, pdfData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {pdfOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToWordAsync(string xmlFilePath, string wordOutputPath, WordConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Input XML file not found: {xmlFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(wordOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var xmlData = await _xmlReader.ReadAsync(xmlFilePath, options);

            var converter = _converterFactory.GetConverter<XmlData, byte[]>(OutputFormat.Word);
            var wordData = converter.Convert(xmlData, options);

            await _wordWriter.WriteAsync(wordOutputPath, wordData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {wordOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToYamlAsync(string xmlFilePath, string yamlOutputPath, YamlConversionOptions? options = null)
    {
        try
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Input XML file not found: {xmlFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(yamlOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var xmlData = await _xmlReader.ReadAsync(xmlFilePath, options);

            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Yaml);
            var yaml = converter.Convert(xmlData, options);

            await _yamlWriter.WriteAsync(yamlOutputPath, yaml);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {yamlOutputPath}", ex);
        }
    }
    
    public async Task<Stream> ConvertStreamAsync(Stream input, ConversionOptions options)
    {
        return await _streamConverter.ConvertAsync(input, options);
    }

    public async Task<byte[]> ConvertStreamToBytesAsync(Stream input, ConversionOptions options)
    {
        return await _streamConverter.ConvertToBytesAsync(input, options);
    }

    public async Task<string> ConvertStreamToStringAsync(Stream input, ConversionOptions options)
    {
        return await _streamConverter.ConvertToStringAsync(input, options);
    }

    public string ConvertCsvToJson(CsvData data, JsonConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertCsvToJson(data, options);
    }

    public byte[] ConvertCsvToPdf(CsvData data, PdfConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertCsvToPdf(data, options);
    }

    public byte[] ConvertCsvToWord(CsvData data, WordConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertCsvToWord(data, options);
    }

    public string ConvertCsvToXml(CsvData data, XmlConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertCsvToXml(data, options);
    }

    public string ConvertCsvToYaml(CsvData data, YamlConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertCsvToYaml(data, options);
    }
    
    public string ConvertXmlToCsv(XmlData data, CsvConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertXmlToCsv(data, options);
    }

    public string ConvertXmlToJson(XmlData data, JsonConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertXmlToJson(data, options);
    }

    public byte[] ConvertXmlToPdf(XmlData data, PdfConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertXmlToPdf(data, options);
    }

    public byte[] ConvertXmlToWord(XmlData data, WordConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertXmlToWord(data, options);
    }

    public string ConvertXmlToYaml(XmlData data, YamlConversionOptions? options = null)
    {
        return _inMemoryConverter.ConvertXmlToYaml(data, options);
    }
}