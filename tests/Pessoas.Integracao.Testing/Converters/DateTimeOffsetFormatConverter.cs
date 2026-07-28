namespace Pessoas.Integracao.Testing.Converters;

public class DateTimeOffsetFormatConverter : WriteOnlyJsonConverter<DateTimeOffset>
{
    public override void Write(VerifyJsonWriter writer, DateTimeOffset value)
    {
        writer.WriteValue(value.ToString("yyyy-MM-dd HH:mm:ss zzz"));
    }
}
