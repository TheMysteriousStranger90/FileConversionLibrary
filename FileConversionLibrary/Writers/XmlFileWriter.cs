using System.Text;
using System.Xml;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary.Writers;

public class XmlFileWriter : IFileWriter<string>
{
    private readonly IExceptionHandler _exceptionHandler;

    public XmlFileWriter(IExceptionHandler exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task WriteAsync(string filePath, string data, object options = null)
    {
        try
        {
            if (!Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.ChangeExtension(filePath, ".xml");
            }

            bool useIndent = true;
            string indentChars = "  ";
            bool useTabs = false;
            bool expandEntities = false;


            if (options is Dictionary<string, object> optionsDict)
            {
                if (optionsDict.TryGetValue("useIndent", out var indent) && indent is bool indentValue)
                {
                    useIndent = indentValue;
                }

                if (optionsDict.TryGetValue("useTabs", out var tabs) && tabs is bool tabsValue && tabsValue)
                {
                    useTabs = true;
                    indentChars = "\t";
                }

                if (optionsDict.TryGetValue("indentSize", out var size) && size is int sizeValue && !useTabs)
                {
                    indentChars = new string(' ', sizeValue);
                }

                if (optionsDict.TryGetValue("expandEntities", out var expand) && expand is bool expandValue)
                {
                    expandEntities = expandValue;
                }
            }

            try
            {
                var doc = XDocument.Parse(data);

                var settings = new XmlWriterSettings
                {
                    Indent = useIndent,
                    IndentChars = indentChars,
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = Encoding.UTF8,
                    CheckCharacters = true
                };

                using (var writer = XmlWriter.Create(filePath, settings))
                {
                    await Task.Run(() => doc.Save(writer));
                }
            }
            catch (XmlException xmlEx)
            {
                await WriteRawXmlAsync(filePath, data, useIndent, indentChars, expandEntities);
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }

    private async Task WriteRawXmlAsync(string filePath, string xmlData, bool useIndent, string indentChars,
        bool expandEntities)
    {
        if (!xmlData.TrimStart().StartsWith("<?xml"))
        {
            xmlData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + xmlData;
        }

        await File.WriteAllTextAsync(filePath, xmlData, Encoding.UTF8);

        _exceptionHandler?.Handle(new Exception("Used raw XML writing due to parsing issues"));
    }

    private string EscapeXML(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        var sb = new StringBuilder();
        foreach (var ch in content)
        {
            switch (ch)
            {
                case '\'':
                    sb.Append("&apos;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '&':
                    sb.Append("&amp;");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        return sb.ToString();
    }
}