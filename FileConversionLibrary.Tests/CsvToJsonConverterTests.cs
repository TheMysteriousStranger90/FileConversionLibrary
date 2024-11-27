using Newtonsoft.Json;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToJsonConverterTests
{
    private CsvToJsonConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToJsonConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidCsvFile_CreatesValidJsonFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var jsonOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.json");
        
        await File.WriteAllTextAsync(csvFilePath, "Name,Age\nJohn,30\nJane,25");

        // Act
        await _converter.ConvertAsync(csvFilePath, jsonOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(jsonOutputPath));
        var json = await File.ReadAllTextAsync(jsonOutputPath);
        Assert.IsNotNull(JsonConvert.DeserializeObject(json));
    }

    [Test]
    public async Task ConvertAsync_GivenCsvFileWithDifferentDelimiter_CreatesValidJsonFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var jsonOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.json");
        
        await File.WriteAllTextAsync(csvFilePath, "Name;Age\nJohn;30\nJane;25");

        // Act
        await _converter.ConvertAsync(csvFilePath, jsonOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(jsonOutputPath));
        var json = await File.ReadAllTextAsync(jsonOutputPath);
        Assert.IsNotNull(JsonConvert.DeserializeObject(json));
    }
}