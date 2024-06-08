using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FileConversionLibrary;

public class CsvToPdfConverter
{
    public void ConvertCsvToPdf(string csvFilePath, string pdfOutputPath, char delimiter = ',')
    {
        try
        {
            var csvContent = File.ReadAllLines(csvFilePath);
            var headers = csvContent[0].Split(delimiter);

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

                for (var i = 1; i < csvContent.Length; i++)
                {
                    var row = csvContent[i].Split(delimiter);
                    foreach (var cell in row)
                    {
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}