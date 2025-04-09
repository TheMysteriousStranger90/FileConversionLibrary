using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests
{
    [TestFixture]
    public class XmlToWordConverterTests
    {
        private XmlToWordConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new XmlToWordConverter();
        }

        [Test]
        public void Convert_GivenValidXmlData_ReturnsWordBytes()
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
            Assert.IsInstanceOf<byte[]>(result);
            Assert.Greater(result.Length, 0);
            
            // Check for DOCX file signature (PK ZIP signature)
            byte[] docxSignature = { 0x50, 0x4B, 0x03, 0x04 };
            for (int i = 0; i < docxSignature.Length; i++)
            {
                Assert.AreEqual(docxSignature[i], result[i]);
            }
        }

        [Test]
        public void Convert_WithoutTable_ReturnsWordDocument()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John</Name>
                                                <Age>30</Age>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" }
                }
            };

            var options = new Dictionary<string, object>
            {
                ["useTable"] = false
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void Convert_WithHierarchicalFormat_ReturnsWordDocument()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John</Name>
                                                <Details>
                                                    <Age>30</Age>
                                                </Details>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Name", "Details.Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" }
                }
            };

            var options = new Dictionary<string, object>
            {
                ["formatAsHierarchy"] = true
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void Convert_WithCustomFontSettings_ReturnsWordDocument()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John</Name>
                                                <Age>30</Age>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John", "30" }
                }
            };

            var options = new Dictionary<string, object>
            {
                ["fontFamily"] = "Arial",
                ["fontSize"] = 14
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}