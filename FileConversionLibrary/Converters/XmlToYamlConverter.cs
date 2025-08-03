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
        if (input?.Document == null)
        {
            throw new ArgumentException("Invalid XML data or missing XDocument");
        }
        
        bool useCamelCase = false;
        bool convertValues = true;
        bool keepStringsForNumbers = false;
        
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
        }
        
        INamingConvention namingConvention = useCamelCase 
            ? CamelCaseNamingConvention.Instance 
            : NullNamingConvention.Instance;
        
        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(namingConvention)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithMaximumRecursion(100);
            
        var serializer = serializerBuilder.Build();
        
        var yamlObject = ConvertXmlToObject(input.Document.Root, convertValues, keepStringsForNumbers);
        
        return serializer.Serialize(yamlObject);
    }
    
    private object ConvertXmlToObject(XElement element, bool convertValues, bool keepStringsForNumbers)
    {
        if (!element.HasElements && !string.IsNullOrEmpty(element.Value))
        {
            string value = element.Value.Trim();
            
            if (convertValues && !keepStringsForNumbers)
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
                
                if (bool.TryParse(value, out bool boolValue))
                {
                    return boolValue;
                }
            }
            
            return value;
        }
        
        var childGroups = element.Elements()
            .GroupBy(e => e.Name.LocalName)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        var result = new Dictionary<string, object>();
        
        foreach (var group in childGroups)
        {
            string name = group.Key;
            List<XElement> elements = group.Value;
            
            if (elements.Count == 1)
            {
                var childElement = elements[0];
                
                if (childElement.HasElements)
                {
                    result[name] = ConvertXmlToObject(childElement, convertValues, keepStringsForNumbers);
                }
                else
                {
                    string value = childElement.Value.Trim();
                    
                    if (convertValues && !keepStringsForNumbers)
                    {
                        if (int.TryParse(value, out int intValue))
                        {
                            result[name] = intValue;
                            continue;
                        }
                        
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any, 
                                System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                        {
                            result[name] = doubleValue;
                            continue;
                        }
                        
                        if (bool.TryParse(value, out bool boolValue))
                        {
                            result[name] = boolValue;
                            continue;
                        }
                    }
                    
                    result[name] = value;
                }
            }
            else
            {
                var array = new List<object>();
                
                foreach (var childElement in elements)
                {
                    if (childElement.HasElements)
                    {
                        array.Add(ConvertXmlToObject(childElement, convertValues, keepStringsForNumbers));
                    }
                    else
                    {
                        string value = childElement.Value.Trim();
                        
                        if (convertValues && !keepStringsForNumbers)
                        {
                            if (int.TryParse(value, out int intValue))
                            {
                                array.Add(intValue);
                                continue;
                            }
                            
                            if (double.TryParse(value, System.Globalization.NumberStyles.Any, 
                                    System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                            {
                                array.Add(doubleValue);
                                continue;
                            }
                            
                            if (bool.TryParse(value, out bool boolValue))
                            {
                                array.Add(boolValue);
                                continue;
                            }
                        }
                        
                        array.Add(value);
                    }
                }
                
                result[name] = array;
            }
        }
        
        return result;
    }
}