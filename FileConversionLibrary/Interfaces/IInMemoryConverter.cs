using FileConversionLibrary.Models;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibrary.Interfaces;

public interface IInMemoryConverter
{
    string ConvertCsvToJson(CsvData data, JsonConversionOptions? options = null);
    byte[] ConvertCsvToPdf(CsvData data, PdfConversionOptions? options = null);
    byte[] ConvertCsvToWord(CsvData data, WordConversionOptions? options = null);
    string ConvertCsvToXml(CsvData data, XmlConversionOptions? options = null);
    string ConvertCsvToYaml(CsvData data, YamlConversionOptions? options = null);
    
    string ConvertXmlToCsv(XmlData data, CsvConversionOptions? options = null);
    string ConvertXmlToJson(XmlData data, JsonConversionOptions? options = null);
    byte[] ConvertXmlToPdf(XmlData data, PdfConversionOptions? options = null);
    byte[] ConvertXmlToWord(XmlData data, WordConversionOptions? options = null);
    string ConvertXmlToYaml(XmlData data, YamlConversionOptions? options = null);
}