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
    public void Convert_GivenValidCsvFile_CreatesValidJsonFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var jsonOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.json");

        // Act
        _converter.ConvertCsvToJson(csvFilePath, jsonOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(jsonOutputPath));
        var json = File.ReadAllText(jsonOutputPath);
        Assert.IsNotNull(JsonConvert.DeserializeObject(json));
    }
        
    [Test]
    public void Convert_GivenCsvFileWithDifferentDelimiter_CreatesValidJsonFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var jsonOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.json");

        // Act
        _converter.ConvertCsvToJson(csvFilePath, jsonOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(jsonOutputPath));
        var json = File.ReadAllText(jsonOutputPath);
        Assert.IsNotNull(JsonConvert.DeserializeObject(json));
    }
}