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
    private readonly IFileReader<XmlData> _xmlReader;
    private readonly IFileWriter<string> _csvWriter;
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
        _xmlReader = new XmlFileReader(_exceptionHandler);
        _csvWriter = new CsvFileWriter(_exceptionHandler);
        _converterFactory = new ConverterFactory();
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
            _xmlReader,
            _csvWriter,
            _converterFactory,
            _exceptionHandler
        );

        return _converterFacade;
    }
}