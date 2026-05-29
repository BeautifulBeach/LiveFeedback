namespace LiveFeedback.Shared.Records;

public abstract record Result<T, E>
{
    public sealed record Ok(T Value) : Result<T, E>;

    public sealed record Err(E Error) : Result<T, E>;

    public bool IsOk => this is Ok;
    public bool IsErr => this is Err;
}