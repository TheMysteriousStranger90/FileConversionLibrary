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
                Document = xmlDoc
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            Assert.IsNotNull(result);
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            Assert.IsTrue(lines[0].Contains("Age") && lines[0].Contains("Name"));
            
            Assert.IsTrue(result.Contains("John") && result.Contains("30"));
            Assert.IsTrue(result.Contains("Jane") && result.Contains("25"));
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
                Document = xmlDoc
            };

            var options = new Dictionary<string, object>
            {
                ["delimiter"] = ';'
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            Assert.IsTrue(result.Contains("Age;Name"));
            Assert.IsTrue(result.Contains("30;John"));
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
                Document = xmlDoc
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
                Document = xmlDoc
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

        [Test]
        public void Convert_WithEmptyDocument_ThrowsArgumentException()
        {
            // Arrange
            var xmlData = new XmlData
            {
                Document = new XDocument()
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(xmlData));
        }

        [Test]
        public void Convert_WithTabularDataOnly_UsesTabularData()
        {
            // Arrange
            var xmlData = new XmlData
            {
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
        public void Convert_WithBothDocumentAndTabularData_PrioritizesDocument()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <person>
                                                <FullName>John Doe</FullName>
                                                <Years>30</Years>
                                            </person>
                                           </root>");

            var xmlData = new XmlData
            {
                Document = xmlDoc,
                Headers = new[] { "Name", "Age" },
                Rows = new List<string[]>
                {
                    new[] { "Jane", "25" }
                }
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("FullName") && result.Contains("Years"));
            Assert.IsTrue(result.Contains("John Doe") && result.Contains("30"));
            Assert.IsFalse(result.Contains("Jane") || result.Contains("25"));
        }

        [Test]
        public void Convert_WithAttributes_IncludesAttributes()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <person id=""1"" status=""active"">
                                                <Name>John</Name>
                                                <Age>30</Age>
                                            </person>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("@id") && result.Contains("@status"));
            Assert.IsTrue(result.Contains("1") && result.Contains("active"));
        }

        [Test]
        public void Convert_WithFlattenHierarchyDisabled_DoesNotFlatten()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <person>
                                                <profile>
                                                    <name>John</name>
                                                    <age>30</age>
                                                </profile>
                                            </person>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc
            };

            var options = new Dictionary<string, object>
            {
                ["flattenHierarchy"] = false
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("profile"));
        }

        public void Convert_WithCustomNullValue_UsesCustomNull()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                    <person>
                                        <Name>John</Name>
                                        <MiddleName></MiddleName>
                                        <Age>30</Age>
                                    </person>
                                    <person>
                                        <Name>Jane</Name>
                                        <Age>25</Age>
                                    </person>
                                   </root>");
    
            var xmlData = new XmlData
            {
                Document = xmlDoc
            };

            var options = new Dictionary<string, object>
            {
                ["customNullValue"] = "N/A"
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            Console.WriteLine("Actual result:");
            Console.WriteLine(result);
    
            Assert.IsNotNull(result);
            
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool foundCustomNull = lines.Any(line => line.Contains("N/A"));
    
            Assert.IsTrue(foundCustomNull, $"Expected to find 'N/A' in result. Actual result:\n{result}");
        }
    }
}