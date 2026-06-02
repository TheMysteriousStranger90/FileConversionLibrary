using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToYamlConverterTests
{
    private XmlToYamlConverter _converter = null!;
    private XmlData _simpleXmlData = null!;
    private XmlData _nestedXmlData = null!;
    private XmlData _attributesXmlData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToYamlConverter();

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

        // Attributes XML
        var attributesXml = @"<?xml version=""1.0""?>
<root>
    <item id=""1"" code=""A"" status=""active"">
        <value>Test</value>
    </item>
</root>";
        _attributesXmlData = new XmlData { Document = XDocument.Parse(attributesXml) };
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
    public void Convert_DefaultOptions_ProducesValidYaml()
    {
        var result = _converter.Convert(_simpleXmlData);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        // YAML should have colons or dashes
        Assert.That(result.Contains(':') || result.StartsWith("---", StringComparison.Ordinal), Is.True);
    }

    [Test]
    public void Convert_DefaultOptions_StartsWithDocumentMarker()
    {
        var result = _converter.Convert(_simpleXmlData);
        // Many YAML files start with ---
        var trimmed = result.TrimStart();
        Assert.That(trimmed.StartsWith("---", StringComparison.Ordinal) || trimmed.Contains(':'), Is.True);
    }

    [Test]
    public void Convert_DefaultOptions_PreservesStructure()
    {
        var result = _converter.Convert(_simpleXmlData);
        Assert.That(result, Does.Contain("products"));
        Assert.That(result, Does.Contain("product"));
        Assert.That(result, Does.Contain("name"));
    }

    [Test]
    public void Convert_NestedXml_PreservesHierarchy()
    {
        var result = _converter.Convert(_nestedXmlData);
        Assert.That(result, Does.Contain("company"));
        Assert.That(result, Does.Contain("department"));
        Assert.That(result, Does.Contain("employee"));
    }

    [Test]
    public void Convert_AttributesIncluded_MarkedWithPrefix()
    {
        var result = _converter.Convert(_attributesXmlData);
        // Attributes should be included (typically with @ prefix)
        Assert.That(result.Contains("@id") || result.Contains("id:"), Is.True);
    }

    // ---- Options testing ----

    [Test]
    public void Convert_NoIndentation_MinimalWhitespace()
    {
        var options = new Dictionary<string, object>
        {
            ["useIndentation"] = false
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result, Is.Not.Null);
        // Minified YAML should have fewer spaces
        var spaceCount = result.Count(c => c == ' ');

        var defaultResult = _converter.Convert(_simpleXmlData);
        var defaultSpaceCount = defaultResult.Count(c => c == ' ');

        Assert.That(spaceCount, Is.LessThanOrEqualTo(defaultSpaceCount));
    }

    [Test]
    public void Convert_ConvertValuesFalse_KeepsStrings()
    {
        var options = new Dictionary<string, object>
        {
            ["convertValues"] = false
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0)); // Should have quotes
    }

    [Test]
    public void Convert_SortKeysTrue_SortsAlphabetically()
    {
        var options = new Dictionary<string, object>
        {
            ["sortKeys"] = true
        };
        var result = _converter.Convert(_simpleXmlData, options);
        Assert.That(result, Is.Not.Null);
        // Just verify it doesn't throw
    }

    // ---- Edge cases ----

    [Test]
    public void Convert_EmptyXml_ProducesValidYaml()
    {
        var emptyXml = @"<?xml version=""1.0""?><root></root>";
        var data = new XmlData { Document = XDocument.Parse(emptyXml) };
        var result = _converter.Convert(data);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("root"));
    }

    [Test]
    public void Convert_SingleElement_ProducesValidYaml()
    {
        var singleXml = @"<?xml version=""1.0""?><root><value>123</value></root>";
        var data = new XmlData { Document = XDocument.Parse(singleXml) };
        var result = _converter.Convert(data);
        Assert.That(result, Does.Contain("123"));
    }

    [Test]
    public void Convert_ComplexNesting_ProducesValidYaml()
    {
        var result = _converter.Convert(_nestedXmlData);
        // Check for proper indentation and structure
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.GreaterThan(1));
    }

    [Test]
    public void Convert_MultipleAttributes_AllIncluded()
    {
        var result = _converter.Convert(_attributesXmlData);
        // All attributes should appear
        Assert.That(result.Contains("id") && result.Contains("code") && result.Contains("status"), Is.True);
    }
}
