namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Result pattern pro business validace místo exceptions
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, null);
    public static Result Failure(string error) => new Result(false, error);
}

/// <summary>
/// Result s generickým return value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public new static Result<T> Failure(string error) => new Result<T>(false, default, error);
}