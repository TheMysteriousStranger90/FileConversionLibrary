using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using System.Text.Json;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToJsonConverterTests
{
    private XmlToJsonConverter _converter = null!;
    private XmlData _simpleXmlData = null!;
    private XmlData _nestedXmlData = null!;
    private XmlData _cdataXmlData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToJsonConverter();

        // Simple XML
        var simpleXml = @"<?xml version=""1.0""?>
<products>
    <product id=""1"">
        <name>Widget</name>
        <price>19.99</price>
    </product>
</products>";
        _simpleXmlData = new XmlData { Document = XDocument.Parse(simpleXml) };

        // Nested XML
        var nestedXml = @"<?xml version=""1.0""?>
<company>
    <department name=""Engineering"">
        <employee id=""101"">
            <name>John Doe</name>
            <role>Developer</role>
        </employee>
    </department>
</company>";
        _nestedXmlData = new XmlData { Document = XDocument.Parse(nestedXml) };

        // CData XML
        var cdataXml = @"<?xml version=""1.0""?>
<root>
    <item>
        <description><![CDATA[This is <b>HTML</b> content]]></description>
    </item>
</root>";
        _cdataXmlData = new XmlData { Document = XDocument.Parse(cdataXml) };
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
    public void Convert_DefaultOptions_ProducesValidJson()
    {
        var result = _converter.Convert(_simpleXmlData);
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void Convert_DefaultOptions_PreservesStructure()
    {
        var result = _converter.Convert(_simpleXmlData);
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(root.TryGetProperty("products", out _), Is.True);
    }

    [Test]
    public void Convert_NestedXml_PreservesHierarchy()
    {
        var result = _converter.Convert(_nestedXmlData);
        using var doc = JsonDocument.Parse(result);
        Assert.That(result, Does.Contain("company"));
        Assert.That(result, Does.Contain("department"));
        Assert.That(result, Does.Contain("employee"));
    }

    [Test]
    public void Convert_AttributesIncluded_PrefixedWithAt()
    {
        var result = _converter.Convert(_simpleXmlData);
        Assert.That(result, Does.Contain("@id")); // Attribute with @ prefix
    }

    // ---- Options testing ----

    [Test]
    public void Convert_NoIndentation_MinifiedJson()
    {
        var options = new Dictionary<string, object>
        {
            ["useIndentation"] = false
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result, Is.Not.Null);
        // Minified JSON should not have newlines (or very few)
        var lineCount = result.Split('\n').Length;
        Assert.That(lineCount, Is.LessThanOrEqualTo(3)); // Allow for minimal breaks
    }

    [Test]
    public void Convert_ConvertValuesFalse_KeepsStrings()
    {
        var options = new Dictionary<string, object>
        {
            ["convertValues"] = false
        };
        var result = _converter.Convert(_simpleXmlData, options);
        using var doc = JsonDocument.Parse(result);
        // Values should be strings, not numbers
        Assert.That(result, Does.Contain("\"19.99\"")); // Quoted price
    }

    [Test]
    public void Convert_PreserveCDataTrue_KeepsCDataContent()
    {
        var options = new Dictionary<string, object>
        {
            ["preserveCData"] = true
        };
        var result = _converter.Convert(_cdataXmlData, options);
        Assert.That(result, Does.Contain("<b>HTML</b>")); // CData content preserved
    }

    // ---- Edge cases ----

    [Test]
    public void Convert_EmptyXml_ProducesValidJson()
    {
        var emptyXml = @"<?xml version=""1.0""?><root></root>";
        var data = new XmlData { Document = XDocument.Parse(emptyXml) };
        var result = _converter.Convert(data);
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
        Assert.That(result, Does.Contain("root"));
    }

    [Test]
    public void Convert_SingleElement_ProducesValidJson()
    {
        var singleXml = @"<?xml version=""1.0""?><root><value>123</value></root>";
        var data = new XmlData { Document = XDocument.Parse(singleXml) };
        var result = _converter.Convert(data);
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
        Assert.That(result, Does.Contain("123"));
    }

    [Test]
    public void Convert_ComplexNesting_ProducesValidJson()
    {
        var complexXml = @"<?xml version=""1.0""?>
<library>
    <books>
        <book id=""1"">
            <title>Book One</title>
            <authors>
                <author>Author A</author>
                <author>Author B</author>
            </authors>
        </book>
    </books>
</library>";
        var data = new XmlData { Document = XDocument.Parse(complexXml) };
        var result = _converter.Convert(data);
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
        Assert.That(result, Does.Contain("authors"));
    }
}
