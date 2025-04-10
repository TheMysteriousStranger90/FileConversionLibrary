﻿using System.Text;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Converters;

public class CsvToXmlConverter : IConverter<CsvData, string>
{
    public enum XmlOutputFormat
    {
        Elements,
        Attributes,
        Mixed
    }
    
    public string Convert(CsvData input, object options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid CSV data");
        }

        var outputFormat = XmlOutputFormat.Elements;
        bool useCData = true;
        
        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("format", out var format) && format is XmlOutputFormat fmt)
            {
                outputFormat = fmt;
            }
            
            if (optionsDict.TryGetValue("useCData", out var cdata) && cdata is bool cdataValue)
            {
                useCData = cdataValue;
            }
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XComment($" Generated by FileConversionLibrary at {DateTime.Now:yyyy-MM-dd HH:mm:ss} "),
            new XElement("root")
        );
        
        var root = doc.Root;

        var attributeHeaders = DetermineAttributeFields(input, outputFormat);
        
        foreach (var row in input.Rows)
        {
            var rowElement = new XElement("row");

            if (outputFormat != XmlOutputFormat.Elements)
            {
                for (var i = 0; i < input.Headers.Length; i++)
                {
                    if (i < row.Length && attributeHeaders.Contains(i))
                    {
                        var header = input.Headers[i];
                        var value = row[i];
                        
                        if (string.IsNullOrEmpty(value))
                            continue;
                        
                        rowElement.SetAttributeValue(SanitizeXmlElementName(header), EscapeXmlAttributeValue(value));
                    }
                }
            }
            
            for (var i = 0; i < input.Headers.Length; i++)
            {
                if (i < row.Length && !attributeHeaders.Contains(i))
                {
                    var header = input.Headers[i];
                    var value = row[i];
                    
                    if (string.IsNullOrEmpty(value))
                    {
                        rowElement.Add(new XElement(SanitizeXmlElementName(header)));
                        continue;
                    }

                    XElement fieldElement;
                    if (useCData && NeedsCData(value))
                    {
                        fieldElement = new XElement(SanitizeXmlElementName(header), 
                                       new XCData(value));
                    }
                    else
                    {
                        fieldElement = new XElement(SanitizeXmlElementName(header), value);
                    }
                    
                    rowElement.Add(fieldElement);
                }
            }
            
            root.Add(rowElement);
        }

        return doc.ToString();
    }
    
    private HashSet<int> DetermineAttributeFields(CsvData input, XmlOutputFormat format)
    {
        var result = new HashSet<int>();
        
        if (format == XmlOutputFormat.Elements)
            return result;
        
        for (int i = 0; i < input.Headers.Length; i++)
        {
            var header = input.Headers[i];

            if (format == XmlOutputFormat.Attributes)
            {
                result.Add(i);
                continue;
            }
            
            bool isAttributeCandidate = true;
            int maxLength = 0;
            bool hasSpecialChars = false;
            
            foreach (var row in input.Rows)
            {
                if (i < row.Length && !string.IsNullOrEmpty(row[i]))
                {
                    var value = row[i];
                    maxLength = Math.Max(maxLength, value.Length);
                    
                    if (value.Contains('\n') || value.Contains('\r') || 
                        value.Contains('<') || value.Contains('>') ||
                        value.Length > 80)
                    {
                        hasSpecialChars = true;
                        break;
                    }
                }
            }

            isAttributeCandidate = isAttributeCandidate && !hasSpecialChars && maxLength <= 80;
            
            if (isAttributeCandidate)
            {
                result.Add(i);
            }
        }
        
        return result;
    }
    
    private bool NeedsCData(string value)
    {
        return value.Contains('<') || value.Contains('>') || 
               value.Contains('&') || value.Contains('\n') ||
               value.Contains("]]>");
    }

    private string SanitizeXmlElementName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Field";
            
        string sanitized = name.Trim();
        
        if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = "_" + sanitized;
        }
        
        var result = new StringBuilder();
        foreach (char c in sanitized)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                result.Append(c);
            else
                result.Append('_');
        }
        
        return result.ToString();
    }
    
    private string EscapeXmlAttributeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
            
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}