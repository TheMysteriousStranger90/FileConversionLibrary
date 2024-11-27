# File Conversion Library
![Image 1](Screenshots/Screen1.png)

This library provides functionality to convert CSV and XML files to various formats such as XML, PDF, Word, JSON, and YAML.

## Usage

### CSV to XML Conversion

```csharp
var converter = new CsvToXmlConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.csv", @"C:\Users\User\Desktop\output.xml");
```

### CSV to PDF Conversion

```csharp
var converter = new CsvToPdfConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.csv", @"C:\Users\User\Desktop\output.pdf");
```

### CSV to Word Conversion

```csharp
var converter = new CsvToWordConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.csv", @"C:\Users\User\Desktop\output.docx");
```

### CSV to YAML Conversion

```csharp
var converter = new CsvToYamlConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.csv", @"C:\Users\User\Desktop\output.yaml");
```

### CSV to JSON Conversion

```csharp
var converter = new CsvToJsonConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.csv", @"C:\Users\User\Desktop\output.json");
```

### XML to CSV Conversion
```csharp
var converter = new XmlToCsvConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.csv");
```

### XML to PDF Conversion
```csharp
var converter = new XmlToPdfConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.pdf");
```

### XML to Word Conversion
```csharp
var converter = new XmlToWordConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.docx");
```

### XML to YAML Conversion
```csharp
var converter = new XmlToYamlConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.yaml");
```

### XML to JSON Conversion
```csharp
var converter = new XmlToJsonConverter();
await converter.ConvertAsync(@"C:\Users\User\Desktop\input.xml", @"C:\Users\User\Desktop\output.json");
```

## Notes
Notes
The CsvToXmlConverter, CsvToJsonConverter, CsvToPdfConverter, CsvToWordConverter, and CsvToYamlConverter classes read a CSV file, parse it, and write the content to an XML, JSON, PDF, Word, and YAML file respectively. The first line of the CSV file is assumed to be the header.

The XmlToCsvConverter, XmlToJsonConverter, XmlToPdfConverter, XmlToWordConverter, and XmlToYamlConverter classes read an XML file, parse it, and write the content to a CSV, JSON, PDF, Word, and YAML file respectively. The XML file is assumed to be in the format produced by the CsvToXmlConverter.

All classes handle errors such as file not found and invalid file format, and print an error message to the console.

## Contributing

Contributions are welcome. Please fork the repository and create a pull request with your changes.

## Author

Bohdan Harabadzhyu

## License

[MIT](https://choosealicense.com/licenses/mit/)