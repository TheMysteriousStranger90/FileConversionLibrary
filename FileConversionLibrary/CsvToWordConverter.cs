using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FileConversionLibrary;

public class CsvToWordConverter
{
    public void ConvertCsvToWord(string csvFilePath, string wordOutputPath, char delimiter = ',')
    {
        try
        {
            var csvContent = File.ReadAllLines(csvFilePath);
            var headers = csvContent[0].Split(delimiter);

            using (var wordDocument =
                   WordprocessingDocument.Create(wordOutputPath, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var headerParagraph = body.AppendChild(new Paragraph());
                var headerRun = headerParagraph.AppendChild(new Run());
                headerRun.AppendChild(new Text(string.Join(" ", headers)));

                for (var i = 1; i < csvContent.Length; i++)
                {
                    var row = csvContent[i].Split(delimiter);
                    var paragraph = body.AppendChild(new Paragraph());
                    var run = paragraph.AppendChild(new Run());
                    run.AppendChild(new Text(string.Join(" ", row)));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}