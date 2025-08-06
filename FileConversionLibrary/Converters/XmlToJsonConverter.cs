using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace FileConversionLibrary.Converters;

public class XmlToJsonConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object? options = null)
    {
        if (input?.Document == null)
        {
            throw new ArgumentException("Invalid XML data or missing XDocument");
        }

        bool useIndentation = true;
        bool convertValues = true;
        bool removeWhitespace = true;
        bool preserveCData = true;

        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("useIndentation", out var indent) && indent is bool indentValue)
            {
                useIndentation = indentValue;
            }

            if (optionsDict.TryGetValue("convertValues", out var convert) && convert is bool convertValue)
            {
                convertValues = convertValue;
            }

            if (optionsDict.TryGetValue("removeWhitespace", out var remove) && remove is bool removeValue)
            {
                removeWhitespace = removeValue;
            }
            
            if (optionsDict.TryGetValue("preserveCData", out var preserve) && preserve is bool preserveValue)
            {
                preserveCData = preserveValue;
            }
        }

        var docToConvert = new XDocument(input.Document);

        if (preserveCData)
        {
            PreserveCDataSections(docToConvert.Root);
        }

        if (removeWhitespace)
        {
            if (docToConvert.Root != null) RemoveWhitespaceNodes(docToConvert.Root);
        }

        string json = JsonConvert.SerializeXNode(docToConvert.Root,
            Formatting.Indented,
            omitRootObject: false);

        if (convertValues || removeWhitespace)
        {
            JObject jsonObj = JObject.Parse(json);

            if (removeWhitespace)
            {
                RemoveWhitespaceTextNodes(jsonObj);
            }

            if (convertValues)
            {
                ConvertStringValues(jsonObj);
            }

            json = jsonObj.ToString(useIndentation ? Formatting.Indented : Formatting.None);
        }

        return json;
    }
    
    private void PreserveCDataSections(XElement? element)
    {
        if (element == null) return;
        
        var cdataNodes = element.Nodes().OfType<XCData>().ToList();
        
        if (cdataNodes.Any())
        {
            element.SetAttributeValue("cdata", "true");
            
            string cdataContent = string.Join("", cdataNodes.Select(c => c.Value));
            
            foreach (var node in element.Nodes().ToList())
            {
                node.Remove();
            }
            
            element.Add(new XText(cdataContent));
        }
        
        foreach (var child in element.Elements().ToList())
        {
            PreserveCDataSections(child);
        }
    }

    private void RemoveWhitespaceNodes(XElement element)
    {
        var nodesToRemove = element.Nodes()
            .OfType<XText>()
            .Where(t => string.IsNullOrWhiteSpace(t.Value))
            .ToList();

        foreach (var node in nodesToRemove)
        {
            node.Remove();
        }

        foreach (var child in element.Elements())
        {
            RemoveWhitespaceNodes(child);
        }
    }

    private void RemoveWhitespaceTextNodes(JToken token)
    {
        if (token is JObject obj)
        {
            var textProps = obj.Properties()
                .Where(p => p.Name == "#text")
                .ToList();

            foreach (var prop in textProps)
            {
                if (prop.Value is JArray textArray)
                {
                    bool onlyWhitespace = true;

                    foreach (var item in textArray)
                    {
                        if (item is JValue val && val.Type == JTokenType.String)
                        {
                            string? text = val.Value<string>();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                onlyWhitespace = false;
                                break;
                            }
                        }
                        else
                        {
                            onlyWhitespace = false;
                            break;
                        }
                    }

                    if (onlyWhitespace)
                    {
                        prop.Remove();
                    }
                }
                else if (prop.Value is JValue val && val.Type == JTokenType.String)
                {
                    string? text = val.Value<string>();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        prop.Remove();
                    }
                }
            }

            foreach (var property in obj.Properties().ToList())
            {
                RemoveWhitespaceTextNodes(property.Value);
            }
        }
        else if (token is JArray array)
        {
            foreach (var item in array)
            {
                RemoveWhitespaceTextNodes(item);
            }
        }
    }

    private void ConvertStringValues(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties().ToList())
            {
                if (obj.TryGetValue("@cdata", out var cdataFlag) && 
                    cdataFlag.Type == JTokenType.Boolean && 
                    cdataFlag.Value<bool>() && 
                    property.Name != "@cdata")
                {
                    continue;
                }
                
                if (property.Value is JValue jValue && jValue.Type == JTokenType.String)
                {
                    string? strValue = jValue.Value<string>();

                    if (int.TryParse(strValue, out int intValue))
                    {
                        property.Value = intValue;
                    }
                    else if (double.TryParse(strValue, System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                    {
                        property.Value = doubleValue;
                    }
                    else if (bool.TryParse(strValue, out bool boolValue))
                    {
                        property.Value = boolValue;
                    }
                }
                else
                {
                    ConvertStringValues(property.Value);
                }
            }
        }
        else if (token is JArray jArray)
        {
            foreach (var item in jArray)
            {
                ConvertStringValues(item);
            }
        }
    }
}