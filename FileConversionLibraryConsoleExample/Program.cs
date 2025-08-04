using System.Xml.Linq;
using FileConversionLibrary;
using FileConversionLibrary.Models;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibraryConsoleExample;

class Program
{
    static async Task Main(string[] args)
    {
        var fileConverter = new FileConverter();

        Console.WriteLine("=== FileConversionLibrary Test Console ===\n");

        try
        {
            // Test original file-based API
            await TestOriginalFileAPI(fileConverter);

            // Test new Stream API
            //await TestStreamAPI(fileConverter);

            // Test new In-Memory API
            //TestInMemoryAPI(fileConverter);

            Console.WriteLine("\n🎉 All tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error occurred: {ex.Message}");
            Console.WriteLine($"Details: {ex}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task TestOriginalFileAPI(FileConverter fileConverter)
    {
        Console.WriteLine("📁 Testing Original File-based API:");
        Console.WriteLine("=====================================");

        // CSV conversions
        await fileConverter.ConvertCsvToPdfAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.pdf"
        );
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

        await fileConverter.ConvertCsvToXmlAsync(@"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.xml");
        Console.WriteLine("✅ CSV to XML conversion completed.");

        await fileConverter.ConvertCsvToYamlAsync(
            @"C:\Users\User\Desktop\csv_input.csv",
            @"C:\Users\User\Desktop\output1.yaml"
        );
        Console.WriteLine("✅ CSV to YAML conversion completed.");

        
        // XML conversions
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
            @"C:\Users\User\Desktop\output2.docx"
        );
        Console.WriteLine("✅ XML to Word conversion completed.");

        await fileConverter.ConvertXmlToYamlAsync(
            @"C:\Users\User\Desktop\xml_input.xml",
            @"C:\Users\User\Desktop\output2.yaml"
        );
        Console.WriteLine("✅ XML to YAML conversion completed.");
        
        Console.WriteLine();
    }

    static async Task TestStreamAPI(FileConverter fileConverter)
    {
        Console.WriteLine("🌊 Testing New Stream API:");
        Console.WriteLine("==========================");

        // Test Stream API with CSV to JSON
        if (File.Exists(@"C:\Users\User\Desktop\csv_input.csv"))
        {
            using var inputStream = File.OpenRead(@"C:\Users\User\Desktop\csv_input.csv");

            // Test ConvertStreamToStringAsync
            var jsonOptions = new ConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "json"
            };

            var jsonResult = await fileConverter.ConvertStreamToStringAsync(inputStream, jsonOptions);
            await File.WriteAllTextAsync(@"C:\Users\User\Desktop\stream_output.json", jsonResult);
            Console.WriteLine("✅ Stream to JSON string conversion completed.");

            // Reset stream position for next test
            inputStream.Position = 0;

            // Test ConvertStreamToBytesAsync for PDF
            var pdfOptions = new ConversionOptions
            {
                SourceFormat = "csv",
                TargetFormat = "pdf"
            };

            var pdfBytes = await fileConverter.ConvertStreamToBytesAsync(inputStream, pdfOptions);
            await File.WriteAllBytesAsync(@"C:\Users\User\Desktop\stream_output.pdf", pdfBytes);
            Console.WriteLine("✅ Stream to PDF bytes conversion completed.");

            // Reset stream position for next test
            inputStream.Position = 0;

            // Test ConvertStreamAsync (Stream to Stream)
            var xmlOptions = new ConversionOptions
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

    static void TestInMemoryAPI(FileConverter fileConverter)
    {
        Console.WriteLine("💾 Testing New In-Memory API:");
        Console.WriteLine("=============================");

        // Create sample CSV data
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

        // Test advanced JSON conversion
        var jsonOptions = new JsonConversionOptions
        {
            ConvertValues = true,
            UseIndentation = true,
            IncludeRowNumbers = false,
            CreateNestedObjects = false,
            ConvertArrays = false
        };

        var json = fileConverter.ConvertCsvToJson(csvData, jsonOptions);
        File.WriteAllText(@"C:\Users\User\Desktop\inmemory_output.json", json);
        Console.WriteLine("✅ In-Memory CSV to JSON conversion completed.");
        Console.WriteLine($"   📄 Sample JSON preview: {json.Substring(0, Math.Min(100, json.Length))}...");

        // Test advanced PDF conversion
        var pdfOptions = new PdfConversionOptions
        {
            FontSize = 11f,
            Title = "Employee Report",
            IncludeTimestamp = false,
            IncludeRowNumbers = false,
            AlternateRowColors = false,
            LandscapeOrientation = false,
            FontFamily = "Helvetica"
        };

        var pdfBytes = fileConverter.ConvertCsvToPdf(csvData, pdfOptions);
        File.WriteAllBytes(@"C:\Users\User\Desktop\inmemory_output.pdf", pdfBytes);
        Console.WriteLine("✅ In-Memory CSV to PDF conversion completed.");
        Console.WriteLine($"   📊 PDF size: {pdfBytes.Length:N0} bytes");

        // Test advanced Word conversion
        var wordOptions = new WordConversionOptions
        {
            UseTable = true,
            FontFamily = "Calibri",
            FontSize = 11,
            AlternateRowColors = true,
            FormatAsHierarchy = false
        };

        var wordBytes = fileConverter.ConvertCsvToWord(csvData, wordOptions);
        File.WriteAllBytes(@"C:\Users\User\Desktop\inmemory_output.docx", wordBytes);
        Console.WriteLine("✅ In-Memory CSV to Word conversion completed.");
        Console.WriteLine($"   📝 Word document size: {wordBytes.Length:N0} bytes");

        // Test advanced XML conversion
        var xmlOptions = new XmlConversionOptions
        {
            OutputFormat = "Elements",
            UseCData = false,
            IncludeTimestamp = true,
            NamingConvention = "Original",
            AddComments = true
        };

        var xml = fileConverter.ConvertCsvToXml(csvData, xmlOptions);
        File.WriteAllText(@"C:\Users\User\Desktop\inmemory_output.xml", xml);
        Console.WriteLine("✅ In-Memory CSV to XML conversion completed.");
        Console.WriteLine($"   🏷️  XML preview: {xml.Substring(0, Math.Min(150, xml.Length))}...");

        // Test advanced YAML conversion
        var yamlOptions = new YamlConversionOptions
        {
            Structure = "Array",
            NamingConvention = "Original",
            ConvertDataTypes = true,
            IncludeComments = true,
            SortKeys = false
        };

        var yaml = fileConverter.ConvertCsvToYaml(csvData, yamlOptions);
        File.WriteAllText(@"C:\Users\User\Desktop\inmemory_output.yaml", yaml);
        Console.WriteLine("✅ In-Memory CSV to YAML conversion completed.");
        Console.WriteLine($"   📋 YAML preview: {yaml.Substring(0, Math.Min(200, yaml.Length))}...");

        // Test XML data conversions
        TestXmlInMemoryConversions(fileConverter);

        Console.WriteLine();
    }

    static void TestXmlInMemoryConversions(FileConverter fileConverter)
    {
        Console.WriteLine("\n🔄 Testing XML In-Memory Conversions:");

        try
        {
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Products>
    <Product>
        <ProductID>1</ProductID>
        <ProductName>Laptop Pro</ProductName>
        <Price>1299.99</Price>
        <Category>Electronics</Category>
        <InStock>true</InStock>
    </Product>
    <Product>
        <ProductID>2</ProductID>
        <ProductName>Office Chair</ProductName>
        <Price>249.50</Price>
        <Category>Furniture</Category>
        <InStock>false</InStock>
    </Product>
    <Product>
        <ProductID>3</ProductID>
        <ProductName>Programming Book</ProductName>
        <Price>45.99</Price>
        <Category>Books</Category>
        <InStock>true</InStock>
    </Product>
    <Product>
        <ProductID>4</ProductID>
        <ProductName>Wireless Mouse</ProductName>
        <Price>29.99</Price>
        <Category>Electronics</Category>
        <InStock>true</InStock>
    </Product>
</Products>";

            var xmlData = new XmlData
            {
                Document = XDocument.Parse(xmlContent),
                RootElementName = "Products",
                XmlVersion = "1.0",
                Encoding = "UTF-8"
                // Headers and Rows will be ignored as Document takes precedence
            };

            // XML to CSV
            var csvOptions = new CsvConversionOptions { Delimiter = ',' };
            var csv = fileConverter.ConvertXmlToCsv(xmlData, csvOptions);
            File.WriteAllText(@"C:\Users\User\Desktop\xml_inmemory_output.csv", csv);
            Console.WriteLine("✅ In-Memory XML to CSV conversion completed.");

            // XML to JSON
            var jsonOptions = new JsonConversionOptions { ConvertValues = true, UseIndentation = true };
            var json = fileConverter.ConvertXmlToJson(xmlData, jsonOptions);
            File.WriteAllText(@"C:\Users\User\Desktop\xml_inmemory_output.json", json);
            Console.WriteLine("✅ In-Memory XML to JSON conversion completed.");

            // XML to PDF
            var pdfOptions = new PdfConversionOptions
            {
                Title = "Product Catalog",
                FontSize = 10f,
                AlternateRowColors = true
            };
            var pdfBytes = fileConverter.ConvertXmlToPdf(xmlData, pdfOptions);
            File.WriteAllBytes(@"C:\Users\User\Desktop\xml_inmemory_output.pdf", pdfBytes);
            Console.WriteLine("✅ In-Memory XML to PDF conversion completed.");

            // XML to Word
            var wordOptions = new WordConversionOptions
            {
                UseTable = true,
                FontFamily = "Arial",
                FontSize = 11
            };
            var wordBytes = fileConverter.ConvertXmlToWord(xmlData, wordOptions);
            File.WriteAllBytes(@"C:\Users\User\Desktop\xml_inmemory_output.docx", wordBytes);
            Console.WriteLine("✅ In-Memory XML to Word conversion completed.");

            // XML to YAML
            var yamlOptions = new YamlConversionOptions
            {
                Structure = "Dictionary",
                ConvertDataTypes = true
            };
            var yaml = fileConverter.ConvertXmlToYaml(xmlData, yamlOptions);
            File.WriteAllText(@"C:\Users\User\Desktop\xml_inmemory_output.yaml", yaml);
            Console.WriteLine("✅ In-Memory XML to YAML conversion completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in XML In-Memory conversions: {ex.Message}");
        }
    }
}