using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using YamlDotNet.RepresentationModel;

namespace FileConversionLibrary.Tests
{
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
        public void Convert_GivenValidXmlData_ReturnsValidYaml()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John</Name>
                                                <Age>30</Age>
                                            </element>
                                            <element>
                                                <Name>Jane</Name>
                                                <Age>25</Age>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" },
                    new[] { "Jane", "25" }
                }
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            Assert.IsNotNull(result);
            
            // Verify the YAML can be parsed
            using var reader = new StringReader(result);
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            
            // At least one document should be present
            Assert.GreaterOrEqual(yamlStream.Documents.Count, 1);
            Assert.IsNotNull(yamlStream.Documents[0].RootNode);
        }
        
        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}