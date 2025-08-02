using FileConversionLibrary.Converters;
using FileConversionLibrary.Enums;
using FileConversionLibrary.Exceptions;
using FileConversionLibrary.Factories;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using FileConversionLibrary.Readers;
using FileConversionLibrary.Writers;
using iTextSharp.text;

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

    public async Task ConvertCsvToJsonAsync(string csvFilePath, string jsonOutputPath)
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
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(pdfOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
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
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(wordOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
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
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(xmlOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
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
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"Input CSV file not found: {csvFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(yamlOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
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
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"Input XML file not found: {xmlFilePath}");
            }

            var outputDirectory = Path.GetDirectoryName(csvOutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

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

    public async Task ConvertXmlToJsonAsync(
        string xmlFilePath,
        string jsonOutputPath,
        bool convertValues = true,
        bool removeWhitespace = true)
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
            
            var xmlData = await _xmlReader.ReadWithAutoDetectDelimiterAsync(xmlFilePath);

            if (xmlData.Document == null)
            {
                throw new InvalidOperationException("XML document could not be loaded properly");
            }

            var converterOptions = new Dictionary<string, object>
            {
                ["useIndentation"] = true,
                ["convertValues"] = convertValues,
                ["removeWhitespace"] = removeWhitespace
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

    public async Task ConvertXmlToPdfAsync(
        string xmlFilePath,
        string pdfOutputPath,
        bool hierarchicalView = false,
        float fontSize = 10f,
        bool addBorders = true,
        bool alternateRowColors = false)
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
            
            var xmlData = await _xmlReader.ReadWithAutoDetectDelimiterAsync(xmlFilePath);

            var converterOptions = new Dictionary<string, object>
            {
                ["fontSize"] = fontSize,
                ["addBorders"] = addBorders,
                ["alternateRowColors"] = alternateRowColors,
                ["headerBackgroundColor"] = new BaseColor(220, 220, 220),
                ["hierarchicalView"] = hierarchicalView
            };

            var converter = _converterFactory.GetConverter<XmlData, byte[]>(OutputFormat.Pdf);
            var pdfData = converter.Convert(xmlData, converterOptions);

            await _pdfWriter.WriteAsync(pdfOutputPath, pdfData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {pdfOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToWordAsync(
        string xmlFilePath,
        string wordOutputPath,
        bool useTable = true,
        bool addHeaderRow = true,
        bool formatAsHierarchy = false,
        string fontFamily = "Calibri",
        int fontSize = 11)
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
            
            var xmlData = await _xmlReader.ReadWithAutoDetectDelimiterAsync(xmlFilePath);

            if (xmlData.Headers == null || xmlData.Rows == null)
            {
                throw new InvalidOperationException("XML data could not be loaded properly");
            }

            var converterOptions = new Dictionary<string, object>
            {
                ["useTable"] = useTable,
                ["addHeaderRow"] = addHeaderRow,
                ["fontFamily"] = fontFamily,
                ["fontSize"] = fontSize,
                ["formatAsHierarchy"] = formatAsHierarchy
            };

            var converter = _converterFactory.GetConverter<XmlData, byte[]>(OutputFormat.Word);
            var wordData = converter.Convert(xmlData, converterOptions);

            await _wordWriter.WriteAsync(wordOutputPath, wordData);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {wordOutputPath}", ex);
        }
    }

    public async Task ConvertXmlToYamlAsync(
        string xmlFilePath,
        string yamlOutputPath,
        bool useCamelCase = false,
        bool convertValues = true,
        bool keepStringsForNumbers = false)
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
            
            var xmlData = await _xmlReader.ReadWithAutoDetectDelimiterAsync(xmlFilePath);

            if (xmlData.Document == null)
            {
                throw new InvalidOperationException("XML document could not be loaded properly");
            }

            var converterOptions = new Dictionary<string, object>
            {
                ["useCamelCase"] = useCamelCase,
                ["convertValues"] = convertValues,
                ["keepStringsForNumbers"] = keepStringsForNumbers
            };

            var converter = _converterFactory.GetConverter<XmlData, string>(OutputFormat.Yaml);
            var yaml = converter.Convert(xmlData, converterOptions);

            await _yamlWriter.WriteAsync(yamlOutputPath, yaml);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw new FileConversionException($"Failed to convert {xmlFilePath} to {yamlOutputPath}", ex);
        }
    }
}