using System.Data;
using System.Xml;

namespace FileConversionLibrary.Helpers;

public static class XmlHelperFile
{
    public static async Task<(string[] Headers, List<string[]> Rows)> ReadXmlAsync(string xmlFilePath)
    {
        var xmlContent = await File.ReadAllTextAsync(xmlFilePath);
        var xmlFile = new XmlDocument();
        xmlFile.LoadXml(xmlContent);

        var xmlReader = new XmlNodeReader(xmlFile);
        var dataSet = new DataSet();
        dataSet.ReadXml(xmlReader);

        if (dataSet.Tables.Count == 0)
        {
            throw new Exception("No tables found in the XML file.");
        }

        var table = dataSet.Tables[0];
        var headers = new string[table.Columns.Count];
        for (var i = 0; i < table.Columns.Count; i++)
        {
            headers[i] = table.Columns[i].ColumnName;
        }

        var rows = new List<string[]>();
        foreach (DataRow row in table.Rows)
        {
            var rowData = new string[table.Columns.Count];
            for (var i = 0; i < table.Columns.Count; i++)
            {
                rowData[i] = row[i].ToString();
            }
            rows.Add(rowData);
        }

        return (headers, rows);
    }
}