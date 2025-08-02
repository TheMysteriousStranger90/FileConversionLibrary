using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FileConversionLibrary.Converters;

public class CsvToWordConverter : IConverter<CsvData, byte[]>
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

        bool useTable = true;
        bool addHeaderRow = true;
        string fontFamily = "Calibri";
        int fontSize = 11;
        bool formatAsHierarchy = false;
        string title = "CSV Data Export";
        bool includeTimestamp = true;
        bool includeRowNumbers = false;
        bool alternateRowColors = false;
        bool addBorders = true;
        bool autoFitTable = true;
        string headerStyle = "strong";
        int titleFontSize = 16;
        bool includeStatistics = true;
        string pageOrientation = "portrait";
        double tableWidth = 100.0;
        bool wrapText = true;

        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("useTable", out var table) && table is bool tableValue)
            {
                useTable = tableValue;
            }

            if (optionsDict.TryGetValue("addHeaderRow", out var header) && header is bool headerValue)
            {
                addHeaderRow = headerValue;
            }

            if (optionsDict.TryGetValue("fontFamily", out var font) && font is string fontValue)
            {
                fontFamily = fontValue;
            }

            if (optionsDict.TryGetValue("fontSize", out var size) && size is int sizeValue)
            {
                fontSize = sizeValue;
            }

            if (optionsDict.TryGetValue("formatAsHierarchy", out var hierarchy) && hierarchy is bool hierarchyValue)
            {
                formatAsHierarchy = hierarchyValue;
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

            if (optionsDict.TryGetValue("alternateRowColors", out var alternate) && alternate is bool alternateValue)
            {
                alternateRowColors = alternateValue;
            }

            if (optionsDict.TryGetValue("addBorders", out var borders) && borders is bool bordersValue)
            {
                addBorders = bordersValue;
            }

            if (optionsDict.TryGetValue("autoFitTable", out var autoFit) && autoFit is bool autoFitValue)
            {
                autoFitTable = autoFitValue;
            }

            if (optionsDict.TryGetValue("headerStyle", out var hStyle) && hStyle is string hStyleValue)
            {
                headerStyle = hStyleValue;
            }

            if (optionsDict.TryGetValue("titleFontSize", out var tSize) && tSize is int tSizeValue)
            {
                titleFontSize = tSizeValue;
            }

            if (optionsDict.TryGetValue("includeStatistics", out var stats) && stats is bool statsValue)
            {
                includeStatistics = statsValue;
            }

            if (optionsDict.TryGetValue("pageOrientation", out var orientation) && orientation is string orientationValue)
            {
                pageOrientation = orientationValue;
            }

            if (optionsDict.TryGetValue("tableWidth", out var width) && width is double widthValue)
            {
                tableWidth = widthValue;
            }

            if (optionsDict.TryGetValue("wrapText", out var wrap) && wrap is bool wrapValue)
            {
                wrapText = wrapValue;
            }
        }

        using (var memoryStream = new MemoryStream())
        {
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                SetupPageSettings(body, pageOrientation);

                if (!string.IsNullOrEmpty(title))
                {
                    AddTitle(body, title, fontFamily, titleFontSize);
                }

                if (includeTimestamp)
                {
                    AddTimestamp(body, fontFamily, fontSize);
                }

                if (useTable)
                {
                    if (formatAsHierarchy)
                    {
                        AddHierarchicalContent(body, input, fontFamily, fontSize, includeRowNumbers);
                    }
                    else
                    {
                        AddTableContent(body, input, fontFamily, fontSize, addHeaderRow, 
                            includeRowNumbers, alternateRowColors, addBorders, autoFitTable, 
                            headerStyle, tableWidth, wrapText);
                    }
                }
                else
                {
                    AddParagraphContent(body, input, fontFamily, fontSize, addHeaderRow);
                }

                if (includeStatistics)
                {
                    //AddStatistics(body, input, fontFamily, fontSize);
                }

                wordDocument.Save();
            }

            return memoryStream.ToArray();
        }
    }

    private void SetupPageSettings(Body body, string orientation)
    {
        var sectionProperties = new SectionProperties();
        var pageSize = new PageSize();

        if (orientation.ToLower() == "landscape")
        {
            pageSize.Width = 15840U;
            pageSize.Height = 12240U;
            pageSize.Orient = PageOrientationValues.Landscape;
        }
        else
        {
            pageSize.Width = 12240U;
            pageSize.Height = 15840U;
            pageSize.Orient = PageOrientationValues.Portrait;
        }

        sectionProperties.Append(pageSize);
        body.Append(sectionProperties);
    }

    private void AddTitle(Body body, string title, string fontFamily, int titleFontSize)
    {
        var titleParagraph = new Paragraph();
        var titleRun = new Run();
        var titleText = new Text(title);

        var titleRunProperties = new RunProperties(
            new FontSize { Val = (titleFontSize * 2).ToString() },
            new Bold(),
            new RunFonts { Ascii = fontFamily }
        );

        titleRun.AppendChild(titleRunProperties);
        titleRun.AppendChild(titleText);
        titleParagraph.AppendChild(titleRun);

        var paragraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "240" }
        );
        titleParagraph.PrependChild(paragraphProperties);

        body.AppendChild(titleParagraph);
    }

    private void AddTimestamp(Body body, string fontFamily, int fontSize)
    {
        var timestampParagraph = new Paragraph();
        var timestampRun = new Run();
        var timestampText = new Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        var timestampRunProperties = new RunProperties(
            new FontSize { Val = (fontSize * 2).ToString() },
            new Italic(),
            new RunFonts { Ascii = fontFamily }
        );

        timestampRun.AppendChild(timestampRunProperties);
        timestampRun.AppendChild(timestampText);
        timestampParagraph.AppendChild(timestampRun);

        var paragraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "120" }
        );
        timestampParagraph.PrependChild(paragraphProperties);

        body.AppendChild(timestampParagraph);
    }

    private void AddTableContent(Body body, CsvData input, string fontFamily, int fontSize, 
        bool addHeaderRow, bool includeRowNumbers, bool alternateRowColors, bool addBorders, 
        bool autoFitTable, string headerStyle, double tableWidth, bool wrapText)
    {
        var table = new Table();

        var tableProperties = new TableProperties();

        if (addBorders)
        {
            var tableBorders = new TableBorders(
                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
            );
            tableProperties.AppendChild(tableBorders);
        }

        var tableWidth100 = new TableWidth { Width = (tableWidth * 50).ToString(), Type = TableWidthUnitValues.Pct };
        tableProperties.AppendChild(tableWidth100);

        if (autoFitTable)
        {
            var tableLayout = new TableLayout { Type = TableLayoutValues.Autofit };
            tableProperties.AppendChild(tableLayout);
        }

        table.AppendChild(tableProperties);

        var headers = input.Headers.ToList();
        if (includeRowNumbers)
        {
            headers.Insert(0, "Row #");
        }

        if (addHeaderRow)
        {
            var headerRow = new TableRow();
            
            foreach (var header in headers)
            {
                var headerCell = new TableCell();
                var headerParagraph = new Paragraph();
                var headerRun = new Run();
                var headerText = new Text(header);

                var headerRunProperties = CreateHeaderRunProperties(fontFamily, fontSize, headerStyle);
                headerRun.AppendChild(headerRunProperties);
                headerRun.AppendChild(headerText);
                headerParagraph.AppendChild(headerRun);

                var cellProperties = new TableCellProperties();
                if (alternateRowColors)
                {
                    var shading = new Shading
                    {
                        Val = ShadingPatternValues.Clear,
                        Color = "auto",
                        Fill = "D9D9D9"
                    };
                    cellProperties.AppendChild(shading);
                }

                headerCell.AppendChild(cellProperties);
                headerCell.AppendChild(headerParagraph);
                headerRow.AppendChild(headerCell);
            }

            table.AppendChild(headerRow);
        }

        for (int i = 0; i < input.Rows.Count; i++)
        {
            var row = input.Rows[i];
            var tableRow = new TableRow();

            if (includeRowNumbers)
            {
                var numberCell = CreateDataCell((i + 1).ToString(), fontFamily, fontSize, 
                    alternateRowColors && i % 2 == 1, wrapText);
                tableRow.AppendChild(numberCell);
            }

            for (int j = 0; j < input.Headers.Length; j++)
            {
                var cellData = j < row.Length ? (row[j] ?? string.Empty) : string.Empty;
                var cell = CreateDataCell(cellData, fontFamily, fontSize, 
                    alternateRowColors && i % 2 == 1, wrapText);
                tableRow.AppendChild(cell);
            }

            table.AppendChild(tableRow);
        }

        body.AppendChild(table);
    }

    private RunProperties CreateHeaderRunProperties(string fontFamily, int fontSize, string headerStyle)
    {
        var runProperties = new RunProperties(
            new FontSize { Val = (fontSize * 2).ToString() },
            new RunFonts { Ascii = fontFamily }
        );

        switch (headerStyle.ToLower())
        {
            case "strong":
                runProperties.AppendChild(new Bold());
                break;
            case "emphasis":
                runProperties.AppendChild(new Italic());
                break;
            case "heading":
                runProperties.AppendChild(new Bold());
                runProperties.AppendChild(new FontSize { Val = ((fontSize + 2) * 2).ToString() });
                break;
        }

        return runProperties;
    }

    private TableCell CreateDataCell(string content, string fontFamily, int fontSize, 
        bool useAlternateColor, bool wrapText)
    {
        var cell = new TableCell();
        var paragraph = new Paragraph();
        var run = new Run();
        var text = new Text(content);

        var runProperties = new RunProperties(
            new FontSize { Val = (fontSize * 2).ToString() },
            new RunFonts { Ascii = fontFamily }
        );

        run.AppendChild(runProperties);
        run.AppendChild(text);
        paragraph.AppendChild(run);

        var cellProperties = new TableCellProperties();
        
        if (useAlternateColor)
        {
            var shading = new Shading
            {
                Val = ShadingPatternValues.Clear,
                Color = "auto",
                Fill = "F2F2F2"
            };
            cellProperties.AppendChild(shading);
        }

        if (!wrapText)
        {
            var noWrap = new NoWrap();
            cellProperties.AppendChild(noWrap);
        }

        cell.AppendChild(cellProperties);
        cell.AppendChild(paragraph);

        return cell;
    }

    private void AddHierarchicalContent(Body body, CsvData input, string fontFamily, 
        int fontSize, bool includeRowNumbers)
    {
        for (int i = 0; i < input.Rows.Count; i++)
        {
            var row = input.Rows[i];

            var recordTitle = new Paragraph();
            var titleRun = new Run();
            var titleText = new Text($"Record {i + 1}");

            var titleRunProperties = new RunProperties(
                new FontSize { Val = ((fontSize + 2) * 2).ToString() },
                new Bold(),
                new RunFonts { Ascii = fontFamily }
            );

            titleRun.AppendChild(titleRunProperties);
            titleRun.AppendChild(titleText);
            recordTitle.AppendChild(titleRun);

            var titleParagraphProperties = new ParagraphProperties(
                new SpacingBetweenLines { Before = "120", After = "60" }
            );
            recordTitle.PrependChild(titleParagraphProperties);

            body.AppendChild(recordTitle);

            for (int j = 0; j < input.Headers.Length; j++)
            {
                if (j >= row.Length) continue;

                var fieldParagraph = new Paragraph();
                var labelRun = new Run();
                var labelText = new Text($"{input.Headers[j]}: ");

                var labelRunProperties = new RunProperties(
                    new FontSize { Val = (fontSize * 2).ToString() },
                    new Bold(),
                    new RunFonts { Ascii = fontFamily }
                );

                labelRun.AppendChild(labelRunProperties);
                labelRun.AppendChild(labelText);
                fieldParagraph.AppendChild(labelRun);

                var valueRun = new Run();
                var valueText = new Text(row[j] ?? string.Empty);

                var valueRunProperties = new RunProperties(
                    new FontSize { Val = (fontSize * 2).ToString() },
                    new RunFonts { Ascii = fontFamily }
                );

                valueRun.AppendChild(valueRunProperties);
                valueRun.AppendChild(valueText);
                fieldParagraph.AppendChild(valueRun);

                var fieldParagraphProperties = new ParagraphProperties(
                    new Indentation { Left = "360" }
                );
                fieldParagraph.PrependChild(fieldParagraphProperties);

                body.AppendChild(fieldParagraph);
            }
        }
    }

    private void AddParagraphContent(Body body, CsvData input, string fontFamily, 
        int fontSize, bool addHeaderRow)
    {
        if (addHeaderRow)
        {
            var headerParagraph = new Paragraph();
            var headerRun = new Run();
            var headerText = new Text(string.Join(" | ", input.Headers));

            var headerRunProperties = new RunProperties(
                new FontSize { Val = (fontSize * 2).ToString() },
                new Bold(),
                new RunFonts { Ascii = fontFamily }
            );

            headerRun.AppendChild(headerRunProperties);
            headerRun.AppendChild(headerText);
            headerParagraph.AppendChild(headerRun);

            body.AppendChild(headerParagraph);
        }

        foreach (var row in input.Rows)
        {
            var rowParagraph = new Paragraph();
            var rowRun = new Run();
            var rowText = new Text(string.Join(" | ", row));

            var rowRunProperties = new RunProperties(
                new FontSize { Val = (fontSize * 2).ToString() },
                new RunFonts { Ascii = fontFamily }
            );

            rowRun.AppendChild(rowRunProperties);
            rowRun.AppendChild(rowText);
            rowParagraph.AppendChild(rowRun);

            body.AppendChild(rowParagraph);
        }
    }
/*
    private void AddStatistics(Body body, CsvData input, string fontFamily, int fontSize)
    {
        var statsParagraph = new Paragraph();
        var statsRun = new Run();
        var statsText = new Text($"\nStatistics:\nTotal rows: {input.Rows.Count}\nTotal columns: {input.Headers.Length}\nGenerated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        var statsRunProperties = new RunProperties(
            new FontSize { Val = ((fontSize - 1) * 2).ToString() },
            new Italic(),
            new RunFonts { Ascii = fontFamily }
        );

        statsRun.AppendChild(statsRunProperties);
        statsRun.AppendChild(statsText);
        statsParagraph.AppendChild(statsRun);

        var statsParagraphProperties = new ParagraphProperties(
            new SpacingBetweenLines { Before = "240" },
            new Justification { Val = JustificationValues.Right }
        );
        statsParagraph.PrependChild(statsParagraphProperties);

        body.AppendChild(statsParagraph);
    }
*/
}