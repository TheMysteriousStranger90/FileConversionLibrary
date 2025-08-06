using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FileConversionLibrary.Converters;

public class XmlToPdfConverter : IConverter<XmlData, byte[]>
{
    public byte[] Convert(XmlData input, object? options = null)
    {
        if (input?.Document == null)
        {
            throw new ArgumentException("Invalid XML data");
        }

        float fontSize = 10f;
        bool addBorders = true;
        bool alternateRowColors = false;
        bool hierarchicalView = false;
        BaseColor headerBackgroundColor = BaseColor.LIGHT_GRAY;
        float tablePadding = 5f;
        float[]? columnWidths = null;
        bool includeCData = true;
        bool includeComments = false;

        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("fontSize", out var size) && size is float sizeValue)
            {
                fontSize = sizeValue;
            }

            if (optionsDict.TryGetValue("addBorders", out var borders) && borders is bool bordersValue)
            {
                addBorders = bordersValue;
            }

            if (optionsDict.TryGetValue("alternateRowColors", out var alternate) && alternate is bool alternateValue)
            {
                alternateRowColors = alternateValue;
            }

            if (optionsDict.TryGetValue("headerBackgroundColor", out var headerColor) && headerColor is BaseColor color)
            {
                headerBackgroundColor = color;
            }

            if (optionsDict.TryGetValue("hierarchicalView", out var hierarchical) &&
                hierarchical is bool hierarchicalValue)
            {
                hierarchicalView = hierarchicalValue;
            }

            if (optionsDict.TryGetValue("tablePadding", out var padding) && padding is float paddingValue)
            {
                tablePadding = paddingValue;
            }

            if (optionsDict.TryGetValue("columnWidths", out var widths) && widths is float[] widthsArray)
            {
                columnWidths = widthsArray;
            }
            
            if (optionsDict.TryGetValue("includeCData", out var cdata) && cdata is bool cdataValue)
            {
                includeCData = cdataValue;
            }
            
            if (optionsDict.TryGetValue("includeComments", out var comments) && comments is bool commentsValue)
            {
                includeComments = commentsValue;
            }
        }

        using (var memoryStream = new MemoryStream())
        {
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_7);
            writer.SetFullCompression();

            document.Open();

            document.AddTitle("XML Data Export");
            document.AddCreationDate();
            document.AddCreator("FileConversionLibrary");

            var font = FontFactory.GetFont(FontFactory.HELVETICA, fontSize);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, fontSize);
            var cdataFont = FontFactory.GetFont(FontFactory.COURIER, fontSize - 0.5f);
            var commentFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, fontSize - 0.5f);

            if (hierarchicalView && input.Document?.Root != null)
            {
                var title = new Paragraph("XML Document Structure",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, fontSize + 4));
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                AddHierarchicalContent(document, input.Document.Root, font, cdataFont, commentFont, 
                    includeCData, includeComments);
            }
            else if (input.Headers != null && input.Rows != null && input.Headers.Length > 0)
            {
                var table = new PdfPTable(input.Headers.Length);
                table.WidthPercentage = 100;
                table.DefaultCell.Padding = tablePadding;

                if (columnWidths != null && columnWidths.Length == input.Headers.Length)
                {
                    table.SetWidths(columnWidths);
                }

                if (!addBorders)
                {
                    table.DefaultCell.Border = Rectangle.NO_BORDER;
                }

                foreach (var header in input.Headers)
                {
                    var cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = headerBackgroundColor;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.Padding = tablePadding;

                    if (!addBorders)
                    {
                        cell.Border = Rectangle.NO_BORDER;
                    }

                    table.AddCell(cell);
                }

                for (int i = 0; i < input.Rows.Count; i++)
                {
                    var row = input.Rows[i];

                    BaseColor? rowBackground = null;
                    if (alternateRowColors && i % 2 == 1)
                    {
                        rowBackground = new BaseColor(240, 240, 240);
                    }

                    for (int j = 0; j < row.Length && j < input.Headers.Length; j++)
                    {
                        var cellText = row[j] ?? string.Empty;
                        var cell = new PdfPCell(new Phrase(cellText, font));

                        if (rowBackground != null)
                        {
                            cell.BackgroundColor = rowBackground;
                        }

                        if (!addBorders)
                        {
                            cell.Border = Rectangle.NO_BORDER;
                        }

                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.Padding = tablePadding;

                        table.AddCell(cell);
                    }
                }

                document.Add(table);
            }
            else
            {
                document.Add(new Paragraph("XML Structure (Hierarchical View):", headerFont));
                document.Add(new Paragraph(" "));
                
                if (input.Document?.Root != null)
                {
                    AddHierarchicalContent(document, input.Document.Root, font, cdataFont, commentFont, 
                        includeCData, includeComments);
                }
                else
                {
                    document.Add(new Paragraph("No valid XML structure found.", font));
                }
            }

            document.Close();

            return memoryStream.ToArray();
        }
    }

    private void AddHierarchicalContent(Document document, XElement element, Font font, Font cdataFont, 
        Font commentFont, bool includeCData, bool includeComments, int level = 0)
    {
        string indent = new string(' ', level * 4);

        var elementFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, font.Size);
        var elementNameParagraph = new Paragraph($"{indent}{element.Name.LocalName}", elementFont);
        elementNameParagraph.SpacingBefore = level == 0 ? 0 : 5f;
        document.Add(elementNameParagraph);

        if (element.Attributes().Any())
        {
            var attrFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, font.Size - 0.5f);
            foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
            {
                document.Add(new Paragraph($"{indent}  @{attr.Name.LocalName}: {attr.Value}", attrFont));
            }
        }

        if (includeComments)
        {
            var comments = element.Nodes().OfType<XComment>().ToList();
            foreach (var comment in comments)
            {
                document.Add(new Paragraph($"{indent}  <!-- {comment.Value} -->", commentFont));
            }
        }

        var textValue = element.Nodes().OfType<XText>()
            .Where(t => !string.IsNullOrWhiteSpace(t.Value))
            .Select(t => t.Value.Trim())
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(textValue))
        {
            if (textValue.Length > 100)
            {
                textValue = textValue.Substring(0, 97) + "...";
            }

            document.Add(new Paragraph($"{indent}  Value: {textValue}", font));
        }

        if (includeCData)
        {
            var cdataValue = element.Nodes().OfType<XCData>()
                .Select(c => c.Value.Trim())
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(cdataValue))
            {
                var cdataText = $"{indent}  CDATA:";
                document.Add(new Paragraph(cdataText, font));
                
                var cdataLines = cdataValue.Split('\n');
                foreach (var line in cdataLines)
                {
                    var trimmedLine = line.TrimStart();
                    document.Add(new Paragraph($"{indent}    {trimmedLine}", cdataFont));
                }
            }
        }

        foreach (var child in element.Elements())
        {
            AddHierarchicalContent(document, child, font, cdataFont, commentFont, 
                includeCData, includeComments, level + 1);
        }
    }
}