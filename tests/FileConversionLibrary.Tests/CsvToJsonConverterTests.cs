using System.Text.Json;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using FileConversionLibrary.Models.Options;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToJsonConverterTests
{
    private CsvToJsonConverter _converter = null!;
    private CsvData _simpleCsvData = null!;
    private CsvData _typedCsvData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToJsonConverter();

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

        _typedCsvData = new CsvData
        {
            Headers = ["ID", "Price", "InStock", "Notes"],
            Rows =
            [
                ["1", "19.99", "true", "good"],
                ["2", "5.50", "false", ""]
            ]
        };
    }

    // ---- Null / argument guard ----

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
    public void Convert_DefaultOptions_ProducesValidJson()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void Convert_DefaultOptions_ReturnsArray()
    {
        var result = _converter.Convert(_simpleCsvData);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public void Convert_DefaultOptions_CorrectRowCount()
    {
        var result = _converter.Convert(_simpleCsvData);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(3));
    }

    [Test]
    public void Convert_DefaultOptions_StringValuesPreserved()
    {
        var result = _converter.Convert(_simpleCsvData);
        using var doc = JsonDocument.Parse(result);
        var name = doc.RootElement[0].GetProperty("Name").GetString();
        Assert.That(name, Is.EqualTo("Alice"));
    }

    // ---- ConvertValues ----

    [Test]
    public void Convert_ConvertValuesTrue_IntegerParsed()
    {
        var opts = new Dictionary<string, object> { ["convertValues"] = true };
        var result = _converter.Convert(_typedCsvData, opts);
        using var doc = JsonDocument.Parse(result);
        var id = doc.RootElement[0].GetProperty("ID");
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.Number));
        Assert.That(id.GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public void Convert_ConvertValuesTrue_BooleanParsed()
    {
        var opts = new Dictionary<string, object> { ["convertValues"] = true };
        var result = _converter.Convert(_typedCsvData, opts);
        using var doc = JsonDocument.Parse(result);
        var inStock = doc.RootElement[0].GetProperty("InStock");
        Assert.That(inStock.ValueKind, Is.EqualTo(JsonValueKind.True));
    }

    [Test]
    public void Convert_ConvertValuesFalse_AllStrings()
    {
        var opts = new Dictionary<string, object> { ["convertValues"] = false };
        var result = _converter.Convert(_typedCsvData, opts);
        using var doc = JsonDocument.Parse(result);
        var id = doc.RootElement[0].GetProperty("ID");
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
    }

    // ---- UseIndentation ----

    [Test]
    public void Convert_UseIndentationFalse_ProducesMinifiedJson()
    {
        var opts = new Dictionary<string, object> { ["useIndentation"] = false };
        var result = _converter.Convert(_simpleCsvData, opts);
        Assert.That(result, Does.Not.Contain("\n"));
    }

    [Test]
    public void Convert_UseIndentationTrue_ContainsNewlines()
    {
        var opts = new Dictionary<string, object> { ["useIndentation"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        Assert.That(result, Does.Contain("\n").Or.Contain(Environment.NewLine));
    }

    // ---- IncludeRowNumbers ----

    [Test]
    public void Convert_IncludeRowNumbersTrue_AddsRowNumberField()
    {
        var opts = new Dictionary<string, object> { ["includeRowNumbers"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement[0].TryGetProperty("_rowNumber", out var rn), Is.True);
        Assert.That(rn.GetInt32(), Is.EqualTo(1));
    }

    // ---- PreserveEmptyValues ----

    [Test]
    public void Convert_PreserveEmptyValuesFalse_OmitsEmptyFields()
    {
        var opts = new Dictionary<string, object> { ["preserveEmptyValues"] = false };
        var result = _converter.Convert(_typedCsvData, opts);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement[1].TryGetProperty("Notes", out _), Is.False);
    }

    [Test]
    public void Convert_PreserveEmptyValuesTrue_KeepsEmptyFields()
    {
        var opts = new Dictionary<string, object> { ["preserveEmptyValues"] = true };
        var result = _converter.Convert(_typedCsvData, opts);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement[1].TryGetProperty("Notes", out _), Is.True);
    }

    // ---- GroupByColumn ----

    [Test]
    public void Convert_GroupByColumn_ProducesObjectWithGroups()
    {
        var data = new CsvData
        {
            Headers = ["Department", "Name", "Salary"],
            Rows =
            [
                ["Engineering", "Alice", "90000"],
                ["Marketing",   "Bob",   "70000"],
                ["Engineering", "Carol", "85000"]
            ]
        };
        var opts = new Dictionary<string, object>
        {
            ["groupByColumn"] = true,
            ["groupByColumnName"] = "Department"
        };
        var result = _converter.Convert(data, opts);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(doc.RootElement.TryGetProperty("Engineering", out _), Is.True);
        Assert.That(doc.RootElement.TryGetProperty("Marketing", out _), Is.True);
    }

    // ---- Nested objects ----

    [Test]
    public void Convert_CreateNestedObjects_NestedPropertyCreated()
    {
        var data = new CsvData
        {
            Headers = ["Address.City", "Address.Zip", "Name"],
            Rows = [["London", "EC1", "Alice"]]
        };
        var opts = new Dictionary<string, object>
        {
            ["createNestedObjects"] = true,
            ["nestedSeparator"] = "."
        };
        var result = _converter.Convert(data, opts);
        using var doc = JsonDocument.Parse(result);
        var row = doc.RootElement[0];
        Assert.That(row.TryGetProperty("Address", out var addr), Is.True);
        Assert.That(addr.TryGetProperty("City", out var city), Is.True);
        Assert.That(city.GetString(), Is.EqualTo("London"));
    }

    // ---- ConvertArrays ----

    [Test]
    public void Convert_ConvertArraysTrue_SemicolonDelimitedBecomesArray()
    {
        var data = new CsvData
        {
            Headers = ["Name", "Tags"],
            Rows = [["Alice", "red;green;blue"]]
        };
        var opts = new Dictionary<string, object>
        {
            ["convertArrays"] = true,
            ["arrayDelimiter"] = ";"
        };
        var result = _converter.Convert(data, opts);
        using var doc = JsonDocument.Parse(result);
        var tags = doc.RootElement[0].GetProperty("Tags");
        Assert.That(tags.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(tags.GetArrayLength(), Is.EqualTo(3));
    }

    // ---- Options object overload ----

    [Test]
    public void Convert_WithJsonConversionOptions_UseIndentationFalse()
    {
        var result = _converter.Convert(_simpleCsvData,
            new Dictionary<string, object> { ["useIndentation"] = false });
        Assert.That(result, Does.Not.Contain("\n"));
    }

    // ---- Empty rows ----

    [Test]
    public void Convert_NoRows_ProducesEmptyJsonArray()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        using var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(0));
    }
}
