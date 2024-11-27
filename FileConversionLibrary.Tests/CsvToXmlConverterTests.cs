using NUnit.Framework;
using System.IO;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToXmlConverterTests
{
    private CsvToXmlConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToXmlConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidCsvFile_CreatesValidXmlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var xmlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");

        await File.WriteAllTextAsync(csvFilePath, "Name,Age\nJohn,30\nJane,25");

        // Act
        await _converter.ConvertAsync(csvFilePath, xmlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(xmlOutputPath));
        var doc = XDocument.Load(xmlOutputPath);
        Assert.IsNotNull(doc.Root);
    }

    [Test]
    public async Task ConvertAsync_GivenCsvFileWithDifferentDelimiter_CreatesValidXmlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var xmlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");

        await File.WriteAllTextAsync(csvFilePath, "Name;Age\nJohn;30\nJane;25");

        // Act
        await _converter.ConvertAsync(csvFilePath, xmlOutputPath, ';');

        // Assert
        Assert.IsTrue(File.Exists(xmlOutputPath));
        var doc = XDocument.Load(xmlOutputPath);
        Assert.IsNotNull(doc.Root);
    }

    [Test]
    public async Task ConvertAsync_GivenCsvFileWithQuotedFields_CreatesValidXmlFile()
    {
        // Arrange
        var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        var xmlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");

        await File.WriteAllTextAsync(csvFilePath, "\"Name\",\"Age\"\n\"John\",\"30\"\n\"Jane\",\"25\"");

        // Act
        await _converter.ConvertAsync(csvFilePath, xmlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(xmlOutputPath));
        var doc = XDocument.Load(xmlOutputPath);
        Assert.IsNotNull(doc.Root);
    }
}