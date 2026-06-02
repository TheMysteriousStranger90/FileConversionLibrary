using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToXmlConverterTests
{
    private CsvToXmlConverter _converter = null!;
    private CsvData _simpleCsvData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToXmlConverter();

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
    public void Convert_DefaultOptions_ReturnsNonEmptyString()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Convert_DefaultOptions_ProducesValidXml()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.DoesNotThrow(() => XDocument.Parse(result));
    }

    [Test]
    public void Convert_DefaultOptions_HasRootElement()
    {
        var result = _converter.Convert(_simpleCsvData);
        var doc = XDocument.Parse(result);
        Assert.That(doc.Root, Is.Not.Null);
        Assert.That(doc.Root!.Name.LocalName, Is.EqualTo("root"));
    }

    [Test]
    public void Convert_DefaultOptions_CorrectRowCount()
    {
        var result = _converter.Convert(_simpleCsvData);
        var doc = XDocument.Parse(result);
        var rows = doc.Root!.Descendants("row").ToList();
        Assert.That(rows.Count, Is.EqualTo(3));
    }

    [Test]
    public void Convert_DefaultOptions_DataValuesPresent()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result, Does.Contain("Alice"));
        Assert.That(result, Does.Contain("London"));
    }

    // ---- CData ----

    [Test]
    public void Convert_UseCDataTrue_OutputContainsCData()
    {
        // CData is only added when value contains special XML chars (<, >, &, \n)
        var data = new CsvData
        {
            Headers = ["Name", "Description"],
            Rows = [["Alice", "A & B < C > D"]]
        };
        var opts = new Dictionary<string, object> { ["useCData"] = true };
        var result = _converter.Convert(data, opts);
        Assert.That(result, Does.Contain("<![CDATA["));
    }

    [Test]
    public void Convert_UseCDataFalse_OutputDoesNotContainCData()
    {
        var data = new CsvData
        {
            Headers = ["Name", "Description"],
            Rows = [["Alice", "A & B < C > D"]]
        };
        var opts = new Dictionary<string, object> { ["useCData"] = false };
        var result = _converter.Convert(data, opts);
        Assert.That(result, Does.Not.Contain("<![CDATA["));
    }

    // ---- IncludeRowNumbers ----

    [Test]
    public void Convert_IncludeRowNumbersTrue_RowsHaveNumberAttribute()
    {
        var opts = new Dictionary<string, object> { ["includeRowNumbers"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        var doc = XDocument.Parse(result);
        var firstRow = doc.Root!.Descendants("row").First();
        var numberAttr = firstRow.Attribute("number");
        Assert.That(numberAttr, Is.Not.Null);
        Assert.That(numberAttr!.Value, Is.EqualTo("1"));
    }

    // ---- Attributes output format ----
    // The option key is "format" (not "outputFormat")

    [Test]
    public void Convert_AttributesFormat_ValuesAsAttributes()
    {
        var opts = new Dictionary<string, object> { ["format"] = CsvToXmlConverter.XmlOutputFormat.Attributes };
        var result = _converter.Convert(_simpleCsvData, opts);
        var doc = XDocument.Parse(result);
        var firstRow = doc.Root!.Descendants("row").First();
        Assert.That(firstRow.Attribute("Name")?.Value, Is.EqualTo("Alice"));
    }

    // ---- Empty rows ----

    [Test]
    public void Convert_NoRows_ProducesValidXml()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        Assert.DoesNotThrow(() => XDocument.Parse(result));
    }

    [Test]
    public void Convert_NoRows_RootHasNoRowChildren()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        var doc = XDocument.Parse(result);
        var rows = doc.Root!.Descendants("row").ToList();
        Assert.That(rows.Count, Is.EqualTo(0));
    }
}
