# File Conversion Library

This library provides functionality to convert CSV files to various formats such as XML, PDF, Word, Json and YAML.

## Usage

### CSV to XML Conversion

```csharp
var converter = new CsvToXmlConverter();
converter.ConvertCsvToXml("path/to/input.csv", "path/to/output.xml");
```

### CSV to PDF Conversion

```csharp
var converter = new CsvToPdfConverter();
converter.ConvertCsvToPdf("path/to/input.csv", "path/to/output.pdf");
```

### CSV to Word Conversion

```csharp
var converter = new CsvToWordConverter();
converter.ConvertCsvToWord("path/to/input.csv", "path/to/output.docx");
```

### CSV to YAML Conversion

```csharp
var converter = new CsvToYamlConverter();
converter.ConvertCsvToYaml("path/to/input.csv", "path/to/output.yaml");
```

### CSV to JSON Conversion

```csharp
var converter = new CsvToJsonConverter();
converter.ConvertCsvToYaml("path/to/input.csv", "path/to/output.json");
```

### XML to CSV Conversion
```csharp
var converter = new XmlToCsvConverter();
converter.Convert("path/to/input.xml", "path/to/output.csv");
```

## Notes
The CsvToXmlConverter, CsvToJsonConverter, CsvToPdfConverter, CsvToWordConverter, and CsvToYamlConverter classes read a CSV file, parse it, and write the content to an XML, JSON, PDF, Word, and YAML file respectively. The first line of the CSV file is assumed to be the header.
The XmlToCsvConverter class reads an XML file, parses it, and writes the content to a CSV file. The XML file is assumed to be in the format produced by the CsvToXmlConverter.
All classes handle errors such as file not found and invalid file format, and print an error message to the console.

## Author

Bohdan Harabadzhyu

## License

[MIT](https://choosealicense.com/licenses/mit/)

