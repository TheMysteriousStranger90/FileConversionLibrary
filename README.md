# File Conversion Library

This library provides functionality to convert CSV files to XML and vice versa.

## Usage

### CSV to XML Conversion

```csharp
var converter = new CsvToXmlConverter();
converter.Convert("path/to/input.csv", "path/to/output.xml");
```

### XML to CSV Conversion
```csharp
var converter = new XmlToCsvConverter();
converter.Convert("path/to/input.xml", "path/to/output.csv");
```

## Notes
The CsvToXmlConverter class reads a CSV file, parses it, and writes the content to an XML file. The first line of the CSV file is assumed to be the header.
The XmlToCsvConverter class reads an XML file, parses it, and writes the content to a CSV file. The XML file is assumed to be in the format produced by the CsvToXmlConverter.
Both classes handle errors such as file not found and invalid file format, and print an error message to the console.

