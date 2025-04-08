using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests
{
    public class CsvToXmlConverterTests
    {
        private CsvToXmlConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new CsvToXmlConverter();
        }

        [Test]
        public void Convert_GivenValidCsvData_ReturnsValidXml()
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
            
            // Verify the XML can be parsed
            var doc = XDocument.Parse(result);
            Assert.IsNotNull(doc.Root);
            
            // Verify structure
            var rows = doc.Root.Elements();
            Assert.AreEqual(2, rows.Count());
            
            // Check first row content
            var firstRow = rows.First();
            Assert.AreEqual("30", firstRow.Element("Age").Value);
            Assert.AreEqual("John", firstRow.Element("Name").Value);
        }
        
        [Test]
        public void Convert_WithElementsFormat_CreatesCorrectStructure()
        {
            // Arrange
            var csvData = new CsvData
            {
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" }
                }
            };
            
            var options = new Dictionary<string, object>
            {
                ["format"] = CsvToXmlConverter.XmlOutputFormat.Elements,
                ["useCData"] = false
            };

            // Act
            var result = _converter.Convert(csvData, options);

            // Assert
            var doc = XDocument.Parse(result);
            var firstRow = doc.Root.Elements().First();
            
            // Verify elements format (nested elements)
            Assert.IsNull(firstRow.Attribute("Name"));
            Assert.IsNotNull(firstRow.Element("Name"));
        }
        
        [Test]
        public void Convert_WithAttributesFormat_CreatesCorrectStructure()
        {
            // Arrange
            var csvData = new CsvData
            {
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" }
                }
            };
            
            var options = new Dictionary<string, object>
            {
                ["format"] = CsvToXmlConverter.XmlOutputFormat.Attributes
            };

            // Act
            var result = _converter.Convert(csvData, options);

            // Assert
            var doc = XDocument.Parse(result);
            var firstRow = doc.Root.Elements().First();
            
            // Verify attributes format
            Assert.IsNotNull(firstRow.Attribute("Name"));
            Assert.AreEqual("John", firstRow.Attribute("Name").Value);
        }
        
        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}