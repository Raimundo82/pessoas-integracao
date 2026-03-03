public sealed class TimePeriodDto
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public TimePeriodDto(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End timestamp cannot be earlier than start timestamp.");

        Start = start;
        End = end;
    }

    public string StartAsSapString() => Start.ToString("yyyy-MM-dd HH:mm:ss");
    public string EndAsSapString() => End.ToString("yyyy-MM-dd HH:mm:ss");
}