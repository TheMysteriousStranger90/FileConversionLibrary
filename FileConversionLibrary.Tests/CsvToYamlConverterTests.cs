using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using YamlDotNet.RepresentationModel;

namespace FileConversionLibrary.Tests
{
    public class CsvToYamlConverterTests
    {
        private CsvToYamlConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new CsvToYamlConverter();
        }

        [Test]
        public void Convert_GivenValidCsvData_ReturnsValidYaml()
        {
            // Arrange
            var csvData = new CsvData
            {
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" },
                    new[] { "Jane", "25" }
                }
            };

            // Act
            var result = _converter.Convert(csvData);

            // Assert
            Assert.IsNotNull(result);
            
            // Verify the YAML can be parsed
            using var stringReader = new StringReader(result);
            var yamlStream = new YamlStream();
            yamlStream.Load(stringReader);
            
            var document = yamlStream.Documents[0];
            var rootNode = document.RootNode as YamlSequenceNode;
            
            Assert.IsNotNull(rootNode);
            Assert.AreEqual(2, rootNode.Children.Count);
            
            // Verify first item structure
            var firstItem = rootNode.Children[0] as YamlMappingNode;
            Assert.IsTrue(firstItem.Children.ContainsKey(new YamlScalarNode("Name")));
            Assert.AreEqual("John", 
                ((YamlScalarNode)firstItem.Children[new YamlScalarNode("Name")]).Value);
        }
        
        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}