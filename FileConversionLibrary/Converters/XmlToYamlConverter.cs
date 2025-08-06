using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FileConversionLibrary.Converters;

public class XmlToYamlConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object? options = null)
    {
        if (input?.Document?.Root == null)
        {
            throw new ArgumentException("Invalid XML data or missing XDocument");
        }
        
        bool useCamelCase = false;
        bool convertValues = true;
        bool keepStringsForNumbers = false;
        bool includeRootElement = true;
        bool preserveCData = true;
        bool includeComments = false;
        
        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("useCamelCase", out var camel) && camel is bool camelValue)
            {
                useCamelCase = camelValue;
            }
            
            if (optionsDict.TryGetValue("convertValues", out var convert) && convert is bool convertValue)
            {
                convertValues = convertValue;
            }
            
            if (optionsDict.TryGetValue("keepStringsForNumbers", out var keepStrings) && keepStrings is bool keepStringsValue)
            {
                keepStringsForNumbers = keepStringsValue;
            }
            
            if (optionsDict.TryGetValue("includeRootElement", out var includeRoot) && includeRoot is bool includeRootValue)
            {
                includeRootElement = includeRootValue;
            }
            
            if (optionsDict.TryGetValue("preserveCData", out var preserveCdataOption) && preserveCdataOption is bool preserveCdataValue)
            {
                preserveCData = preserveCdataValue;
            }
            
            if (optionsDict.TryGetValue("includeComments", out var includeCommentsOption) && includeCommentsOption is bool includeCommentsValue)
            {
                includeComments = includeCommentsValue;
            }
        }
        
        INamingConvention namingConvention = useCamelCase 
            ? CamelCaseNamingConvention.Instance 
            : NullNamingConvention.Instance;
        
        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(namingConvention)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithMaximumRecursion(100);
            
        var serializer = serializerBuilder.Build();
        
        object yamlObject;
        
        if (includeRootElement)
        {
            yamlObject = ConvertXmlElementWithRoot(input.Document.Root, convertValues, keepStringsForNumbers, 
                preserveCData, includeComments);
        }
        else
        {
            yamlObject = ConvertXmlToObject(input.Document.Root, convertValues, keepStringsForNumbers, 
                preserveCData, includeComments);
        }
        
        return serializer.Serialize(yamlObject);
    }
    
    private object ConvertXmlElementWithRoot(XElement rootElement, bool convertValues, bool keepStringsForNumbers,
        bool preserveCData, bool includeComments)
    {
        var rootName = rootElement.Name.LocalName;
        var rootContent = ConvertXmlToObject(rootElement, convertValues, keepStringsForNumbers, 
            preserveCData, includeComments);
        
        return new Dictionary<string, object>
        {
            [rootName] = rootContent
        };
    }
    
    private object ConvertXmlToObject(XElement element, bool convertValues, bool keepStringsForNumbers,
        bool preserveCData, bool includeComments)
    {
        var result = new Dictionary<string, object>();
        
        foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            var attrName = $"@{attr.Name.LocalName}";
            result[attrName] = ConvertValue(attr.Value, convertValues, keepStringsForNumbers);
        }
        
        if (includeComments)
        {
            var comments = element.Nodes().OfType<XComment>().ToList();
            if (comments.Any())
            {
                var commentsList = comments.Select(c => c.Value.Trim()).ToList();
                result["#comments"] = commentsList;
            }
        }
        
        var cdataNode = element.Nodes().OfType<XCData>().FirstOrDefault();
        if (cdataNode != null && preserveCData)
        {
            result["#cdata"] = cdataNode.Value;
            
            if (!element.Elements().Any() && 
                !element.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value)))
            {
                return result;
            }
        }
        
        if (!element.HasElements)
        {
            string? textValue = null;
            
            if (cdataNode == null || !preserveCData)
            {
                textValue = element.Nodes()
                    .OfType<XText>()
                    .Where(t => !string.IsNullOrWhiteSpace(t.Value))
                    .Select(t => t.Value.Trim())
                    .FirstOrDefault();
            }
            
            if (!string.IsNullOrEmpty(textValue))
            {
                if (result.Count > 0)
                {
                    result["#text"] = ConvertValue(textValue, convertValues, keepStringsForNumbers);
                }
                else
                {
                    return ConvertValue(textValue, convertValues, keepStringsForNumbers);
                }
            }
            
            return result;
        }
        
        var childGroups = element.Elements()
            .GroupBy(e => e.Name.LocalName)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var group in childGroups)
        {
            string name = group.Key;
            List<XElement> elements = group.Value;
            
            if (elements.Count == 1)
            {
                var childElement = elements[0];
                result[name] = ConvertXmlToObject(childElement, convertValues, keepStringsForNumbers, 
                    preserveCData, includeComments);
            }
            else
            {
                var array = new List<object>();
                
                foreach (var childElement in elements)
                {
                    array.Add(ConvertXmlToObject(childElement, convertValues, keepStringsForNumbers, 
                        preserveCData, includeComments));
                }
                
                result[name] = array;
            }
        }
        
        return result;
    }
    
    private object ConvertValue(string value, bool convertValues, bool keepStringsForNumbers)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        
        value = value.Trim();
        
        if (!convertValues || keepStringsForNumbers)
        {
            return value;
        }
        
        if (!value.StartsWith("$") && !value.StartsWith("€") && !value.StartsWith("£"))
        {
            if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            
            if (double.TryParse(value, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
            {
                return doubleValue;
            }
        }
        
        if (bool.TryParse(value, out bool boolValue))
        {
            return boolValue;
        }
        
        return value;
    }
}