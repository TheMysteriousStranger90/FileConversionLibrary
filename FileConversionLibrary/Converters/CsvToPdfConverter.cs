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

        if (input.Headers.Length == 0)
        {
            throw new ArgumentException("No headers found in CSV data");
        }

        float fontSize = 10f;
        bool addBorders = true;
        bool alternateRowColors = false;
        BaseColor headerBackgroundColor = BaseColor.LIGHT_GRAY;
        BaseColor alternateRowColor = new BaseColor(245, 245, 245);
        float tablePadding = 5f;
        float[]? columnWidths = null;
        bool autoFitColumns = true;
        string title = "CSV Data Export";
        bool includeTimestamp = true;
        bool includeRowNumbers = false;
        Rectangle pageSize = PageSize.A4;
        bool landscapeOrientation = false;
        string fontFamily = BaseFont.HELVETICA;

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

            if (optionsDict.TryGetValue("headerBackgroundColor", out var headerColor) &&
                headerColor is BaseColor headerColorValue)
            {
                headerBackgroundColor = headerColorValue;
            }

            if (optionsDict.TryGetValue("alternateRowColor", out var altColor) && altColor is BaseColor altColorValue)
            {
                alternateRowColor = altColorValue;
            }

            if (optionsDict.TryGetValue("tablePadding", out var padding) && padding is float paddingValue)
            {
                tablePadding = paddingValue;
            }

            if (optionsDict.TryGetValue("columnWidths", out var widths) && widths is float[] widthsArray)
            {
                columnWidths = widthsArray;
                autoFitColumns = false;
            }

            if (optionsDict.TryGetValue("autoFitColumns", out var autoFit) && autoFit is bool autoFitValue)
            {
                autoFitColumns = autoFitValue;
            }

            if (optionsDict.TryGetValue("title", out var titleObj) && titleObj is string titleValue)
            {
                title = titleValue;
            }

            if (optionsDict.TryGetValue("includeTimestamp", out var timestamp) && timestamp is bool timestampValue)
            {
                includeTimestamp = timestampValue;
            }

            if (optionsDict.TryGetValue("includeRowNumbers", out var rowNums) && rowNums is bool rowNumsValue)
            {
                includeRowNumbers = rowNumsValue;
            }

            if (optionsDict.TryGetValue("landscapeOrientation", out var landscape) && landscape is bool landscapeValue)
            {
                landscapeOrientation = landscapeValue;
            }

            if (optionsDict.TryGetValue("fontFamily", out var font) && font is string fontValue)
            {
                fontFamily = fontValue;
            }
        }

        using (var memoryStream = new MemoryStream())
        {
            Rectangle pageSizeRect = landscapeOrientation ? pageSize.Rotate() : pageSize;
            var document = new Document(pageSizeRect, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_7);
            writer.SetFullCompression();

            document.Open();

            document.AddTitle(title);
            document.AddCreationDate();
            document.AddCreator("FileConversionLibrary");
            document.AddSubject("CSV to PDF Conversion");

            var baseFont = BaseFont.CreateFont(fontFamily, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var headerFont = new Font(baseFont, fontSize + 1, Font.BOLD);
            var cellFont = new Font(baseFont, fontSize);
            var titleFont = new Font(baseFont, fontSize + 6, Font.BOLD);

            if (!string.IsNullOrEmpty(title))
            {
                var titleParagraph = new Paragraph(title, titleFont);
                titleParagraph.Alignment = Element.ALIGN_CENTER;
                titleParagraph.SpacingAfter = 20f;
                document.Add(titleParagraph);
            }

            if (includeTimestamp)
            {
                var timestampParagraph = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", cellFont);
                timestampParagraph.Alignment = Element.ALIGN_CENTER;
                timestampParagraph.SpacingAfter = 15f;
                document.Add(timestampParagraph);
            }

            var headers = input.Headers.ToList();
            if (includeRowNumbers)
            {
                headers.Insert(0, "Row #");
            }

            var table = new PdfPTable(headers.Count);
            table.WidthPercentage = 100;
            table.DefaultCell.Padding = tablePadding;

            if (columnWidths != null && columnWidths.Length == headers.Count)
            {
                table.SetWidths(columnWidths);
            }
            else if (autoFitColumns)
            {
                var calculatedWidths = CalculateColumnWidths(input, includeRowNumbers);
                table.SetWidths(calculatedWidths);
            }

            if (!addBorders)
            {
                table.DefaultCell.Border = Rectangle.NO_BORDER;
            }

            foreach (var header in headers)
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
                    rowBackground = alternateRowColor;
                }

                if (includeRowNumbers)
                {
                    var rowNumberCell = new PdfPCell(new Phrase((i + 1).ToString(), cellFont));
                    if (rowBackground != null)
                    {
                        rowNumberCell.BackgroundColor = rowBackground;
                    }

                    rowNumberCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    rowNumberCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    rowNumberCell.Padding = tablePadding;

                    if (!addBorders)
                    {
                        rowNumberCell.Border = Rectangle.NO_BORDER;
                    }

                    table.AddCell(rowNumberCell);
                }

                for (int j = 0; j < input.Headers.Length; j++)
                {
                    var cellText = j < row.Length ? (row[j] ?? string.Empty) : string.Empty;

                    if (cellText.Length > 200)
                    {
                        cellText = cellText.Substring(0, 197) + "...";
                    }

                    var cell = new PdfPCell(new Phrase(cellText, cellFont));

                    if (rowBackground != null)
                    {
                        cell.BackgroundColor = rowBackground;
                    }

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.Padding = tablePadding;

                    if (!addBorders)
                    {
                        cell.Border = Rectangle.NO_BORDER;
                    }

                    table.AddCell(cell);
                }
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
    }

    private float[] CalculateColumnWidths(CsvData input, bool includeRowNumbers)
    {
        var columnCount = input.Headers.Length + (includeRowNumbers ? 1 : 0);
        var widths = new float[columnCount];

        int startIndex = includeRowNumbers ? 1 : 0;

        if (includeRowNumbers)
        {
            widths[0] = 0.8f;
        }

        for (int i = 0; i < input.Headers.Length; i++)
        {
            var header = input.Headers[i];
            int maxLength = header.Length;

            foreach (var row in input.Rows.Take(Math.Min(100, input.Rows.Count)))
            {
                if (i < row.Length && row[i] != null)
                {
                    maxLength = Math.Max(maxLength, row[i].Length);
                }
            }

            widths[startIndex + i] = Math.Max(1.0f, Math.Min(maxLength / 10.0f, 4.0f));
        }

        return widths;
    }
}