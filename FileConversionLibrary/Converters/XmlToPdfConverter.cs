using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FileConversionLibrary.Converters;

public class XmlToPdfConverter : IConverter<XmlData, byte[]>
{
    public byte[] Convert(XmlData input, object options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid XML data");
        }

        float fontSize = 10f;
        bool addBorders = true;
        bool alternateRowColors = false;
        bool hierarchicalView = false;
        BaseColor headerBackgroundColor = BaseColor.LIGHT_GRAY;
        float tablePadding = 5f;
        float[] columnWidths = null;

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

            if (hierarchicalView && input.Document?.Root != null)
            {
                var title = new Paragraph("XML Document Structure",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, fontSize + 4));
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                AddHierarchicalContent(document, input.Document.Root, font);
            }
            else
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

                    BaseColor rowBackground = null;
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

            document.Close();

            return memoryStream.ToArray();
        }
    }

    private void AddHierarchicalContent(Document document, XElement element, Font font, int level = 0)
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

        var cdataValue = element.Nodes().OfType<XCData>()
            .Select(c => c.Value.Trim())
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(cdataValue))
        {
            var cdataFont = FontFactory.GetFont(FontFactory.COURIER, font.Size - 0.5f);
            document.Add(new Paragraph($"{indent}  CDATA: {cdataValue}", cdataFont));
        }

        foreach (var child in element.Elements())
        {
            AddHierarchicalContent(document, child, font, level + 1);
        }
    }
}