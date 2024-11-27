namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToPdfConverterTests
{
    private CsvToPdfConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToPdfConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidCsvFile_CreatesPdfFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");
        
        await File.WriteAllTextAsync(csvFilePath, "Name,Age\nJohn,30\nJane,25");

        // Act
        await _converter.ConvertAsync(csvFilePath, pdfOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(pdfOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenCsvFileWithDifferentDelimiter_CreatesPdfFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");
        
        await File.WriteAllTextAsync(csvFilePath, "Name;Age\nJohn;30\nJane;25");

        // Act
        await _converter.ConvertAsync(csvFilePath, pdfOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(pdfOutputPath));
    }
}