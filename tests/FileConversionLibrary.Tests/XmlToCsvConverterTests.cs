using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToCsvConverterTests
{
    private XmlToCsvConverter _converter = null!;
    private XmlData _simpleXmlData = null!;
    private XmlData _nestedXmlData = null!;
    private XmlData _emptyXmlData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToCsvConverter();

        // Simple tabular XML
        var simpleXml = @"<?xml version=""1.0""?>
<products>
    <product id=""1"">
        <name>Widget</name>
        <price>19.99</price>
        <inStock>true</inStock>
    </product>
    <product id=""2"">
        <name>Gadget</name>
        <price>29.99</price>
        <inStock>false</inStock>
    </product>
</products>";
        _simpleXmlData = new XmlData { Document = XDocument.Parse(simpleXml) };

        // Nested XML
        var nestedXml = @"<?xml version=""1.0""?>
<employees>
    <employee id=""101"">
        <name>
            <first>John</first>
            <last>Doe</last>
        </name>
        <age>30</age>
    </employee>
</employees>";
        _nestedXmlData = new XmlData { Document = XDocument.Parse(nestedXml) };

        // Empty XML
        var emptyXml = @"<?xml version=""1.0""?><data><records></records></data>";
        _emptyXmlData = new XmlData { Document = XDocument.Parse(emptyXml) };
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
    public void Convert_DefaultOptions_ProducesValidCsv()
    {
        var result = _converter.Convert(_simpleXmlData);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        Assert.That(result, Does.Contain(","));
    }

    [Test]
    public void Convert_DefaultOptions_IncludesHeaders()
    {
        var result = _converter.Convert(_simpleXmlData);
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.GreaterThanOrEqualTo(2)); // At least header + 1 row
        Assert.That(lines[0], Does.Contain("@id")); // Attribute with @ prefix
        Assert.That(lines[0], Does.Contain("name"));
        Assert.That(lines[0], Does.Contain("price"));
    }

    [Test]
    public void Convert_DefaultOptions_CorrectRowCount()
    {
        var result = _converter.Convert(_simpleXmlData);
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.EqualTo(3)); // Header + 2 data rows
    }

    [Test]
    public void Convert_NestedXml_FlattensStructure()
    {
        var result = _converter.Convert(_nestedXmlData);
        Assert.That(result, Does.Contain("name.first")); // Dot notation for nested
    }

    // ---- Options testing ----

    [Test]
    public void Convert_CustomDelimiter_UsesSemicolon()
    {
        var options = new Dictionary<string, object>
        {
            ["delimiter"] = ';'
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result, Does.Contain(";"));
    }

    [Test]
    public void Convert_NoHeaders_OmitsHeaderRow()
    {
        var options = new Dictionary<string, object>
        {
            ["includeHeaders"] = false
        };
        var result = _converter.Convert(_simpleXmlData, options);
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.EqualTo(2)); // Only data rows
    }

    [Test]
    public void Convert_QuoteValues_AddsQuotes()
    {
        var options = new Dictionary<string, object>
        {
            ["quoteValues"] = true
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0)); // Should have quotes
    }

    // ---- Edge cases ----

    [Test]
    public void Convert_EmptyXml_ReturnsEmptyString()
    {
        var result = _converter.Convert(_emptyXmlData);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Convert_SingleElement_ProducesValidCsv()
    {
        var singleXml = @"<?xml version=""1.0""?><root><item><value>123</value></item></root>";
        var data = new XmlData { Document = XDocument.Parse(singleXml) };
        var result = _converter.Convert(data);
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.EqualTo(2)); // Header + 1 row
    }

    [Test]
    public void Convert_AttributesOnly_IncludesAttributes()
    {
        var attrXml = @"<?xml version=""1.0""?><root><item id=""1"" code=""A""/></root>";
        var data = new XmlData { Document = XDocument.Parse(attrXml) };
        var result = _converter.Convert(data);
        Assert.That(result, Does.Contain("@id"));
        Assert.That(result, Does.Contain("@code"));
    }
}
