﻿using FileConversionLibrary.Converters;
using FileConversionLibrary.Enums;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Factories;

public class ConverterFactory
{
    public IConverter<TInput, TOutput> GetConverter<TInput, TOutput>(OutputFormat format = OutputFormat.Json)
    {
        if (typeof(TInput) == typeof(CsvData) && typeof(TOutput) == typeof(string))
        {
            if (format == OutputFormat.Xml)
            {
                return (IConverter<TInput, TOutput>)(object)new CsvToXmlConverter();
            }
            else if (format == OutputFormat.Yaml)
            {
                return (IConverter<TInput, TOutput>)(object)new CsvToYamlConverter();
            }
            else
            {
                return (IConverter<TInput, TOutput>)(object)new CsvToJsonConverter();
            }
        }

        if (typeof(TInput) == typeof(CsvData) && typeof(TOutput) == typeof(byte[]) && format == OutputFormat.Pdf)
        {
            return (IConverter<TInput, TOutput>)(object)new CsvToPdfConverter();
        }

        if (typeof(TInput) == typeof(CsvData) && typeof(TOutput) == typeof(byte[]) && format == OutputFormat.Word)
        {
            return (IConverter<TInput, TOutput>)(object)new CsvToWordConverter();
        }

        if (typeof(TInput) == typeof(XmlData) && typeof(TOutput) == typeof(string) && format == OutputFormat.Csv)
        {
            return (IConverter<TInput, TOutput>)(object)new XmlToCsvConverter();
        }

        if (typeof(TInput) == typeof(XmlData) && typeof(TOutput) == typeof(string) && format == OutputFormat.Json)
        {
            return (IConverter<TInput, TOutput>)(object)new XmlToJsonConverter();
        }

        if (typeof(TInput) == typeof(XmlData) && typeof(TOutput) == typeof(byte[]) && format == OutputFormat.Pdf)
        {
            return (IConverter<TInput, TOutput>)(object)new XmlToPdfConverter();
        }
        
        if (typeof(TInput) == typeof(XmlData) && typeof(TOutput) == typeof(byte[]) && format == OutputFormat.Word)
        {
            return (IConverter<TInput, TOutput>)(object)new XmlToWordConverter();
        }
        
        if (typeof(TInput) == typeof(XmlData) && typeof(TOutput) == typeof(string) && format == OutputFormat.Yaml)
        {
            return (IConverter<TInput, TOutput>)(object)new XmlToYamlConverter();
        }

        throw new NotSupportedException(
            $"Converter from {typeof(TInput)} to {typeof(TOutput)} with format {format} is not supported");
    }
}