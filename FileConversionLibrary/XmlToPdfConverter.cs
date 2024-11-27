using System.Xml;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FileConversionLibrary;

public class XmlToPdfConverter : IXmlConverter
{
    public async Task ConvertAsync(string xmlFilePath, string pdfOutputPath)
    {
        try
        {
            var (headers, rows) = await XmlHelperFile.ReadXmlAsync(xmlFilePath);

            using (var stream = new FileStream(pdfOutputPath, FileMode.Create))
            {
                var document = new Document();
                var pdfWriter = PdfWriter.GetInstance(document, stream);

                document.Open();

                var table = new PdfPTable(headers.Length);
                foreach (var header in headers)
                {
                    table.AddCell(header);
                }

                foreach (var row in rows)
                {
                    foreach (var cell in row)
                    {
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();
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