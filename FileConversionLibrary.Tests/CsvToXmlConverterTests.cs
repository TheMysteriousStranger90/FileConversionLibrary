using NUnit.Framework;
using System.IO;
using System.Xml.Linq;

namespace FileConversionLibrary.Tests
{
    [TestFixture]
    public class CsvToXmlConverterTests
    {
        private CsvToXmlConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new CsvToXmlConverter();
        }

        [Test]
        public void Convert_GivenValidCsvFile_CreatesValidXmlFile()
        {
            // Arrange
            var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
            var xmlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");

            // Act
            _converter.ConvertCsvToXml(csvFilePath, xmlOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(xmlOutputPath));
            var doc = XDocument.Load(xmlOutputPath);
            Assert.IsNotNull(doc.Root);
         }
         
         [Test]
         public void Convert_GivenCsvFileWithDifferentDelimiter_CreatesValidXmlFile()
         {
             // Arrange
             var csvFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.csv");
             var xmlOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");
 
             // Act
             _converter.ConvertCsvToXml(csvFilePath, xmlOutputPath, ';');
 
             // Assert
             Assert.IsTrue(File.Exists(xmlOutputPath));
             var doc = XDocument.Load(xmlOutputPath);
             Assert.IsNotNull(doc.Root);
         }
 
         [Test]
         public void Convert_GivenCsvFileWithQuotedFields_CreatesValidXmlFile()
         {
             // Arrange
             var csvFilePath = "test.csv";
             var xmlOutputPath = "test.xml";
 
             // Act
             _converter.ConvertCsvToXml(csvFilePath, xmlOutputPath);
 
             // Assert
             Assert.IsTrue(File.Exists(xmlOutputPath));
             var doc = XDocument.Load(xmlOutputPath);
             Assert.IsNotNull(doc.Root);
         }
     }
 }