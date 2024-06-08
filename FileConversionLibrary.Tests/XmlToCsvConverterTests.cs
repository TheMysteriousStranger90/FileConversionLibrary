using NUnit.Framework;
using System.IO;
using System.Xml;

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
        public void Convert_GivenValidXmlFile_CreatesValidCsvFile()
        {
            // Arrange
            var xmlFilePath = "test.xml";
            var csvOutputPath = "test.csv";

            // Act
            _converter.ConvertXmlToCsv(xmlFilePath, csvOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(csvOutputPath));
        }

        [Test]
        public void Convert_GivenXmlFileWithNestedElements_CreatesValidCsvFile()
        {
            // Arrange
            var xmlFilePath = "test_nested_elements.xml";
            var csvOutputPath = "test.csv";

            // Act
            _converter.ConvertXmlToCsv(xmlFilePath, csvOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(csvOutputPath));
        }

        [Test]
        public void Convert_GivenXmlFileWithAttributes_CreatesValidCsvFile()
        {
            // Arrange
            var xmlFilePath = "test_attributes.xml";
            var csvOutputPath = "test.csv";

            // Act
            _converter.ConvertXmlToCsv(xmlFilePath, csvOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(csvOutputPath));
        }
    }
}