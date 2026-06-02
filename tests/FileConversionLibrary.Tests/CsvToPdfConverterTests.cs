using FileConversionLibrary.Converters;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Tests;

[TestFixture]
public class CsvToPdfConverterTests
{
    private CsvToPdfConverter _converter = null!;
    private CsvData _simpleCsvData = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new CsvToPdfConverter();

        _simpleCsvData = new CsvData
        {
            Headers = ["Name", "Age", "City"],
            Rows =
            [
                ["Alice", "28", "London"],
                ["Bob", "35", "Paris"],
                ["Carol", "22", "Berlin"]
            ]
        };
    }

    // ---- Argument guards ----

    [Test]
    public void Convert_NullInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _converter.Convert(null!));
    }

    [Test]
    public void Convert_EmptyHeaders_ThrowsArgumentException()
    {
        var data = new CsvData { Headers = [], Rows = [] };
        Assert.Throws<ArgumentException>(() => _converter.Convert(data));
    }

    // ---- Default behaviour ----

    [Test]
    public void Convert_DefaultOptions_ReturnsByteArray()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Convert_DefaultOptions_StartsWithPdfMagicBytes()
    {
        var result = _converter.Convert(_simpleCsvData);
        // PDF magic bytes: %PDF (0x25 0x50 0x44 0x46)
        Assert.That(result[0], Is.EqualTo(0x25));
        Assert.That(result[1], Is.EqualTo(0x50));
        Assert.That(result[2], Is.EqualTo(0x44));
        Assert.That(result[3], Is.EqualTo(0x46));
    }

    [Test]
    public void Convert_DefaultOptions_ProducesValidPdf()
    {
        var result = _converter.Convert(_simpleCsvData);
        Assert.DoesNotThrow(() =>
        {
            var reader = new iTextSharp.text.pdf.PdfReader(result);
            reader.Close();
        });
    }

    [Test]
    public void Convert_DefaultOptions_HasAtLeastOnePage()
    {
        var result = _converter.Convert(_simpleCsvData);
        var reader = new iTextSharp.text.pdf.PdfReader(result);
        try
        {
            Assert.That(reader.NumberOfPages, Is.GreaterThanOrEqualTo(1));
        }
        finally
        {
            reader.Close();
        }
    }

    // ---- Orientation ----

    [Test]
    public void Convert_LandscapeOrientationTrue_PageIsWiderThanTall()
    {
        var opts = new Dictionary<string, object> { ["landscapeOrientation"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        var reader = new iTextSharp.text.pdf.PdfReader(result);
        try
        {
            var pageSize = reader.GetPageSizeWithRotation(1);
            Assert.That(pageSize.Width, Is.GreaterThan(pageSize.Height));
        }
        finally
        {
            reader.Close();
        }
    }

    [Test]
    public void Convert_DefaultOrientation_PageIsTallerThanWide()
    {
        var result = _converter.Convert(_simpleCsvData);
        var reader = new iTextSharp.text.pdf.PdfReader(result);
        try
        {
            var pageSize = reader.GetPageSizeWithRotation(1);
            Assert.That(pageSize.Height, Is.GreaterThan(pageSize.Width));
        }
        finally
        {
            reader.Close();
        }
    }

    // ---- Options ----

    [Test]
    public void Convert_CustomTitle_ProducesValidPdf()
    {
        var opts = new Dictionary<string, object> { ["title"] = "My Report" };
        var result = _converter.Convert(_simpleCsvData, opts);
        var reader = new iTextSharp.text.pdf.PdfReader(result);
        try
        {
            Assert.That(reader.NumberOfPages, Is.GreaterThanOrEqualTo(1));
        }
        finally
        {
            reader.Close();
        }
    }

    [Test]
    public void Convert_AddBordersFalse_ProducesValidPdf()
    {
        var opts = new Dictionary<string, object> { ["addBorders"] = false };
        var result = _converter.Convert(_simpleCsvData, opts);
        Assert.DoesNotThrow(() =>
        {
            var reader = new iTextSharp.text.pdf.PdfReader(result);
            reader.Close();
        });
    }

    [Test]
    public void Convert_AlternateRowColorsTrue_ProducesValidPdf()
    {
        var opts = new Dictionary<string, object> { ["alternateRowColors"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        Assert.DoesNotThrow(() =>
        {
            var reader = new iTextSharp.text.pdf.PdfReader(result);
            reader.Close();
        });
    }

    [Test]
    public void Convert_IncludeRowNumbersTrue_ProducesValidPdf()
    {
        var opts = new Dictionary<string, object> { ["includeRowNumbers"] = true };
        var result = _converter.Convert(_simpleCsvData, opts);
        Assert.DoesNotThrow(() =>
        {
            var reader = new iTextSharp.text.pdf.PdfReader(result);
            reader.Close();
        });
    }

    // ---- Empty rows ----

    [Test]
    public void Convert_NoRows_ProducesValidPdf()
    {
        var data = new CsvData { Headers = ["A", "B"], Rows = [] };
        var result = _converter.Convert(data);
        Assert.DoesNotThrow(() =>
        {
            var reader = new iTextSharp.text.pdf.PdfReader(result);
            reader.Close();
        });
    }
}
