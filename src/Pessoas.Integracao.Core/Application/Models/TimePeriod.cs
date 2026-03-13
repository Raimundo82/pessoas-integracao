namespace Pessoas.Integracao.Core.Application.Models;

public sealed class TimePeriod
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public TimePeriod(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End timestamp cannot be earlier than start timestamp.");

        Start = start;
        End = end;
    }

    public string StartAsString() => Start.ToString("yyyy-MM-dd HH:mm:ss");
    public string EndAsString() => End.ToString("yyyy-MM-dd HH:mm:ss");
}