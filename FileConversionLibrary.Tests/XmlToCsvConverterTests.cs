using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests
{
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
        public void Convert_GivenValidXmlData_ReturnsValidCsv()
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
            Assert.IsTrue(result.Contains("Name,Age"));
            Assert.IsTrue(result.Contains("John,30"));
            Assert.IsTrue(result.Contains("Jane,25"));
        }

        [Test]
        public void Convert_WithSemicolonDelimiter_UsesSemicolon()
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
                ["delimiter"] = ';'
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsTrue(result.Contains("Name;Age"));
            Assert.IsTrue(result.Contains("John;30"));
        }

        [Test]
        public void Convert_WithQuoteValues_AddsQuotes()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John, Doe</Name>
                                                <Age>30</Age>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "John, Doe", "30" }
                }
            };

            var options = new Dictionary<string, object>
            {
                ["quoteValues"] = true
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsTrue(result.Contains("\"John, Doe\""));
        }

        [Test]
        public void Convert_WithDataContainingSpecialChars_EscapesCorrectly()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Notes>Line 1
Line 2""quoted""</Notes>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Notes" },
                Rows = new List<string[]>
                {
                    new[] { "Line 1\nLine 2\"quoted\"" }
                }
            };

            var options = new Dictionary<string, object>
            {
                ["quoteValues"] = true
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsTrue(result.Contains("\"Line 1\nLine 2\"\"quoted\"\"\""));
        }

        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}