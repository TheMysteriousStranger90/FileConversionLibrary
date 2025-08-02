using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FileConversionLibrary.Converters;

public class CsvToPdfConverter : IConverter<CsvData, byte[]>
{
    public byte[] Convert(CsvData input, object? options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid CSV data");
        }

        using (var memoryStream = new MemoryStream())
        {
            var document = new Document(PageSize.A4, 10, 10, 10, 10);
            var pdfWriter = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            var table = new PdfPTable(input.Headers.Length)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            
            foreach (var header in input.Headers)
            {
                var cell = new PdfPCell(new Phrase(header))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(cell);
            }
            
            foreach (var row in input.Rows)
            {
                foreach (var cell in row)
                {
                    table.AddCell(new PdfPCell(new Phrase(cell))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                }
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
    }
}