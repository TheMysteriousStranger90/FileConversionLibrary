using System.Text;
using System.Xml.Linq;
using FileConversionLibrary;
using FileConversionLibrary.Models;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibraryConsoleExample;

/// <summary>
/// Demonstrates the capabilities of FileConversionLibrary with comprehensive examples
/// of file, stream, and in-memory conversion APIs.
/// </summary>
sealed class Program
{
    private static readonly FileConverter _converter = new();
    private static readonly string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     FileConversionLibrary - Demonstration Console        ║");
        Console.WriteLine("║              .NET 9 | v1.8.0                              ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

        try
        {
            await RunInteractiveDemo();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ All demonstrations completed successfully!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.ResetColor();

            if (args.Contains("--verbose"))
            {
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task RunInteractiveDemo()
    {
        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await DemoFileConversions();
                    break;
                case "2":
                    await DemoStreamConversions();
                    break;
                case "3":
                    DemoInMemoryConversions();
                    break;
                case "4":
                    await DemoAdvancedOptions();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│           Select Demo Type              │");
        Console.WriteLine("├─────────────────────────────────────────┤");
        Console.WriteLine("│  1. File-to-File Conversions           │");
        Console.WriteLine("│  2. Stream-based Conversions            │");
        Console.WriteLine("│  3. In-Memory Conversions               │");
        Console.WriteLine("│  4. Advanced Options Examples           │");
        Console.WriteLine("│  5. Exit                                │");
        Console.WriteLine("└─────────────────────────────────────────┘");
        Console.Write("\nYour choice: ");
    }

    static async Task DemoFileConversions()
    {
        Console.WriteLine("\n📁 File-to-File Conversions\n" + new string('─', 50));

        var csvPath = Path.Combine(_desktopPath, "sample.csv");
        var xmlPath = Path.Combine(_desktopPath, "sample.xml");

        // Create sample files if they don't exist
        await EnsureSampleFiles(csvPath, xmlPath);

        // CSV Conversions
        await ConvertWithLog("CSV → JSON", () =>
            _converter.ConvertCsvToJsonAsync(csvPath, Path.Combine(_desktopPath, "output-from-csv.json")));

        await ConvertWithLog("CSV → PDF", () =>
            _converter.ConvertCsvToPdfAsync(csvPath, Path.Combine(_desktopPath, "output-from-csv.pdf")));

        await ConvertWithLog("CSV → Word", () =>
            _converter.ConvertCsvToWordAsync(csvPath, Path.Combine(_desktopPath, "output-from-csv.docx")));

        await ConvertWithLog("CSV → XML", () =>
            _converter.ConvertCsvToXmlAsync(csvPath, Path.Combine(_desktopPath, "output-from-csv.xml")));

        await ConvertWithLog("CSV → YAML", () =>
            _converter.ConvertCsvToYamlAsync(csvPath, Path.Combine(_desktopPath, "output-from-csv.yaml")));

        // XML Conversions
        await ConvertWithLog("XML → CSV", () =>
            _converter.ConvertXmlToCsvAsync(xmlPath, Path.Combine(_desktopPath, "output-from-xml.csv")));

        await ConvertWithLog("XML → JSON", () =>
            _converter.ConvertXmlToJsonAsync(xmlPath, Path.Combine(_desktopPath, "output-from-xml.json")));

        await ConvertWithLog("XML → PDF", () =>
            _converter.ConvertXmlToPdfAsync(xmlPath, Path.Combine(_desktopPath, "output-from-xml.pdf")));

        await ConvertWithLog("XML → Word", () =>
            _converter.ConvertXmlToWordAsync(xmlPath, Path.Combine(_desktopPath, "output-from-xml.docx")));

        await ConvertWithLog("XML → YAML", () =>
            _converter.ConvertXmlToYamlAsync(xmlPath, Path.Combine(_desktopPath, "output-from-xml.yaml")));

        Console.WriteLine($"\n✓ All files saved to: {_desktopPath}");
    }

    static async Task DemoStreamConversions()
    {
        Console.WriteLine("\n🌊 Stream-based Conversions\n" + new string('─', 50));

        var csvPath = Path.Combine(_desktopPath, "sample.csv");
        await EnsureSampleFiles(csvPath, "");

        // Stream to String (for text formats: JSON, YAML, XML)
        using (var inputStream = File.OpenRead(csvPath))
        {
            var jsonOptions = new JsonConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "json",
                UseIndentation = true
            };

            var jsonResult = await _converter.ConvertStreamToStringAsync(inputStream, jsonOptions);
            await File.WriteAllTextAsync(Path.Combine(_desktopPath, "stream-output.json"), jsonResult);
            Console.WriteLine("✓ CSV → JSON (via stream)");
        }

        // Stream to Bytes (for binary formats: PDF, Word)
        using (var inputStream = File.OpenRead(csvPath))
        {
            var pdfOptions = new PdfConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "pdf",
                Title = "Stream Demo Report"
            };

            var pdfBytes = await _converter.ConvertStreamToBytesAsync(inputStream, pdfOptions);
            await File.WriteAllBytesAsync(Path.Combine(_desktopPath, "stream-output.pdf"), pdfBytes);
            Console.WriteLine("✓ CSV → PDF (via stream)");
        }

        // Stream to Stream
        using (var inputStream = File.OpenRead(csvPath))
        {
            var xmlOptions = new XmlConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "xml",
                UseCData = true
            };

            using var outputStream = await _converter.ConvertStreamAsync(inputStream, xmlOptions);
            using var fileStream = File.Create(Path.Combine(_desktopPath, "stream-output.xml"));
            await outputStream.CopyToAsync(fileStream);
            Console.WriteLine("✓ CSV → XML (stream-to-stream)");
        }
    }

    static void DemoInMemoryConversions()
    {
        Console.WriteLine("\n💾 In-Memory Conversions\n" + new string('─', 50));

        // Create sample data in memory
        var csvData = new CsvData
        {
            Headers = ["ID", "Product", "Price", "Stock"],
            Rows =
            [
                ["1", "Laptop", "1299.99", "15"],
                ["2", "Mouse", "29.99", "150"],
                ["3", "Keyboard", "79.99", "85"]
            ]
        };

        // Convert CsvData to JSON
        var jsonResult = _converter.ConvertCsvToJson(csvData);
        File.WriteAllText(Path.Combine(_desktopPath, "memory-output.json"), jsonResult);
        Console.WriteLine("✓ CsvData → JSON");

        // Convert CsvData to XML
        var xmlResult = _converter.ConvertCsvToXml(csvData);
        File.WriteAllText(Path.Combine(_desktopPath, "memory-output.xml"), xmlResult);
        Console.WriteLine("✓ CsvData → XML");

        // Create XmlData in memory
        var xmlDoc = XDocument.Parse(@"
            <Products>
                <Product id=""1"">
                    <Name>Monitor</Name>
                    <Price>299.99</Price>
                </Product>
                <Product id=""2"">
                    <Name>Webcam</Name>
                    <Price>89.99</Price>
                </Product>
            </Products>");

        var xmlData = new XmlData { Document = xmlDoc };

        // Convert XmlData to JSON
        var xmlToJsonResult = _converter.ConvertXmlToJson(xmlData);
        File.WriteAllText(Path.Combine(_desktopPath, "memory-xml-output.json"), xmlToJsonResult);
        Console.WriteLine("✓ XmlData → JSON");

        // Convert XmlData to CSV
        var xmlToCsvResult = _converter.ConvertXmlToCsv(xmlData);
        File.WriteAllText(Path.Combine(_desktopPath, "memory-xml-output.csv"), xmlToCsvResult);
        Console.WriteLine("✓ XmlData → CSV");
    }

    static async Task DemoAdvancedOptions()
    {
        Console.WriteLine("\n⚙️  Advanced Options Examples\n" + new string('─', 50));

        var csvPath = Path.Combine(_desktopPath, "sample.csv");
        await EnsureSampleFiles(csvPath, "");

        // JSON with custom options
        var jsonOptions = new JsonConversionOptions
        {
            SourceFormat = "csv",
            TargetFormat = "json",
            UseIndentation = true,
            ConvertValues = true
        };
        using (var input = File.OpenRead(csvPath))
        {
            var result = await _converter.ConvertStreamToStringAsync(input, jsonOptions);
            await File.WriteAllTextAsync(Path.Combine(_desktopPath, "advanced-json.json"), result);
        }

        Console.WriteLine("✓ JSON with type conversion and indentation");

        // PDF with custom options
        var pdfOptions = new PdfConversionOptions
        {
            SourceFormat = "csv",
            TargetFormat = "pdf",
            Title = "Advanced PDF Report",
            FontSize = 12,
            LandscapeOrientation = true
        };
        using (var input = File.OpenRead(csvPath))
        {
            input.Position = 0;
            var result = await _converter.ConvertStreamToBytesAsync(input, pdfOptions);
            await File.WriteAllBytesAsync(Path.Combine(_desktopPath, "advanced.pdf"), result);
        }

        Console.WriteLine("✓ PDF with landscape orientation and custom font");

        // Word with custom options
        var wordOptions = new WordConversionOptions
        {
            SourceFormat = "csv",
            TargetFormat = "word",
            UseTable = true,
            AddHeaderRow = true
        };
        using (var input = File.OpenRead(csvPath))
        {
            input.Position = 0;
            var result = await _converter.ConvertStreamToBytesAsync(input, wordOptions);
            await File.WriteAllBytesAsync(Path.Combine(_desktopPath, "advanced.docx"), result);
        }

        Console.WriteLine("✓ Word document with table format");
    }

    static async Task EnsureSampleFiles(string csvPath, string xmlPath)
    {
        // Create sample CSV
        if (!File.Exists(csvPath))
        {
            var csvContent = @"ID,Name,Age,City,Department,Salary
1,John Doe,28,New York,Engineering,75000
2,Jane Smith,32,London,Marketing,82000
3,Bob Johnson,45,Tokyo,Sales,95000
4,Alice Brown,29,Berlin,HR,68000";
            await File.WriteAllTextAsync(csvPath, csvContent);
        }

        // Create sample XML
        if (!string.IsNullOrEmpty(xmlPath) && !File.Exists(xmlPath))
        {
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Employees>
    <Employee id=""1"">
        <Name>John Doe</Name>
        <Age>28</Age>
        <City>New York</City>
        <Department>Engineering</Department>
        <Salary>75000</Salary>
    </Employee>
    <Employee id=""2"">
        <Name>Jane Smith</Name>
        <Age>32</Age>
        <City>London</City>
        <Department>Marketing</Department>
        <Salary>82000</Salary>
    </Employee>
</Employees>";
            await File.WriteAllTextAsync(xmlPath, xmlContent);
        }
    }

    static async Task ConvertWithLog(string description, Func<Task> conversionAction)
    {
        try
        {
            await conversionAction();
            Console.WriteLine($"✓ {description}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ {description} - Error: {ex.Message}");
        }
    }
}
