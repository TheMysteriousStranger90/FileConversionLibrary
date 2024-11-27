namespace FileConversionLibrary.Tests;

[TestFixture]
public class XmlToYamlConverterTests
{
    private XmlToYamlConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new XmlToYamlConverter();
    }

    [Test]
    public async Task ConvertAsync_GivenValidXmlFile_CreatesYamlFile()
    {
        // Arrange
        var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");

        // Create a sample XML file
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
        await _converter.ConvertAsync(xmlFilePath, yamlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenXmlFileWithNestedElements_CreatesYamlFile()
    {
        // Arrange
        var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_nested_elements.xml");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");

        // Create a sample XML file with nested elements
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
        await _converter.ConvertAsync(xmlFilePath, yamlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }

    [Test]
    public async Task ConvertAsync_GivenXmlFileWithAttributes_CreatesYamlFile()
    {
        // Arrange
        var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_attributes.xml");
        var yamlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.yaml");

        // Create a sample XML file with attributes
        var xmlContent = @"<root>
                                <element Name='John' Age='30' />
                                <element Name='Jane' Age='25' />
                               </root>";
        await File.WriteAllTextAsync(xmlFilePath, xmlContent);

        // Act
        await _converter.ConvertAsync(xmlFilePath, yamlOutputPath);

        // Assert
        Assert.IsTrue(File.Exists(yamlOutputPath));
    }
}