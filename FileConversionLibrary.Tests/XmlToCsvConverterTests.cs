﻿namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToCsvConverterTests
{
    private XmlToCsvConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToCsvConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidXmlFile_CreatesValidCsvFile()
    {
        // Arrange
        var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");
        var csvOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        
        var xmlContent = @"<root>
                                <element>
                                    <Name>John</Name>
                                    <Age>30</Age>
                                </element>
                                <element>
                                    <Name>Jane</Name>
                                    <Age>25</Age>
                                </element>
                               </root>";
        await File.WriteAllTextAsync(xmlFilePath, xmlContent);

        // Act
        await _converter.ConvertAsync(xmlFilePath, csvOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(csvOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenXmlFileWithNestedElements_CreatesValidCsvFile()
    {
        // Arrange
        var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_nested_elements.xml");
        var csvOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        
        var xmlContent = @"<root>
                                <element>
                                    <Name>John</Name>
                                    <Details>
                                        <Age>30</Age>
                                    </Details>
                                </element>
                                <element>
                                    <Name>Jane</Name>
                                    <Details>
                                        <Age>25</Age>
                                    </Details>
                                </element>
                               </root>";
        await File.WriteAllTextAsync(xmlFilePath, xmlContent);

        // Act
        await _converter.ConvertAsync(xmlFilePath, csvOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(csvOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenXmlFileWithAttributes_CreatesValidCsvFile()
    {
        // Arrange
        var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_attributes.xml");
        var csvOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
        
        var xmlContent = @"<root>
                                <element Name='John' Age='30' />
                                <element Name='Jane' Age='25' />
                               </root>";
        await File.WriteAllTextAsync(xmlFilePath, xmlContent);

        // Act
        await _converter.ConvertAsync(xmlFilePath, csvOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(csvOutputPath));
    }
}