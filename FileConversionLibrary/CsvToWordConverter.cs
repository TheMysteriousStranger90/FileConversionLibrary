﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary;

public class CsvToWordConverter : ICsvConverter
{
    public async Task ConvertAsync(string csvFilePath, string wordOutputPath, char delimiter = ',')
    {
        try
        {
            var csvData = await CsvHelperFile.ReadCsvAsync(csvFilePath, delimiter);

            using (var wordDocument =
                   WordprocessingDocument.Create(wordOutputPath, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var headerParagraph = body.AppendChild(new Paragraph());
                var headerRun = headerParagraph.AppendChild(new Run());
                headerRun.AppendChild(new Text(string.Join(" ", csvData.Headers)));

                foreach (var row in csvData.Rows)
                {
                    var paragraph = body.AppendChild(new Paragraph());
                    var run = paragraph.AppendChild(new Run());
                    run.AppendChild(new Text(string.Join(" ", row)));
                }
            }
            Console.WriteLine($"Saving Word file");
            Console.WriteLine("Word file saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}