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
    public void Convert_GivenValidCsvFile_CreatesWordFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var wordOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.docx");

        // Act
        _converter.ConvertCsvToWord(csvFilePath, wordOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(wordOutputPath));
    }
        
    [Test]
    public void Convert_GivenCsvFileWithDifferentDelimiter_CreatesWordFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var wordOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.docx");

        // Act
        _converter.ConvertCsvToWord(csvFilePath, wordOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(wordOutputPath));
    }
}