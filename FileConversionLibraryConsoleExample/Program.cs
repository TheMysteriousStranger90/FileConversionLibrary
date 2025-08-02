using FileConversionLibrary;

namespace FileConversionLibraryConsoleExample;

class Program
{
    static async Task Main(string[] args)
    {
        var fileConverter = new FileConverter();

        // CSV to PDF Conversion
        await fileConverter.ConvertCsvToPdfAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.pdf"
        );
        Console.WriteLine("CSV to PDF conversion completed.");

        // CSV to JSON Conversion
        await fileConverter.ConvertCsvToJsonAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.json"
        );
        Console.WriteLine("CSV to JSON conversion completed.");

        // CSV to Word Conversion
        await fileConverter.ConvertCsvToWordAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.docx"
        );
        Console.WriteLine("CSV to Word conversion completed.");

        // CSV to XML Conversion
        await fileConverter.ConvertCsvToXmlAsync(
            csvFilePath: @"C:\Users\User\Desktop\csv_input.csv",
            xmlOutputPath: @"C:\Users\User\Desktop\output1.xml");
        Console.WriteLine("CSV to XML conversion completed.");

        // CSV to YAML Conversion
        await fileConverter.ConvertCsvToYamlAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.yaml"
        );
        Console.WriteLine("CSV to YAML conversion completed.");

/*
        // XML to CSV Conversion
        await fileConverter.ConvertXmlToCsvAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.csv"
        );
        Console.WriteLine("XML to CSV conversion completed.");

        // XML to JSON Conversion
        await fileConverter.ConvertXmlToJsonAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.json"
        );
        Console.WriteLine("XML to JSON conversion completed.");

        // XML to PDF Conversion
        await fileConverter.ConvertXmlToPdfAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.pdf"
        );
        Console.WriteLine("XML to PDF conversion completed.");

        // XML to Word Conversion
        await fileConverter.ConvertXmlToWordAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.docx"
        );
        Console.WriteLine("XML to Word conversion completed.");

        // XML to YAML Conversion
        await fileConverter.ConvertXmlToYamlAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.yaml"
        );
        Console.WriteLine("XML to YAML conversion completed.");
*/
    }
}