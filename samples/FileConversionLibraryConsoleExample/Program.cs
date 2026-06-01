using System.Xml.Linq;
using FileConversionLibrary;
using FileConversionLibrary.Models;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibraryConsoleExample;

/// <summary>
/// This console application serves as a demonstration and testing ground
/// for the FileConversionLibrary, showcasing its various conversion capabilities.
/// </summary>
sealed class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// Initializes the FileConverter and runs a series of conversion tests.
    /// </summary>
    static async Task Main(string[] args)
    {
        var fileConverter = new FileConverter();

        Console.WriteLine("=== FileConversionLibrary Test Console ===\n");

        try
        {
            // Run tests for the file-based conversion API.
            await TestFileBasedAPI(fileConverter);

            // Uncomment to run tests for the Stream-based conversion API.
            //await TestStreamAPI(fileConverter);

            // Uncomment to run tests for the In-Memory conversion API.
            //TestInMemoryAPI(fileConverter);

            Console.WriteLine("\n🎉 All tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ An error occurred: {ex.Message}");
            Console.WriteLine($"Details: {ex}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Tests the primary file-to-file conversion methods of the library.
    /// This demonstrates reading from a source file and writing to a destination file
    /// using the new unified API with strongly-typed options classes.
    /// </summary>
    static async Task TestFileBasedAPI(FileConverter fileConverter)
    {
        Console.WriteLine("📁 Testing File-based API:");
        Console.WriteLine("===========================");

        // --- CSV to Other Formats ---
        await fileConverter.ConvertCsvToPdfAsync(
            @"C:\Users\User\Desktop\csv_input.csv", @"C:\Users\User\Desktop\output1.pdf");
        Console.WriteLine("✅ CSV to PDF conversion completed.");

        await fileConverter.ConvertCsvToJsonAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.json"
        );
        Console.WriteLine("✅ CSV to JSON conversion completed.");

        await fileConverter.ConvertCsvToWordAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.docx"
        );
        Console.WriteLine("✅ CSV to Word conversion completed.");

        await fileConverter.ConvertCsvToXmlAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.xml"
        );
        Console.WriteLine("✅ CSV to XML conversion completed.");

        await fileConverter.ConvertCsvToYamlAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.yaml"
        );
        Console.WriteLine("✅ CSV to YAML conversion completed.");
        
        // --- XML to Other Formats ---
        await fileConverter.ConvertXmlToCsvAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.csv"
        );
        Console.WriteLine("✅ XML to CSV conversion completed.");

        await fileConverter.ConvertXmlToJsonAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.json"
        );
        Console.WriteLine("✅ XML to JSON conversion completed.");

        await fileConverter.ConvertXmlToPdfAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.pdf"
        );
        Console.WriteLine("✅ XML to PDF conversion completed.");

        await fileConverter.ConvertXmlToWordAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.docx");
        Console.WriteLine("✅ XML to Word conversion completed.");

        await fileConverter.ConvertXmlToYamlAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.yaml");
        Console.WriteLine("✅ XML to YAML conversion completed.");
        
        Console.WriteLine();
    }

    /// <summary>
    /// Tests the stream-based conversion API, which is ideal for web applications
    /// or scenarios where data is processed without being saved to disk.
    /// </summary>
    static async Task TestStreamAPI(FileConverter fileConverter)
    {
        Console.WriteLine("🌊 Testing New Stream API:");
        Console.WriteLine("==========================");

        // Test Stream API with CSV to JSON
        if (File.Exists(@"C:\Users\User\Desktop\csv_input.csv"))
        {
            using var inputStream = File.OpenRead(@"C:\Users\User\Desktop\csv_input.csv");

            // Test ConvertStreamToStringAsync: Reads a stream and returns the converted content as a string.
            var jsonOptions = new JsonConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "json",
                UseIndentation = true
            };

            var jsonResult = await fileConverter.ConvertStreamToStringAsync(inputStream, jsonOptions);
            await File.WriteAllTextAsync(@"C:\Users\User\Desktop\stream_output.json", jsonResult);
            Console.WriteLine("✅ Stream to JSON string conversion completed.");

            // Reset stream position for the next test
            inputStream.Position = 0;

            // Test ConvertStreamToBytesAsync: Reads a stream and returns the converted content as a byte array (e.g., for PDF/Word).
            var pdfOptions = new PdfConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "pdf",
                Title = "Streamed PDF Report"
            };

            var pdfBytes = await fileConverter.ConvertStreamToBytesAsync(inputStream, pdfOptions);
            await File.WriteAllBytesAsync(@"C:\Users\User\Desktop\stream_output.pdf", pdfBytes);
            Console.WriteLine("✅ Stream to PDF bytes conversion completed.");

            // Reset stream position for the next test
            inputStream.Position = 0;

            // Test ConvertStreamAsync (Stream-to-Stream): Reads an input stream and returns a new output stream.
            var xmlOptions = new XmlConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "xml"
            };

            using var outputStream = await fileConverter.ConvertStreamAsync(inputStream, xmlOptions);
            using var fileStream = File.Create(@"C:\Users\User\Desktop\stream_output.xml");
            await outputStream.CopyToAsync(fileStream);
            Console.WriteLine("✅ Stream to Stream conversion completed.");
        }
        else
        {
            Console.WriteLine("⚠️  CSV input file not found, skipping Stream API tests.");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Tests the in-memory conversion API, where data is represented by C# objects
    /// (like CsvData or XmlData) instead of files.
    /// </summary>
    static void TestInMemoryAPI(FileConverter fileConverter)
    {
        Console.WriteLine("💾 Testing New In-Memory API:");
        Console.WriteLine("=============================");

        // Create sample CsvData object in memory.
        var csvData = new CsvData
        {
            Headers = new[] { "ID", "Name", "Age", "City", "Salary", "Department" },
            Rows = new List<string[]>
            {
                new[] { "1", "John Doe", "28", "New York", "75000", "Engineering" },
                new[] { "2", "Jane Smith", "32", "London", "82000", "Marketing" },
                new[] { "3", "Bob Johnson", "45", "Tokyo", "95000", "Sales" },
                new[] { "4", "Alice Brown", "29", "Berlin", "68000", "HR" },
                new[] { "5", "Charlie Wilson", "35", "Sydney", "71000", "Finance" }
            }
        };

        // Test advanced JSON conversion with options
        var jsonOptions = new JsonConversionOptions
        {
            ConvertValues = true,
            UseIndentation = true
        };
        var json = fileConverter.ConvertCsvToJson(csvData, jsonOptions);
        File.WriteAllText(@"C:\Users\User\Desktop\inmemory_output.json", json);
        Console.WriteLine("✅ In-Memory CSV to JSON conversion completed.");

        // Test advanced PDF conversion with options
        var pdfOptions = new PdfConversionOptions
        {
            FontSize = 11f,
            Title = "Employee Report",
            AlternateRowColors = true
        };
        var pdfBytes = fileConverter.ConvertCsvToPdf(csvData, pdfOptions);
        File.WriteAllBytes(@"C:\Users\User\Desktop\inmemory_output.pdf", pdfBytes);
        Console.WriteLine("✅ In-Memory CSV to PDF conversion completed.");

        // Test advanced Word conversion with options
        var wordOptions = new WordConversionOptions
        {
            UseTable = true,
            FontFamily = "Calibri",
            FontSize = 11,
            AlternateRowColors = true
        };
        var wordBytes = fileConverter.ConvertCsvToWord(csvData, wordOptions);
        File.WriteAllBytes(@"C:\Users\User\Desktop\inmemory_output.docx", wordBytes);
        Console.WriteLine("✅ In-Memory CSV to Word conversion completed.");

        // Test advanced XML conversion with options
        var xmlOptions = new XmlConversionOptions
        {
            OutputFormat = "Elements",
            UseCData = false,
            AddComments = true
        };
        var xml = fileConverter.ConvertCsvToXml(csvData, xmlOptions);
        File.WriteAllText(@"C:\Users\User\Desktop\inmemory_output.xml", xml);
        Console.WriteLine("✅ In-Memory CSV to XML conversion completed.");

        // Test advanced YAML conversion with options
        var yamlOptions = new YamlConversionOptions
        {
            Structure = "Array",
            NamingConvention = "CamelCase",
            ConvertDataTypes = true
        };
        var yaml = fileConverter.ConvertCsvToYaml(csvData, yamlOptions);
        File.WriteAllText(@"C:\Users\User\Desktop\inmemory_output.yaml", yaml);
        Console.WriteLine("✅ In-Memory CSV to YAML conversion completed.");

        // Test in-memory conversions starting from XML data.
        TestXmlInMemoryConversions(fileConverter);

        Console.WriteLine();
    }

    /// <summary>
    /// Tests in-memory conversions that start with XML data.
    /// It demonstrates the two ways to provide XML data: as a pre-parsed table (Headers/Rows)
    /// or as a full XML document (XDocument).
    /// </summary>
    static void TestXmlInMemoryConversions(FileConverter fileConverter)
    {
        Console.WriteLine("\n🔄 Testing XML In-Memory Conversions:");

        try
        {
            // For in-memory conversions to table-based formats (PDF, Word, CSV),
            // we manually provide the headers and rows, as if they were already parsed from an XML file.
            // This is the expected input for these converters.
            var xmlDataForTables = new XmlData
            {
                Headers = new[] { "ProductID", "ProductName", "Price", "Category", "InStock" },
                Rows = new List<string[]>
                {
                    new[] { "1", "Laptop Pro", "1299.99", "Electronics", "true" },
                    new[] { "2", "Office Chair", "249.50", "Furniture", "false" },
                    new[] { "3", "Programming Book", "45.99", "Books", "true" },
                    new[] { "4", "Wireless Mouse", "29.99", "Electronics", "true" }
                },
                RootElementName = "Products"
            };

            // For conversions that work with the full XML structure (JSON, YAML),
            // we provide the XDocument object directly.
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?><Products><Product><ProductID>1</ProductID><ProductName>Laptop Pro</ProductName><Price>1299.99</Price><Category>Electronics</Category><InStock>true</InStock></Product><Product><ProductID>2</ProductID><ProductName>Office Chair</ProductName><Price>249.50</Price><Category>Furniture</Category><InStock>false</InStock></Product></Products>";
            var xmlDataForTree = new XmlData
            {
                Document = XDocument.Parse(xmlContent)
            };

            // XML to CSV (uses the table-based data)
            var csv = fileConverter.ConvertXmlToCsv(xmlDataForTables, new CsvConversionOptions { Delimiter = ',' });
            File.WriteAllText(@"C:\Users\User\Desktop\xml_inmemory_output.csv", csv);
            Console.WriteLine("✅ In-Memory XML to CSV conversion completed.");

            // XML to JSON (uses the tree-based data)
            var json = fileConverter.ConvertXmlToJson(xmlDataForTree, new JsonConversionOptions { ConvertValues = true, UseIndentation = true });
            File.WriteAllText(@"C:\Users\User\Desktop\xml_inmemory_output.json", json);
            Console.WriteLine("✅ In-Memory XML to JSON conversion completed.");

            // XML to PDF (uses the table-based data)
            var pdfBytes = fileConverter.ConvertXmlToPdf(xmlDataForTables, new PdfConversionOptions { Title = "Product Catalog", AlternateRowColors = true });
            File.WriteAllBytes(@"C:\Users\User\Desktop\xml_inmemory_output.pdf", pdfBytes);
            Console.WriteLine("✅ In-Memory XML to PDF conversion completed.");

            // XML to Word (uses the table-based data)
            var wordBytes = fileConverter.ConvertXmlToWord(xmlDataForTables, new WordConversionOptions { UseTable = true, FontFamily = "Arial" });
            File.WriteAllBytes(@"C:\Users\User\Desktop\xml_inmemory_output.docx", wordBytes);
            Console.WriteLine("✅ In-Memory XML to Word conversion completed.");

            // XML to YAML (uses the tree-based data)
            var yaml = fileConverter.ConvertXmlToYaml(xmlDataForTree, new YamlConversionOptions { ConvertDataTypes = true });
            File.WriteAllText(@"C:\Users\User\Desktop\xml_inmemory_output.yaml", yaml);
            Console.WriteLine("✅ In-Memory XML to YAML conversion completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in XML In-Memory conversions: {ex.Message}");
        }
    }
}