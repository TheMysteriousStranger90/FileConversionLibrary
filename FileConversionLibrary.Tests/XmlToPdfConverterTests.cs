﻿namespace FileConversionLibrary.Tests;

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
        public async Task ConvertAsync_GivenValidXmlFile_CreatesPdfFile()
        {
            // Arrange
            var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.xml");
            var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");
            
            var xmlContent = @"<root>
                                <element>
                                    <Name>John</Name>
                                    <Age>30</Age>
                                </element>
                                <element>
                                    <Name>Jane</Name>
                                    <Age>25</Age>
                                </element>
                               </root>";
            await File.WriteAllTextAsync(xmlFilePath, xmlContent);

            // Act
            await _converter.ConvertAsync(xmlFilePath, pdfOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(pdfOutputPath));
        }

        [Test]
        public async Task ConvertAsync_GivenXmlFileWithNestedElements_CreatesPdfFile()
        {
            // Arrange
            var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_nested_elements.xml");
            var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");
            
            var xmlContent = @"<root>
                                <element>
                                    <Name>John</Name>
                                    <Details>
                                        <Age>30</Age>
                                    </Details>
                                </element>
                                <element>
                                    <Name>Jane</Name>
                                    <Details>
                                        <Age>25</Age>
                                    </Details>
                                </element>
                               </root>";
            await File.WriteAllTextAsync(xmlFilePath, xmlContent);

            // Act
            await _converter.ConvertAsync(xmlFilePath, pdfOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(pdfOutputPath));
        }

        [Test]
        public async Task ConvertAsync_GivenXmlFileWithAttributes_CreatesPdfFile()
        {
            // Arrange
            var xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_attributes.xml");
            var pdfOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.pdf");
            
            var xmlContent = @"<root>
                                <element Name='John' Age='30' />
                                <element Name='Jane' Age='25' />
                               </root>";
            await File.WriteAllTextAsync(xmlFilePath, xmlContent);

            // Act
            await _converter.ConvertAsync(xmlFilePath, pdfOutputPath);

            // Assert
            Assert.IsTrue(File.Exists(pdfOutputPath));
        }
    }