using System.Text.Json;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace FileConversionLibrary.Converters;

public class XmlToJsonConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid XML data");
        }

        var useIndentation = true;
        var preserveStructure = false;
        
        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("useIndentation", out var indent) && indent is bool indentValue)
            {
                useIndentation = indentValue;
            }
            
            if (optionsDict.TryGetValue("preserveStructure", out var preserveObj) && preserveObj is bool preserveValue)
            {
                preserveStructure = preserveValue;
            }
        }
        
        if (preserveStructure && input.Document != null)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = useIndentation ? Formatting.Indented : Formatting.None
            };
            
            var xElement = input.Document.Root;
            return JsonConvert.SerializeXNode(xElement, Formatting.Indented, true);
        }
        
        var dataList = new List<Dictionary<string, string>>();
        foreach (var row in input.Rows)
        {
            var rowData = new Dictionary<string, string>();
            for (var j = 0; j < input.Headers.Length && j < row.Length; j++)
            {
                if (!string.IsNullOrEmpty(row[j]))
                {
                    rowData[input.Headers[j]] = row[j];
                }
            }
            dataList.Add(rowData);
        }

        var formatting = useIndentation ? Formatting.Indented : Formatting.None;
        return JsonConvert.SerializeObject(dataList, formatting);
    }
}