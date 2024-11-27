namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToWordConverterTests
{
    private CsvToWordConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToWordConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidCsvFile_CreatesWordFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var wordOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.docx");
        
        await File.WriteAllTextAsync(csvFilePath, "Name,Age\nJohn,30\nJane,25");

        // Act
        await _converter.ConvertAsync(csvFilePath, wordOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(wordOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenCsvFileWithDifferentDelimiter_CreatesWordFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var wordOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.docx");
        
        await File.WriteAllTextAsync(csvFilePath, "Name;Age\nJohn;30\nJane;25");

        // Act
        await _converter.ConvertAsync(csvFilePath, wordOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(wordOutputPath));
    }
}