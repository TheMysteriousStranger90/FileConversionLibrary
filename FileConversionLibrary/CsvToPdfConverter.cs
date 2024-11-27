using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FileConversionLibrary;

public class CsvToPdfConverter : ICsvConverter
{
    public async Task ConvertAsync(string csvFilePath, string pdfOutputPath, char delimiter = ',')
    {
        try
        {
            var csvData = await CsvHelperFile.ReadCsvAsync(csvFilePath, delimiter);
            using (var stream = new FileStream(pdfOutputPath, FileMode.Create))
            {
                var document = new Document(PageSize.A4, 10, 10, 10, 10);
                var pdfWriter = PdfWriter.GetInstance(document, stream);

                document.Open();

                var table = new PdfPTable(csvData.Headers.Length)
                {
                    WidthPercentage = 100,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                
                foreach (var header in csvData.Headers)
                {
                    var cell = new PdfPCell(new Phrase(header))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Add data rows
                foreach (var row in csvData.Rows)
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
                Console.WriteLine($"Saving PDF file");
                Console.WriteLine("PDF file saved successfully.");
                document.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}