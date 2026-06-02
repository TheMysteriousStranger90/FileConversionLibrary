using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using System.IO.Compression;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToWordConverterTests
{
    private XmlToWordConverter _converter = null!;
    private XmlData _simpleXmlData = null!;
    private XmlData _complexXmlData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToWordConverter();

        // Simple XML
        var simpleXml = @"<?xml version=""1.0""?>
<products>
    <product id=""1"">
        <name>Widget</name>
        <price>19.99</price>
    </product>
    <product id=""2"">
        <name>Gadget</name>
        <price>29.99</price>
    </product>
</products>";
        _simpleXmlData = new XmlData { Document = XDocument.Parse(simpleXml) };

        // Complex nested XML
        var complexXml = @"<?xml version=""1.0""?>
<company>
    <department name=""Engineering"">
        <employee id=""101"">
            <name>John Doe</name>
            <role>Developer</role>
        </employee>
    </department>
</company>";
        _complexXmlData = new XmlData { Document = XDocument.Parse(complexXml) };
    }

    // ---- Argument guards ----

    [Test]
    public void Convert_NullInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _converter.Convert(null!));
    }

    [Test]
    public void Convert_NullDocument_ThrowsArgumentException()
    {
        var data = new XmlData { Document = null! };
        Assert.Throws<ArgumentException>(() => _converter.Convert(data));
    }

    // ---- Default behaviour ----

    [Test]
    public void Convert_DefaultOptions_ReturnsByteArray()
    {
        var result = _converter.Convert(_simpleXmlData);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Convert_DefaultOptions_IsValidZipFile()
    {
        var result = _converter.Convert(_simpleXmlData);
        // ZIP/DOCX magic bytes: PK (0x50 0x4B)
        Assert.That(result[0], Is.EqualTo(0x50)); // P
        Assert.That(result[1], Is.EqualTo(0x4B)); // K
    }

    [Test]
    public void Convert_DefaultOptions_ContainsWordDocumentXml()
    {
        var result = _converter.Convert(_simpleXmlData);
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var documentEntry = archive.GetEntry("word/document.xml");
        Assert.That(documentEntry, Is.Not.Null, "word/document.xml should exist");

        using var reader = new StreamReader(documentEntry!.Open());
        var content = reader.ReadToEnd();
        Assert.That(content, Does.Contain("<w:document"));
    }

    [Test]
    public void Convert_DefaultOptions_ValidDocumentStructure()
    {
        var result = _converter.Convert(_simpleXmlData);
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var documentEntry = archive.GetEntry("word/document.xml");
        using var docStream = documentEntry!.Open();
        var doc = XDocument.Load(docStream);

        Assert.That(doc.Root, Is.Not.Null);
        Assert.That(doc.Root!.Name.LocalName, Is.EqualTo("document"));
    }

    // ---- Options testing ----

    [Test]
    public void Convert_UseTableTrue_ProducesValidDocx()
    {
        var options = new Dictionary<string, object>
        {
            ["useTable"] = true
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x50)); // Still valid ZIP/DOCX
    }

    [Test]
    public void Convert_LandscapeOrientation_ProducesValidDocx()
    {
        var options = new Dictionary<string, object>
        {
            ["orientation"] = "landscape"
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x50)); // Still valid ZIP/DOCX
    }

    [Test]
    public void Convert_CustomFontSize_ProducesValidDocx()
    {
        var options = new Dictionary<string, object>
        {
            ["fontSize"] = 14
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x50)); // Still valid ZIP/DOCX
    }

    [Test]
    public void Convert_HierarchicalView_ProducesValidDocx()
    {
        var options = new Dictionary<string, object>
        {
            ["hierarchicalView"] = true
        };
        var result = _converter.Convert(_complexXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x50)); // Still valid ZIP/DOCX
    }

    // ---- Edge cases ----

    [Test]
    public void Convert_EmptyXml_ProducesValidDocx()
    {
        var emptyXml = @"<?xml version=""1.0""?><root></root>";
        var data = new XmlData { Document = XDocument.Parse(emptyXml) };
        var result = _converter.Convert(data);
        Assert.That(result[0], Is.EqualTo(0x50)); // Still valid ZIP/DOCX
    }

    [Test]
    public void Convert_SingleElement_ProducesValidDocx()
    {
        var singleXml = @"<?xml version=""1.0""?><root><value>Test</value></root>";
        var data = new XmlData { Document = XDocument.Parse(singleXml) };
        var result = _converter.Convert(data);
        Assert.That(result[0], Is.EqualTo(0x50)); // Still valid ZIP/DOCX
    }

    [Test]
    public void Convert_ComplexNesting_ProducesValidDocx()
    {
        var result = _converter.Convert(_complexXmlData);
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var documentEntry = archive.GetEntry("word/document.xml");
        Assert.That(documentEntry, Is.Not.Null);
    }
}
