namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToYamlConverterTests
{
    private CsvToYamlConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToYamlConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidCsvFile_CreatesYamlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");

        // Create a sample CSV file
        await File.WriteAllTextAsync(csvFilePath, "Name,Age\nJohn,30\nJane,25");

        // Act
        await _converter.ConvertAsync(csvFilePath, yamlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenCsvFileWithDifferentDelimiter_CreatesYamlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");
        
        await File.WriteAllTextAsync(csvFilePath, "Name;Age\nJohn;30\nJane;25");

        // Act
        await _converter.ConvertAsync(csvFilePath, yamlOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }
}