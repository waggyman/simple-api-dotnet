namespace simple_api.Application.Common;

public sealed class Result<T>
{
    public T? Value { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Error is null;

    public static Result<T> Ok(T value) => new() { Value = value };

    public static Result<T> Fail(string error) => new() { Error = error };
}
