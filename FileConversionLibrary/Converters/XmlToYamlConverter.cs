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
            yamlObject = ConvertXmlElementWithRoot(input.Document.Root, convertValues, keepStringsForNumbers);
        }
        else
        {
            yamlObject = ConvertXmlToObject(input.Document.Root, convertValues, keepStringsForNumbers);
        }
        
        return serializer.Serialize(yamlObject);
    }
    
    private object ConvertXmlElementWithRoot(XElement rootElement, bool convertValues, bool keepStringsForNumbers)
    {
        var rootName = rootElement.Name.LocalName;
        var rootContent = ConvertXmlToObject(rootElement, convertValues, keepStringsForNumbers);
        
        return new Dictionary<string, object>
        {
            [rootName] = rootContent
        };
    }
    
    private object ConvertXmlToObject(XElement element, bool convertValues, bool keepStringsForNumbers)
    {
        if (!element.HasElements && !string.IsNullOrEmpty(element.Value))
        {
            return ConvertValue(element.Value.Trim(), convertValues, keepStringsForNumbers);
        }
        
        if (!element.HasElements && string.IsNullOrEmpty(element.Value))
        {
            return string.Empty;
        }
        
        var childGroups = element.Elements()
            .GroupBy(e => e.Name.LocalName)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        var result = new Dictionary<string, object>();
        
        foreach (var attr in element.Attributes())
        {
            var attrName = $"@{attr.Name.LocalName}";
            result[attrName] = ConvertValue(attr.Value, convertValues, keepStringsForNumbers);
        }
        
        foreach (var group in childGroups)
        {
            string name = group.Key;
            List<XElement> elements = group.Value;
            
            if (elements.Count == 1)
            {
                var childElement = elements[0];
                result[name] = ConvertXmlToObject(childElement, convertValues, keepStringsForNumbers);
            }
            else
            {
                var array = new List<object>();
                
                foreach (var childElement in elements)
                {
                    array.Add(ConvertXmlToObject(childElement, convertValues, keepStringsForNumbers));
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