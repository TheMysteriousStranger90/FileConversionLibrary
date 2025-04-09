using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using iTextSharp.text;

namespace FileConversionLibrary.Tests
{
    [TestFixture]
    public class XmlToPdfConverterTests
    {
        private XmlToPdfConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new XmlToPdfConverter();
        }

        [Test]
        public void Convert_GivenValidXmlData_ReturnsPdfBytes()
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
            
            // Verify PDF header signature
            byte[] pdfSignature = { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            for (int i = 0; i < pdfSignature.Length; i++)
            {
                Assert.AreEqual(pdfSignature[i], result[i]);
            }
        }

        [Test]
        public void Convert_WithCustomFontSize_ReturnsPdf()
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
                ["fontSize"] = 12f
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void Convert_WithHierarchicalView_ReturnsPdf()
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
                ["hierarchicalView"] = true
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void Convert_WithAlternateRowColors_ReturnsPdf()
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

            var options = new Dictionary<string, object>
            {
                ["alternateRowColors"] = true,
                ["headerBackgroundColor"] = new BaseColor(200, 200, 200)
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