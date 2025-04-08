using FileConversionLibrary.Exceptions;
using FileConversionLibrary.Factories;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using FileConversionLibrary.Readers;
using FileConversionLibrary.Writers;

namespace FileConversionLibrary;

public class FileConverterServiceLocator
{
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IFileReader<CsvData> _csvReader;
    private readonly IFileWriter<string> _jsonWriter;
    private readonly IFileWriter<string> _xmlWriter;
    private readonly IFileWriter<string> _yamlWriter;
    private readonly IFileWriter<byte[]> _pdfWriter;
    private readonly IFileWriter<byte[]> _wordWriter;
    private readonly ConverterFactory _converterFactory;
    private FileConverterFacade _converterFacade;

    public FileConverterServiceLocator()
    {
        _exceptionHandler = new ConsoleExceptionHandler();
        _csvReader = new CsvFileReader(_exceptionHandler);
        _jsonWriter = new JsonFileWriter(_exceptionHandler);
        _xmlWriter = new XmlFileWriter(_exceptionHandler);
        _yamlWriter = new YamlFileWriter(_exceptionHandler);
        _pdfWriter = new PdfFileWriter(_exceptionHandler);
        _wordWriter = new WordFileWriter(_exceptionHandler);
        _converterFactory = new ConverterFactory();
    }

    public FileConverterServiceLocator(
        IExceptionHandler exceptionHandler,
        IFileReader<CsvData> csvReader,
        IFileWriter<string> jsonWriter,
        IFileWriter<string> xmlWriter,
        IFileWriter<string> yamlWriter,
        IFileWriter<byte[]> pdfWriter,
        IFileWriter<byte[]> wordWriter,
        ConverterFactory converterFactory)
    {
        _exceptionHandler = exceptionHandler;
        _csvReader = csvReader;
        _jsonWriter = jsonWriter;
        _xmlWriter = xmlWriter;
        _yamlWriter = yamlWriter;
        _pdfWriter = pdfWriter;
        _wordWriter = wordWriter;
        _converterFactory = converterFactory;
    }

    public FileConverterFacade GetFileConverterFacade()
    {
        _converterFacade ??= new FileConverterFacade(
            _csvReader,
            _jsonWriter,
            _xmlWriter,
            _yamlWriter,
            _pdfWriter,
            _wordWriter,
            _converterFactory,
            _exceptionHandler
        );

        return _converterFacade;
    }
}