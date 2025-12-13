namespace QuikytLoader.Domain.Common;

/// <summary>
/// Represents the result of an operation that can succeed (with no value) or fail.
/// </summary>
public readonly struct Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Pattern matching helper for explicit success/failure handling.
    /// </summary>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error!);
    }

    /// <summary>
    /// Performs an action based on the result state without returning a value.
    /// </summary>
    public void Match(
        Action onSuccess,
        Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error!);
    }

    // Implicit conversion from Error to failed Result
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail.
/// </summary>
public readonly struct Result<TValue>
{
    public bool IsSuccess { get; }
    public TValue? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, TValue? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TValue> Success(TValue value) => new(true, value, null);
    public static Result<TValue> Failure(Error error) => new(false, default, error);

    /// <summary>
    /// Pattern matching helper for explicit success/failure handling.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }

    /// <summary>
    /// Performs an action based on the result state without returning a value.
    /// </summary>
    public void Match(
        Action<TValue> onSuccess,
        Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(Error!);
    }

    /// <summary>
    /// Maps the success value to a new value, propagating failures.
    /// </summary>
    public Result<TNewValue> Map<TNewValue>(Func<TValue, TNewValue> mapper)
    {
        return IsSuccess
            ? Result<TNewValue>.Success(mapper(Value!))
            : Result<TNewValue>.Failure(Error!);
    }

    /// <summary>
    /// Asynchronously maps the success value to a new value, propagating failures.
    /// </summary>
    public async Task<Result<TNewValue>> MapAsync<TNewValue>(Func<TValue, Task<TNewValue>> mapper)
    {
        return IsSuccess
            ? Result<TNewValue>.Success(await mapper(Value!))
            : Result<TNewValue>.Failure(Error!);
    }

    /// <summary>
    /// Chains this result with another operation that returns a Result, propagating failures.
    /// </summary>
    public Result<TNewValue> Bind<TNewValue>(Func<TValue, Result<TNewValue>> binder)
    {
        return IsSuccess ? binder(Value!) : Result<TNewValue>.Failure(Error!);
    }

    /// <summary>
    /// Asynchronously chains this result with another operation that returns a Result, propagating failures.
    /// </summary>
    public async Task<Result<TNewValue>> BindAsync<TNewValue>(
        Func<TValue, Task<Result<TNewValue>>> binder)
    {
        return IsSuccess ? await binder(Value!) : Result<TNewValue>.Failure(Error!);
    }

    // Implicit conversions for ergonomics
    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
