﻿using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests
{
    public class CsvToWordConverterTests
    {
        private CsvToWordConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new CsvToWordConverter();
        }

        [Test]
        public void Convert_GivenValidCsvData_ReturnsWordBytes()
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
            Assert.IsInstanceOf<byte[]>(result);
            Assert.Greater(result.Length, 0);
            
            // Check for DOCX file signature (PK ZIP signature)
            byte[] docxSignature = { 0x50, 0x4B, 0x03, 0x04 };
            Assert.IsTrue(StartsWithSequence(result, docxSignature));
        }
        
        [Test]
        public void Convert_WithCustomOptions_ReturnsWordBytes()
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
                ["useTable"] = false,
                ["fontFamily"] = "Arial"
            };

            // Act
            var result = _converter.Convert(csvData, options);

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
        
        // Helper method to check if a byte array starts with a sequence
        private bool StartsWithSequence(byte[] data, byte[] sequence)
        {
            if (data.Length < sequence.Length)
                return false;
                
            for (int i = 0; i < sequence.Length; i++)
            {
                if (data[i] != sequence[i])
                    return false;
            }
            
            return true;
        }
    }
}