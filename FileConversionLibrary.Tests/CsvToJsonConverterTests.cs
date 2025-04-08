using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;
using Newtonsoft.Json.Linq;

namespace FileConversionLibrary.Tests
{
    public class CsvToJsonConverterTests
    {
        private CsvToJsonConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new CsvToJsonConverter();
        }

        [Test]
        public void Convert_GivenValidCsvData_ReturnsValidJson()
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
            
            // Verify the JSON can be parsed
            var jsonArray = JArray.Parse(result);
            Assert.AreEqual(2, jsonArray.Count);
            
            // Verify the structure of the first object
            var firstObj = jsonArray[0];
            Assert.AreEqual("John", firstObj["Name"].ToString());
            Assert.AreEqual(30, firstObj["Age"].Value<int>());
            
            // Verify the structure of the second object
            var secondObj = jsonArray[1];
            Assert.AreEqual("Jane", secondObj["Name"].ToString());
            Assert.AreEqual(25, secondObj["Age"].Value<int>());
        }
        
        [Test]
        public void Convert_WithConvertValuesFalse_KeepsStrings()
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
                ["convertValues"] = false
            };

            // Act
            var result = _converter.Convert(csvData, options);

            // Assert
            var jsonObj = JArray.Parse(result)[0];
            Assert.AreEqual("30", jsonObj["Age"].ToString());  // Should remain a string
        }
        
        [Test]
        public void Convert_WithDifferentDataTypes_ConvertsCorrectly()
        {
            // Arrange
            var csvData = new CsvData
            {
                Headers = new[] { "String", "Integer", "Decimal", "Boolean" },
                Rows = new List<string[]>
                {
                    new[] { "text", "42", "3.14", "true" }
                }
            };

            // Act
            var result = _converter.Convert(csvData);

            // Assert
            var jsonObj = JArray.Parse(result)[0];
            Assert.AreEqual("text", jsonObj["String"].ToString());
            Assert.AreEqual(42, jsonObj["Integer"].Value<int>());
            Assert.AreEqual(3.14, jsonObj["Decimal"].Value<double>());
            Assert.IsTrue(jsonObj["Boolean"].Value<bool>());
        }
        
        [Test]
        public void Convert_WithNullInput_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _converter.Convert(null));
        }
    }
}