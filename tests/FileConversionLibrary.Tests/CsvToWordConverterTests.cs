using System.IO.Compression;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToWordConverterTests
{
    private CsvToWordConverter _converter = null!;
    private CsvData _simpleCsvData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToWordConverter();

        _simpleCsvData = new CsvData
        {
            Headers = ["Name", "Age", "City"],
            Rows =
            [
                ["Alice", "28", "London"],
                ["Bob", "35", "Paris"],
                ["Carol", "22", "Berlin"]
            ]
        };
    }

    private static string ReadDocumentXml(byte[] docxBytes)
    {
        using var ms = new MemoryStream(docxBytes);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        var entry = zip.GetEntry("word/document.xml")
                    ?? throw new InvalidDataException("word/document.xml not found");
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }

    // ---- Argument guards ----

    [Test]
    public void Convert_NullInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _converter.Convert(null!));
    }

    [Test]
    public void Convert_EmptyHeaders_ThrowsArgumentException()
    {
        var data = new CsvData { Headers = [], Rows = [] };
        Assert.Throws<ArgumentException>(() => _converter.Convert(data));
    }

    // ---- Default behaviour ----

    [Test]
    public void Convert_DefaultOptions_ReturnsByteArray()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Convert_DefaultOptions_StartsWithZipMagicBytes()
    {
        var result = _converter.Convert(_simpleCsvData);
        // ZIP/OOXML magic bytes: PK (0x50 0x4B)
        Assert.That(result[0], Is.EqualTo(0x50));
        Assert.That(result[1], Is.EqualTo(0x4B));
    }

    [Test]
    public void Convert_DefaultOptions_ContainsWordDocumentXml()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.DoesNotThrow(() => ReadDocumentXml(result));
    }

    [Test]
    public void Convert_DefaultOptions_ContainsTable()
    {
        var result = _converter.Convert(_simpleCsvData);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("<w:tbl"));
    }

    [Test]
    public void Convert_DefaultOptions_HeaderCellsPresent()
    {
        var result = _converter.Convert(_simpleCsvData);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("Name"));
        Assert.That(xml, Does.Contain("Age"));
        Assert.That(xml, Does.Contain("City"));
    }

    // ---- UseTable = false ----

    [Test]
    public void Convert_UseTableFalse_NoTableInDocument()
    {
        var opts = new Dictionary<string, object> { ["useTable"] = false };
        var result = _converter.Convert(_simpleCsvData, opts);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Not.Contain("<w:tbl"));
    }

    // ---- Page orientation ----

    [Test]
    public void Convert_LandscapeOrientation_DocumentXmlContainsLandscape()
    {
        var opts = new Dictionary<string, object> { ["pageOrientation"] = "landscape" };
        var result = _converter.Convert(_simpleCsvData, opts);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("landscape"));
    }

    // ---- IncludeRowNumbers ----

    [Test]
    public void Convert_IncludeRowNumbersTrue_DocumentContainsRowNumber()
    {
        var opts = new Dictionary<string, object> { ["includeRowNumbers"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("1"));
    }

    // ---- FormatAsHierarchy ----

    [Test]
    public void Convert_FormatAsHierarchyTrue_DocumentContainsRecordText()
    {
        var opts = new Dictionary<string, object> { ["formatAsHierarchy"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("Record"));
    }

    // ---- Title ----

    [Test]
    public void Convert_CustomTitle_DocumentContainsTitle()
    {
        var opts = new Dictionary<string, object> { ["title"] = "My Custom Report" };
        var result = _converter.Convert(_simpleCsvData, opts);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("My Custom Report"));
    }

    // ---- Empty rows ----

    [Test]
    public void Convert_NoRows_ProducesValidDocx()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        Assert.DoesNotThrow(() => ReadDocumentXml(result));
    }

    [Test]
    public void Convert_NoRows_ContainsNoDataMessage()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        var xml = ReadDocumentXml(result);
        Assert.That(xml, Does.Contain("No data available"));
    }
}
