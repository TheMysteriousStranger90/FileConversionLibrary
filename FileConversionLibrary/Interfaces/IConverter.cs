namespace FileConversionLibrary.Interfaces;

public interface IConverter<TInput, TOutput>
{
    TOutput Convert(TInput input, object options = null);
}