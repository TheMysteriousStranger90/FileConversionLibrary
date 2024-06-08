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
    public void Convert_GivenValidCsvFile_CreatesPdfFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");

        // Act
        _converter.ConvertCsvToPdf(csvFilePath, pdfOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(pdfOutputPath));
    }
        
    [Test]
    public void Convert_GivenCsvFileWithDifferentDelimiter_CreatesPdfFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");

        // Act
        _converter.ConvertCsvToPdf(csvFilePath, pdfOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(pdfOutputPath));
    }
}