namespace Api.Runs;

public sealed class RunConflictException(Run stored) : Exception
{
    public Run Stored { get; } = stored;
}
