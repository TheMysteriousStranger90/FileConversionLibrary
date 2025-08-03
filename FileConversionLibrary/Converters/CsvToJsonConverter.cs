using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace FileConversionLibrary.Converters
{
    public class CsvToJsonConverter : IConverter<CsvData, string>
    {
        public string Convert(CsvData input, object? options = null)
        {
            if (input?.Headers == null || input.Rows == null)
            {
                throw new ArgumentException("Invalid CSV data");
            }

            if (input.Headers.Length == 0)
            {
                throw new ArgumentException("No headers found in CSV data");
            }

            bool convertValues = true;
            bool useIndentation = true;
            bool includeRowNumbers = false;
            bool groupByColumn = false;
            string? groupByColumnName = null;
            bool createNestedObjects = false;
            string? nestedSeparator = ".";
            bool preserveEmptyValues = false;
            string? dateFormat = null;
            bool convertArrays = false;
            string? arrayDelimiter = ";";

            if (options is Dictionary<string, object> optionsDict)
            {
                if (optionsDict.TryGetValue("convertValues", out var convert) && convert is bool convertValue)
                {
                    convertValues = convertValue;
                }

                if (optionsDict.TryGetValue("useIndentation", out var indent) && indent is bool indentValue)
                {
                    useIndentation = indentValue;
                }

                if (optionsDict.TryGetValue("includeRowNumbers", out var rowNums) && rowNums is bool rowNumsValue)
                {
                    includeRowNumbers = rowNumsValue;
                }

                if (optionsDict.TryGetValue("groupByColumn", out var group) && group is bool groupValue)
                {
                    groupByColumn = groupValue;
                }

                if (optionsDict.TryGetValue("groupByColumnName", out var groupCol) && groupCol is string groupColValue)
                {
                    groupByColumnName = groupColValue;
                }

                if (optionsDict.TryGetValue("createNestedObjects", out var nested) && nested is bool nestedValue)
                {
                    createNestedObjects = nestedValue;
                }

                if (optionsDict.TryGetValue("nestedSeparator", out var separator) && separator is string separatorValue)
                {
                    nestedSeparator = separatorValue;
                }

                if (optionsDict.TryGetValue("preserveEmptyValues", out var preserve) && preserve is bool preserveValue)
                {
                    preserveEmptyValues = preserveValue;
                }

                if (optionsDict.TryGetValue("dateFormat", out var dateFormatObj) && dateFormatObj is string dateFormatValue)
                {
                    dateFormat = dateFormatValue;
                }

                if (optionsDict.TryGetValue("convertArrays", out var arrays) && arrays is bool arraysValue)
                {
                    convertArrays = arraysValue;
                }

                if (optionsDict.TryGetValue("arrayDelimiter", out var delim) && delim is string delimValue)
                {
                    arrayDelimiter = delimValue;
                }
            }

            if (groupByColumn && !string.IsNullOrEmpty(groupByColumnName))
            {
                return ConvertGrouped(input, groupByColumnName, convertValues, useIndentation, 
                    preserveEmptyValues, dateFormat, convertArrays, arrayDelimiter, createNestedObjects, nestedSeparator);
            }

            var resultList = new List<Dictionary<string, object>>();

            for (int rowIndex = 0; rowIndex < input.Rows.Count; rowIndex++)
            {
                var row = input.Rows[rowIndex];
                var rowDict = new Dictionary<string, object>();

                if (includeRowNumbers)
                {
                    rowDict["_rowNumber"] = rowIndex + 1;
                }

                for (int i = 0; i < input.Headers.Length && i < row.Length; i++)
                {
                    var header = input.Headers[i];
                    var value = row[i];

                    if (!preserveEmptyValues && string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    var processedValue = ProcessValue(value, convertValues, dateFormat, convertArrays, arrayDelimiter);

                    if (createNestedObjects && header.Contains(nestedSeparator))
                    {
                        CreateNestedObject(rowDict, header, processedValue, nestedSeparator);
                    }
                    else
                    {
                        rowDict[header] = processedValue;
                    }
                }

                resultList.Add(rowDict);
            }

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = useIndentation ? Formatting.Indented : Formatting.None,
                NullValueHandling = preserveEmptyValues ? NullValueHandling.Include : NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            return JsonConvert.SerializeObject(resultList, jsonSettings);
        }

        private string ConvertGrouped(CsvData input, string groupByColumnName, bool convertValues, 
            bool useIndentation, bool preserveEmptyValues, string? dateFormat, bool convertArrays, 
            string? arrayDelimiter, bool createNestedObjects, string? nestedSeparator)
        {
            var groupColumnIndex = Array.IndexOf(input.Headers, groupByColumnName);
            if (groupColumnIndex == -1)
            {
                throw new ArgumentException($"Group column '{groupByColumnName}' not found in headers");
            }

            var groups = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var row in input.Rows)
            {
                if (groupColumnIndex >= row.Length) continue;

                var groupValue = row[groupColumnIndex] ?? "null";
                if (!groups.ContainsKey(groupValue))
                {
                    groups[groupValue] = new List<Dictionary<string, object>>();
                }

                var rowDict = new Dictionary<string, object>();
                for (int i = 0; i < input.Headers.Length && i < row.Length; i++)
                {
                    if (i == groupColumnIndex) continue;

                    var header = input.Headers[i];
                    var value = row[i];

                    if (!preserveEmptyValues && string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    var processedValue = ProcessValue(value, convertValues, dateFormat, convertArrays, arrayDelimiter);

                    if (createNestedObjects && header.Contains(nestedSeparator))
                    {
                        CreateNestedObject(rowDict, header, processedValue, nestedSeparator);
                    }
                    else
                    {
                        rowDict[header] = processedValue;
                    }
                }

                groups[groupValue].Add(rowDict);
            }

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = useIndentation ? Formatting.Indented : Formatting.None,
                NullValueHandling = preserveEmptyValues ? NullValueHandling.Include : NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(groups, jsonSettings);
        }

        private object ProcessValue(string? value, bool convertValues, string? dateFormat, 
            bool convertArrays, string? arrayDelimiter)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (convertArrays && !string.IsNullOrEmpty(arrayDelimiter) && value.Contains(arrayDelimiter))
            {
                var arrayItems = value.Split(arrayDelimiter, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => ProcessSingleValue(item.Trim(), convertValues, dateFormat))
                    .ToArray();
                return arrayItems;
            }

            return ProcessSingleValue(value, convertValues, dateFormat);
        }

        private object ProcessSingleValue(string value, bool convertValues, string? dateFormat)
        {
            if (!convertValues)
                return value;

            if (!string.IsNullOrEmpty(dateFormat))
            {
                if (DateTime.TryParseExact(value, dateFormat, CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out var dateValue))
                {
                    return dateValue;
                }
            }
            else if (DateTime.TryParse(value, out var genericDateValue))
            {
                return genericDateValue;
            }

            if (int.TryParse(value, out var intValue))
            {
                return intValue;
            }

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
            {
                return doubleValue;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            {
                return decimalValue;
            }

            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }

            if (value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("NULL", StringComparison.Ordinal))
            {
                return null!;
            }

            return value;
        }

        private void CreateNestedObject(Dictionary<string, object> target, string key, object value, string separator)
        {
            var parts = key.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var current = target;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!current.ContainsKey(part))
                {
                    current[part] = new Dictionary<string, object>();
                }

                if (current[part] is Dictionary<string, object> dict)
                {
                    current = dict;
                }
                else
                {
                    current[part] = new Dictionary<string, object>();
                    current = (Dictionary<string, object>)current[part];
                }
            }

            current[parts[^1]] = value;
        }
    }
}