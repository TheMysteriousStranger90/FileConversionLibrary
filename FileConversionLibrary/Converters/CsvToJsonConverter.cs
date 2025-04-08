using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace FileConversionLibrary.Converters
{
    public class CsvToJsonConverter : IConverter<CsvData, string>
    {
        public string Convert(CsvData input, object options = null)
        {
            if (input?.Headers == null || input.Rows == null)
            {
                throw new ArgumentException("Invalid CSV data");
            }
            
            bool convertValues = true;
            bool useIndentation = true;
            
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
            }
            
            var resultList = new List<Dictionary<string, object>>();
            
            foreach (var row in input.Rows)
            {
                var rowDict = new Dictionary<string, object>();
                
                for (int i = 0; i < input.Headers.Length && i < row.Length; i++)
                {
                    var header = input.Headers[i];
                    var value = row[i];
                    
                    if (convertValues && !string.IsNullOrEmpty(value))
                    {
                        if (int.TryParse(value, out var intValue))
                        {
                            rowDict[header] = intValue;
                            continue;
                        }
                        
                        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
                        {
                            rowDict[header] = doubleValue;
                            continue;
                        }
                        
                        if (bool.TryParse(value, out var boolValue))
                        {
                            rowDict[header] = boolValue;
                            continue;
                        }
                    }
                    
                    rowDict[header] = value;
                }
                
                resultList.Add(rowDict);
            }
            
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = useIndentation ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Include
            };
            
            return JsonConvert.SerializeObject(resultList, jsonSettings);
        }
    }
}