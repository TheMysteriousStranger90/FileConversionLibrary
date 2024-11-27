using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary;

public class XmlToWordConverter : IXmlConverter
{
    public async Task ConvertAsync(string xmlFilePath, string wordOutputPath)
    {
        try
        {
            var (headers, rows) = await XmlHelperFile.ReadXmlAsync(xmlFilePath);

            using (var wordDocument = WordprocessingDocument.Create(wordOutputPath, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var headerParagraph = body.AppendChild(new Paragraph());
                var headerRun = headerParagraph.AppendChild(new Run());
                headerRun.AppendChild(new Text(string.Join(" ", headers)));

                foreach (var row in rows)
                {
                    var paragraph = body.AppendChild(new Paragraph());
                    var run = paragraph.AppendChild(new Run());
                    run.AppendChild(new Text(string.Join(" ", row)));
                }
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine($"File not found: {e.FileName}");
        }
        catch (XmlException e)
        {
            Console.WriteLine($"Invalid XML: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }
}