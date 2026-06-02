using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToPdfConverterTests
{
    private XmlToPdfConverter _converter = null!;
    private XmlData _simpleXmlData = null!;
    private XmlData _complexXmlData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToPdfConverter();

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
        <employee id=""102"">
            <name>Jane Smith</name>
            <role>Manager</role>
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
    public void Convert_DefaultOptions_StartsWithPdfMagicBytes()
    {
        var result = _converter.Convert(_simpleXmlData);
        // PDF magic bytes: %PDF (0x25 0x50 0x44 0x46)
        Assert.That(result[0], Is.EqualTo(0x25)); // %
        Assert.That(result[1], Is.EqualTo(0x50)); // P
        Assert.That(result[2], Is.EqualTo(0x44)); // D
        Assert.That(result[3], Is.EqualTo(0x46)); // F
    }

    [Test]
    public void Convert_DefaultOptions_ValidPdfStructure()
    {
        var result = _converter.Convert(_simpleXmlData);
        var pdfString = System.Text.Encoding.ASCII.GetString(result);
        Assert.That(pdfString, Does.Contain("%PDF"));
        Assert.That(pdfString, Does.Contain("%%EOF"));
    }

    // ---- Options testing ----

    [Test]
    public void Convert_LandscapeOrientation_ProducesValidPdf()
    {
        var options = new Dictionary<string, object>
        {
            ["orientation"] = "landscape"
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x25)); // Still valid PDF
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Convert_CustomPageSize_ProducesValidPdf()
    {
        var options = new Dictionary<string, object>
        {
            ["pageSize"] = "A3"
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x25)); // Still valid PDF
    }

    [Test]
    public void Convert_CustomFontSize_ProducesValidPdf()
    {
        var options = new Dictionary<string, object>
        {
            ["fontSize"] = 14
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x25)); // Still valid PDF
    }

    [Test]
    public void Convert_HierarchicalView_ProducesValidPdf()
    {
        var options = new Dictionary<string, object>
        {
            ["hierarchicalView"] = true
        };
        var result = _converter.Convert(_complexXmlData, options);
        Assert.That(result[0], Is.EqualTo(0x25)); // Still valid PDF
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    // ---- Edge cases ----

    [Test]
    public void Convert_EmptyXml_ProducesValidPdf()
    {
        var emptyXml = @"<?xml version=""1.0""?><root></root>";
        var data = new XmlData { Document = XDocument.Parse(emptyXml) };
        var result = _converter.Convert(data);
        Assert.That(result[0], Is.EqualTo(0x25)); // Still valid PDF
    }

    [Test]
    public void Convert_SingleElement_ProducesValidPdf()
    {
        var singleXml = @"<?xml version=""1.0""?><root><value>Test</value></root>";
        var data = new XmlData { Document = XDocument.Parse(singleXml) };
        var result = _converter.Convert(data);
        Assert.That(result[0], Is.EqualTo(0x25)); // Still valid PDF
    }

    [Test]
    public void Convert_DeepNesting_ProducesValidPdf()
    {
        var result = _converter.Convert(_complexXmlData);
        var pdfString = System.Text.Encoding.ASCII.GetString(result);
        Assert.That(pdfString, Does.Contain("%%EOF")); // Valid PDF end
    }
}
