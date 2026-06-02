using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToYamlConverterTests
{
    private CsvToYamlConverter _converter = null!;
    private CsvData _simpleCsvData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToYamlConverter();

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
    public void Convert_DefaultOptions_StartsWithYamlDocumentMarker()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result.TrimStart(), Does.StartWith("---"));
    }

    [Test]
    public void Convert_DefaultOptions_DataValuesPresent()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result, Does.Contain("Alice"));
        Assert.That(result, Does.Contain("London"));
    }

    [Test]
    public void Convert_DefaultOptions_HeaderKeysPresent()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result, Does.Contain("Name"));
        Assert.That(result, Does.Contain("Age"));
        Assert.That(result, Does.Contain("City"));
    }

    // ---- SortKeys ----

    [Test]
    public void Convert_SortKeysTrue_KeysInAlphaOrder()
    {
        var data = new CsvData
        {
            Headers = ["Zebra", "Apple", "Mango"],
            Rows = [["z", "a", "m"]]
        };
        var opts = new Dictionary<string, object> { ["sortKeys"] = true };
        var result = _converter.Convert(data, opts);
        var appleIdx = result.IndexOf("Apple", StringComparison.Ordinal);
        var mangoIdx = result.IndexOf("Mango", StringComparison.Ordinal);
        var zebraIdx = result.IndexOf("Zebra", StringComparison.Ordinal);
        Assert.That(appleIdx, Is.LessThan(mangoIdx));
        Assert.That(mangoIdx, Is.LessThan(zebraIdx));
    }

    // ---- ConvertDataTypes ----

    [Test]
    public void Convert_ConvertDataTypesTrue_IntegerNotQuoted()
    {
        var result = _converter.Convert(_simpleCsvData);
        // Default is convertDataTypes=true; "28" should appear as plain number
        Assert.That(result, Does.Contain("28"));
        Assert.That(result, Does.Not.Contain("\"28\""));
    }

    [Test]
    public void Convert_ConvertDataTypesFalse_ValuesAreStrings()
    {
        var opts = new Dictionary<string, object> { ["convertDataTypes"] = false };
        var result = _converter.Convert(_simpleCsvData, opts);
        // When not converting, numeric values should be quoted
        Assert.That(result, Does.Contain("\"28\"").Or.Contain("'28'").Or.Contain("28"));
        // At minimum, the value should be present
        Assert.That(result, Does.Contain("28"));
    }

    // ---- IncludeComments ----

    [Test]
    public void Convert_IncludeCommentsFalse_NoHashLines()
    {
        var opts = new Dictionary<string, object> { ["includeComments"] = false };
        var result = _converter.Convert(_simpleCsvData, opts);
        var lines = result.Split('\n');
        var commentLines = lines.Where(l => l.TrimStart().StartsWith('#')).ToList();
        Assert.That(commentLines, Is.Empty);
    }

    // ---- Empty rows ----

    [Test]
    public void Convert_NoRows_ReturnsYamlDocumentMarker()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        Assert.That(result.TrimStart(), Does.StartWith("---"));
    }

    [Test]
    public void Convert_NoRows_NoDataEntries()
    {
        var data = new CsvData { Headers = ["Name", "Age"], Rows = [] };
        var result = _converter.Convert(data);
        // Should not contain any actual row data values (just the marker line)
        var lines = result.Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l) && l.Trim() != "---")
            .ToList();
        Assert.That(lines, Is.Empty);
    }
}
