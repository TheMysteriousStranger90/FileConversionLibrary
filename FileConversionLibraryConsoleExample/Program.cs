using FileConversionLibrary;

class Program
{
    static async Task Main(string[] args)
    {
        var serviceLocator = new FileConverterServiceLocator();
        var converterFacade = serviceLocator.GetFileConverterFacade();
        /*
        // CSV to PDF Conversion
        await converterFacade.ConvertCsvToPdfAsync(
            @"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.pdf"
        );
        Console.WriteLine("CSV to PDF conversion completed.");
        
        // CSV to JSON Conversion
        await converterFacade.ConvertCsvToJsonAsync(
            @"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.json"
        );
        Console.WriteLine("CSV to JSON conversion completed.");
        
        // CSV to Word Conversion
        await converterFacade.ConvertCsvToWordAsync(
            @"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.docx"
        );
        Console.WriteLine("CSV to Word conversion completed.");
        
        // CSV to XML Conversion
        await converterFacade.ConvertCsvToXmlAsync(
            csvFilePath: @"C:\Users\User\Desktop\sample.csv",
            xmlOutputPath: @"C:\Users\User\Desktop\output.xml");
        Console.WriteLine("CSV to XML conversion completed.");
        
        // CSV to YAML Conversion
        await converterFacade.ConvertCsvToYamlAsync(
            @"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.yaml"
            );
        Console.WriteLine("CSV to YAML conversion completed.");
        */
        
        // XML to CSV Conversion
        await converterFacade.ConvertXmlToCsvAsync(
            @"C:\Users\User\Desktop\input.xml",
            @"C:\Users\User\Desktop\output.csv"
        );
        Console.WriteLine("XML to CSV conversion completed.");
        
        // XML to JSON Conversion
        await converterFacade.ConvertXmlToJsonAsync(
            @"C:\Users\User\Desktop\input.xml",
            @"C:\Users\User\Desktop\output.json"
        );
        Console.WriteLine("XML to JSON conversion completed.");
        
        // XML to PDF Conversion
        await converterFacade.ConvertXmlToPdfAsync(
            @"C:\Users\User\Desktop\input.xml",
            @"C:\Users\User\Desktop\catalog_table.pdf"
        );
        Console.WriteLine("XML to PDF conversion completed.");
        
/*
        // XML to YAML Conversion
        var xmlToYamlConverter = new XmlToYamlConverter();
        await xmlToYamlConverter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.yaml");
        Console.WriteLine("XML to YAML conversion completed.");

        // XML to PDF Conversion
        var xmlToPdfConverter = new XmlToPdfConverter();
        await xmlToPdfConverter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.pdf");
        Console.WriteLine("XML to PDF conversion completed.");

        // XML to Word Conversion
        var xmlToWordConverter = new XmlToWordConverter();
        await xmlToWordConverter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.docx");
        Console.WriteLine("XML to Word conversion completed.");
*/
    }
}