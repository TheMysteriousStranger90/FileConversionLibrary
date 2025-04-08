using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FileConversionLibrary.Converters;

public class CsvToWordConverter : IConverter<CsvData, byte[]>
{
    public byte[] Convert(CsvData input, object options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid CSV data");
        }

        using (var memoryStream = new MemoryStream())
        {
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());
                
                var table = new Table();
                
                var tableProperties = new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                    )
                );
                table.AppendChild(tableProperties);
                
                var headerRow = new TableRow();
                foreach (var header in input.Headers)
                {
                    var headerCell = new TableCell();
                    var headerParagraph = new Paragraph(new Run(new Text(header)));
                    
                    var runProperties = new RunProperties();
                    runProperties.AppendChild(new Bold());
                    headerParagraph.GetFirstChild<Run>().PrependChild(runProperties);
                    
                    headerCell.Append(headerParagraph);
                    headerRow.Append(headerCell);
                }
                table.Append(headerRow);
                
                foreach (var dataRow in input.Rows)
                {
                    var tableRow = new TableRow();
                    foreach (var cellData in dataRow)
                    {
                        var cell = new TableCell();
                        var paragraph = new Paragraph(new Run(new Text(cellData ?? string.Empty)));
                        cell.Append(paragraph);
                        tableRow.Append(cell);
                    }
                    table.Append(tableRow);
                }

                body.Append(table);
                
                var titleParagraph = new Paragraph(
                    new Run(new Text("CSV Data Export"))
                );
                var titleRunProperties = new RunProperties(
                    new FontSize { Val = "28" },
                    new Bold()
                );
                titleParagraph.GetFirstChild<Run>().PrependChild(titleRunProperties);
                
                body.InsertBefore(titleParagraph, table);
                
                var dateParagraph = new Paragraph(
                    new Run(new Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"))
                );
                body.InsertAfter(dateParagraph, table);
                
                wordDocument.Save();
            }
            
            return memoryStream.ToArray();
        }
    }
}