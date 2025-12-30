namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Result pattern pro business validace místo exceptions
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly string? _error;
    public string Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Success result has no error.");

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) =>
        new(false, error ?? throw new ArgumentNullException(nameof(error)));
}

/// <summary>
/// Result s generickým return value
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Failure result has no value.");

    protected Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        if (isSuccess && value is null)
            throw new InvalidOperationException("Success result must have a value.");

        if (!isSuccess && value is not null)
            throw new InvalidOperationException("Failure result cannot have a value.");

        _value = value;
    }

    public static Result<T> Success(T value) =>
        new(true, value ?? throw new ArgumentNullException(nameof(value)), null);

    public static Result<T> Failure(string error) =>
        new(false, default, error);
}