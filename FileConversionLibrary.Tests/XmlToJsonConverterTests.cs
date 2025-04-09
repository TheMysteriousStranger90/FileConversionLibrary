using System.Xml.Linq;
using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using Newtonsoft.Json.Linq;

namespace FileConversionLibrary.Tests
{
    [TestFixture]
    public class XmlToJsonConverterTests
    {
        private XmlToJsonConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new XmlToJsonConverter();
        }

        [Test]
        public void Convert_GivenValidXmlData_ReturnsValidJson()
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
            
            // Verify the JSON can be parsed
            var jsonObj = JObject.Parse(result);
            Assert.IsNotNull(jsonObj["root"]);
            
            var elements = jsonObj["root"]["element"];
            Assert.AreEqual(2, elements.Count());
            
            Assert.AreEqual("John", elements[0]["Name"].ToString());
            Assert.AreEqual("30", elements[0]["Age"].ToString());
        }

        [Test]
        public void Convert_WithNestedElements_PreservesStructure()
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
                Document = xmlDoc
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            var jsonObj = JObject.Parse(result);
            var details = jsonObj["root"]["element"]["Details"];
            Assert.IsNotNull(details);
            Assert.AreEqual("30", details["Age"].ToString());
        }

        [Test]
        public void Convert_WithAttributes_IncludesAttributes()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element Name='John' Age='30' />
                                            <element Name='Jane' Age='25' />
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc
            };

            // Act
            var result = _converter.Convert(xmlData);

            // Assert
            var jsonObj = JObject.Parse(result);
            var elements = jsonObj["root"]["element"];
            
            Assert.AreEqual("John", elements[0]["@Name"].ToString());
            Assert.AreEqual("30", elements[0]["@Age"].ToString());
        }

        [Test]
        public void Convert_WithConvertValuesOption_ConvertsTypes()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John</Name>
                                                <Age>30</Age>
                                                <Active>true</Active>
                                                <Score>9.5</Score>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc
            };

            var options = new Dictionary<string, object>
            {
                ["convertValues"] = true
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            var jsonObj = JObject.Parse(result);
            var element = jsonObj["root"]["element"];
            
            // Check that Age is a number, not string
            Assert.IsInstanceOf<JValue>(element["Age"]);
            Assert.AreEqual(JTokenType.Integer, element["Age"].Type);
            Assert.AreEqual(30, element["Age"].Value<int>());
            
            // Check that Active is a boolean
            Assert.IsInstanceOf<JValue>(element["Active"]);
            Assert.AreEqual(JTokenType.Boolean, element["Active"].Type);
            Assert.AreEqual(true, element["Active"].Value<bool>());
            
            // Check that Score is a decimal
            Assert.IsInstanceOf<JValue>(element["Score"]);
            Assert.AreEqual(JTokenType.Float, element["Score"].Type);
            Assert.AreEqual(9.5, element["Score"].Value<double>());
        }
        
        [Test]
        public void Convert_WithRemoveWhitespaceOption_RemovesEmptyTextNodes()
        {
            // Arrange
            var xmlDoc = XDocument.Parse(@"<root>
                                            <element>
                                                <Name>John</Name>
                                                <Description>
                                                </Description>
                                            </element>
                                           </root>");
            
            var xmlData = new XmlData
            {
                Document = xmlDoc
            };

            var options = new Dictionary<string, object>
            {
                ["removeWhitespace"] = true
            };

            // Act
            var result = _converter.Convert(xmlData, options);

            // Assert
            var jsonObj = JObject.Parse(result);
            var element = jsonObj["root"]["element"];
            
            // Description should still exist but not have #text node
            Assert.IsNotNull(element["Description"]);
            Assert.IsFalse(element["Description"].ToString().Contains("#text"));
        }

        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}