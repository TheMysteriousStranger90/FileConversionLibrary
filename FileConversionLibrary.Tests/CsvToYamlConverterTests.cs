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
    public void Convert_GivenValidCsvFile_CreatesYamlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");

        // Act
        _converter.ConvertCsvToYaml(csvFilePath, yamlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }
        
    [Test]
    public void Convert_GivenCsvFileWithDifferentDelimiter_CreatesYamlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");

        // Act
        _converter.ConvertCsvToYaml(csvFilePath, yamlOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }
}