using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Converters;

public class XmlToWordConverter : IConverter<XmlData, byte[]>
{
    public byte[] Convert(XmlData input, object? options = null)
    {
        if (input?.Document == null)
        {
            throw new ArgumentException("Invalid XML data");
        }
        
        bool useTable = true;
        bool addHeaderRow = true;
        bool formatAsHierarchy = false;
        string fontFamily = "Calibri";
        int fontSize = 11;
        bool includeCData = true;
        bool includeComments = false;

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
            
            if (optionsDict.TryGetValue("includeCData", out var cdata) && cdata is bool cdataValue)
            {
                includeCData = cdataValue;
            }
            
            if (optionsDict.TryGetValue("includeComments", out var comments) && comments is bool commentsValue)
            {
                includeComments = commentsValue;
            }
        }

        using (var ms = new MemoryStream())
        {
            using (var wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                
                var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
                GenerateStyleDefinitions(stylesPart);

                var body = mainPart.Document.AppendChild(new Body());
                
                var titleParagraph = body.AppendChild(new Paragraph(
                    new ParagraphProperties(new ParagraphStyleId { Val = "Title" }),
                    new Run(
                        new Text("XML Data Export")
                    )
                ));
                
                if (formatAsHierarchy && input.Document?.Root != null)
                {
                    AddHierarchicalContent(body, input.Document.Root, fontFamily, fontSize, 
                        includeCData, includeComments);
                }
                else if (useTable && input.Headers != null && input.Rows != null && input.Headers.Length > 0)
                {
                    var table = CreateTable(input, fontFamily, fontSize, addHeaderRow);
                    body.Append(table);
                }
                else if (input.Headers != null && input.Rows != null && input.Headers.Length > 0)
                {
                    AddSimpleContent(body, input, fontFamily, fontSize);
                }
                else if (input.Document?.Root != null)
                {
                    var fallbackHeader = body.AppendChild(new Paragraph(
                        new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                        new Run(
                            new Text("XML Structure (Hierarchical View)")
                        )
                    ));
                    
                    AddHierarchicalContent(body, input.Document.Root, fontFamily, fontSize, 
                        includeCData, includeComments);
                }
                
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }
    }
    
    private Table CreateTable(XmlData input, string fontFamily, int fontSize, bool addHeaderRow)
    {
        var table = new Table(
            new TableProperties(
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                )
            )
        );
        
        if (addHeaderRow && input.Headers != null)
        {
            var headerRow = new TableRow();

            foreach (var header in input.Headers)
            {
                var cell = new TableCell(
                    new TableCellProperties(
                        new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "DDDDDD" }
                    ),
                    new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                        new Run(
                            new RunProperties(
                                new Bold(),
                                new RunFonts { Ascii = fontFamily },
                                new FontSize
                                {
                                    Val = (fontSize * 2).ToString()
                                }
                            ),
                            new Text(header ?? string.Empty)
                        )
                    )
                );
                headerRow.Append(cell);
            }

            table.Append(headerRow);
        }
        
        if (input.Rows != null)
        {
            foreach (var rowData in input.Rows)
            {
                var tableRow = new TableRow();

                for (int i = 0; i < rowData.Length && i < (input.Headers?.Length ?? 0); i++)
                {
                    var cellText = rowData[i] ?? string.Empty;
                    
                    cellText = new string(cellText.Where(c => !char.IsControl(c)).ToArray());

                    var cell = new TableCell(
                        new Paragraph(
                            new Run(
                                new RunProperties(
                                    new RunFonts { Ascii = fontFamily },
                                    new FontSize { Val = (fontSize * 2).ToString() }
                                ),
                                new Text(cellText)
                            )
                        )
                    );
                    tableRow.Append(cell);
                }

                table.Append(tableRow);
            }
        }

        return table;
    }
    
    private void AddSimpleContent(Body body, XmlData input, string fontFamily, int fontSize)
    {
        var headers = string.Join(", ", input.Headers?.Select(h => h ?? string.Empty) ?? Array.Empty<string>());

        var headerPara = body.AppendChild(new Paragraph());
        var headerRun = headerPara.AppendChild(new Run(
            new RunProperties(
                new Bold(),
                new RunFonts { Ascii = fontFamily },
                new FontSize { Val = (fontSize * 2).ToString() }
            )
        ));
        headerRun.AppendChild(new Text("Headers: " + headers));
        
        if (input.Rows != null)
        {
            foreach (var row in input.Rows)
            {
                var cleanRow = row.Select(cell => cell ?? string.Empty)
                    .Select(cell => new string(cell.Where(c => !char.IsControl(c)).ToArray()));

                var rowText = string.Join(", ", cleanRow);

                var para = body.AppendChild(new Paragraph());
                var run = para.AppendChild(new Run(
                    new RunProperties(
                        new RunFonts { Ascii = fontFamily },
                        new FontSize { Val = (fontSize * 2).ToString() }
                    )
                ));
                run.AppendChild(new Text(rowText));
            }
        }
    }
    
    private void AddHierarchicalContent(Body body, System.Xml.Linq.XElement element, string fontFamily, int fontSize,
        bool includeCData, bool includeComments, int level = 0)
    {
        string headingStyle = level == 0 ? "Heading1" : "Heading" + Math.Min(level + 1, 9);

        var elementPara = body.AppendChild(new Paragraph(
            new ParagraphProperties(new ParagraphStyleId { Val = headingStyle }),
            new Run(
                new RunProperties(
                    new RunFonts { Ascii = fontFamily },
                    new FontSize { Val = (fontSize * 2).ToString() }
                ),
                new Text(element.Name.LocalName)
            )
        ));
        
        foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            var attrPara = body.AppendChild(new Paragraph(
                new ParagraphProperties(
                    new Indentation { Left = ((level + 1) * 360).ToString() }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = fontFamily },
                        new FontSize { Val = (fontSize * 2).ToString() },
                        new Italic()
                    ),
                    new Text($"@{attr.Name.LocalName}: {attr.Value}")
                )
            ));
        }
        
        if (includeComments)
        {
            var comments = element.Nodes().OfType<System.Xml.Linq.XComment>().ToList();
            foreach (var comment in comments)
            {
                var commentPara = body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new Indentation { Left = ((level + 1) * 360).ToString() },
                        new ParagraphStyleId { Val = "Comment" }
                    ),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = fontFamily },
                            new FontSize { Val = (fontSize * 2).ToString() },
                            new Italic(),
                            new Color { Val = "808080" }
                        ),
                        new Text($"<!-- {comment.Value} -->")
                    )
                ));
            }
        }
        
        string? textContent = element.Nodes()
            .OfType<System.Xml.Linq.XText>()
            .Where(t => !string.IsNullOrWhiteSpace(t.Value))
            .Select(t => t.Value.Trim())
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(textContent))
        {
            textContent = new string(textContent.Where(c => !char.IsControl(c)).ToArray());

            var contentPara = body.AppendChild(new Paragraph(
                new ParagraphProperties(
                    new Indentation { Left = ((level + 1) * 360).ToString() }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts { Ascii = fontFamily },
                        new FontSize { Val = (fontSize * 2).ToString() }
                    ),
                    new Text($"Value: {textContent}")
                )
            ));
        }
        
        if (includeCData)
        {
            var cdataContent = element.Nodes()
                .OfType<System.Xml.Linq.XCData>()
                .Select(c => c.Value)
                .FirstOrDefault();
                
            if (!string.IsNullOrEmpty(cdataContent))
            {
                var cdataHeaderPara = body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new Indentation { Left = ((level + 1) * 360).ToString() }
                    ),
                    new Run(
                        new RunProperties(
                            new RunFonts { Ascii = fontFamily },
                            new FontSize { Val = (fontSize * 2).ToString() },
                            new Bold()
                        ),
                        new Text("CDATA:")
                    )
                ));
                
                var cdataLines = cdataContent.Split('\n');
                foreach (var line in cdataLines)
                {
                    var trimmedLine = line.TrimStart();
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        var cdataLinePara = body.AppendChild(new Paragraph(
                            new ParagraphProperties(
                                new Indentation { Left = ((level + 1) * 480).ToString() },
                                new ParagraphStyleId { Val = "Code" }
                            ),
                            new Run(
                                new RunProperties(
                                    new RunFonts { Ascii = "Courier New" },
                                    new FontSize { Val = (fontSize * 2).ToString() }
                                ),
                                new Text(trimmedLine)
                            )
                        ));
                    }
                }
            }
        }
        
        foreach (var child in element.Elements())
        {
            AddHierarchicalContent(body, child, fontFamily, fontSize, includeCData, includeComments, level + 1);
        }
    }

    private void GenerateStyleDefinitions(StyleDefinitionsPart stylesPart)
    {
        var styles = new Styles();
        
        var normalStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true
        };
        normalStyle.Append(new StyleName { Val = "Normal" });
        normalStyle.Append(new PrimaryStyle());

        var normalRunProps = new StyleRunProperties();
        normalRunProps.Append(new RunFonts { Ascii = "Calibri" });
        normalRunProps.Append(new FontSize { Val = "22" });
        normalStyle.Append(normalRunProps);

        styles.Append(normalStyle);
        
        var titleStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Title"
        };
        titleStyle.Append(new StyleName { Val = "Title" });
        titleStyle.Append(new BasedOn { Val = "Normal" });

        var titleParaProps = new StyleParagraphProperties();
        titleParaProps.Append(new Justification { Val = JustificationValues.Center });
        titleParaProps.Append(new SpacingBetweenLines { After = "480" });
        titleStyle.Append(titleParaProps);

        var titleRunProps = new StyleRunProperties();
        titleRunProps.Append(new Bold());
        titleRunProps.Append(new FontSize { Val = "56" });
        titleStyle.Append(titleRunProps);

        styles.Append(titleStyle);
        
        var codeStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Code"
        };
        codeStyle.Append(new StyleName { Val = "Code" });
        codeStyle.Append(new BasedOn { Val = "Normal" });
        
        var codeRunProps = new StyleRunProperties();
        codeRunProps.Append(new RunFonts { Ascii = "Courier New" });
        codeStyle.Append(codeRunProps);
        
        var codeParaProps = new StyleParagraphProperties();
        codeParaProps.Append(new SpacingBetweenLines { Before = "120", After = "120" });
        codeStyle.Append(codeParaProps);
        
        styles.Append(codeStyle);
        
        var commentStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Comment"
        };
        commentStyle.Append(new StyleName { Val = "Comment" });
        commentStyle.Append(new BasedOn { Val = "Normal" });
        
        var commentRunProps = new StyleRunProperties();
        commentRunProps.Append(new Italic());
        commentRunProps.Append(new Color { Val = "808080" });
        commentStyle.Append(commentRunProps);
        
        styles.Append(commentStyle);
        
        for (int i = 1; i <= 9; i++)
        {
            var headingStyle = new Style
            {
                Type = StyleValues.Paragraph,
                StyleId = "Heading" + i
            };
            headingStyle.Append(new StyleName { Val = "Heading " + i });

            var headingParaProps = new StyleParagraphProperties();
            headingParaProps.Append(new SpacingBetweenLines { Before = "240", After = "240" });
            headingStyle.Append(headingParaProps);

            var headingRunProps = new StyleRunProperties();
            
            int headingSize = 28 - (i * 2);
            if (headingSize < 22) headingSize = 22;

            headingRunProps.Append(new FontSize { Val = (headingSize * 2).ToString() });

            if (i <= 3)
            {
                headingRunProps.Append(new Bold());
            }
            
            if (i == 1)
                headingRunProps.Append(new Color { Val = "2E74B5" });

            headingStyle.Append(headingRunProps);
            styles.Append(headingStyle);
        }

        stylesPart.Styles = styles;
    }
}