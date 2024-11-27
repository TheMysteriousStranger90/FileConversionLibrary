using FileConversionLibrary;

class Program
{
    static async Task Main(string[] args)
    {
        // CSV to XML Conversion
        var csvToXmlConverter = new CsvToXmlConverter();
        await csvToXmlConverter.ConvertAsync(@"C:\Users\User\Desktop\sample.csv", @"C:\Users\User\Desktop\output.xml");
        Console.WriteLine("CSV to XML conversion completed.");

        // CSV to PDF Conversion
        var csvToPdfConverter = new CsvToPdfConverter();
        await csvToPdfConverter.ConvertAsync(@"C:\Users\User\Desktop\sample.csv", @"C:\Users\User\Desktop\output.pdf");
        Console.WriteLine("CSV to PDF conversion completed.");

        // CSV to Word Conversion
        var csvToWordConverter = new CsvToWordConverter();
        await csvToWordConverter.ConvertAsync(@"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.docx");
        Console.WriteLine("CSV to Word conversion completed.");

        // CSV to YAML Conversion
        var csvToYamlConverter = new CsvToYamlConverter();
        await csvToYamlConverter.ConvertAsync(@"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.yaml");
        Console.WriteLine("CSV to YAML conversion completed.");

        // CSV to JSON Conversion
        var csvToJsonConverter = new CsvToJsonConverter();
        await csvToJsonConverter.ConvertAsync(@"C:\Users\User\Desktop\sample.csv",
            @"C:\Users\User\Desktop\output.json");
        Console.WriteLine("CSV to JSON conversion completed.");


        // XML to CSV Conversion
        var xmlToCsvConverter = new XmlToCsvConverter();
        await xmlToCsvConverter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.csv");
        Console.WriteLine("XML to CSV conversion completed.");

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

        // XML to JSON Conversion
        var xmlToJsonConverter = new XmlToJsonConverter();
        await xmlToJsonConverter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.json");
        Console.WriteLine("XML to JSON conversion completed.");
    }
}