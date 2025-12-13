namespace QuikytLoader.Domain.Common;

public static class ResultExtensions
{
    /// <summary>
    /// Unwraps the value or throws an exception. Use sparingly - prefer explicit error handling.
    /// </summary>
    public static TValue Unwrap<TValue>(this Result<TValue> result)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Cannot unwrap failed result: {result.Error!.Message}");

        return result.Value!;
    }

    /// <summary>
    /// Gets the value if successful, otherwise returns the default value.
    /// </summary>
    public static TValue? ValueOr<TValue>(this Result<TValue> result, TValue? defaultValue = default)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Performs a side effect if the result is successful, then returns the result unchanged.
    /// Useful for logging or other actions that don't affect the result.
    /// </summary>
    public static Result<TValue> Tap<TValue>(
        this Result<TValue> result,
        Action<TValue> onSuccess)
    {
        if (result.IsSuccess)
            onSuccess(result.Value!);

        return result;
    }

    /// <summary>
    /// Performs a side effect if the result is a failure, then returns the result unchanged.
    /// Useful for logging errors.
    /// </summary>
    public static Result<TValue> TapError<TValue>(
        this Result<TValue> result,
        Action<Error> onError)
    {
        if (!result.IsSuccess)
            onError(result.Error!);

        return result;
    }

    /// <summary>
    /// Maps a Task&lt;Result&lt;T&gt;&gt; to Result&lt;TNew&gt; using a mapper function.
    /// </summary>
    public static async Task<Result<TNewValue>> MapAsync<TValue, TNewValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TNewValue> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Binds a Task&lt;Result&lt;T&gt;&gt; to another Result-returning operation.
    /// </summary>
    public static async Task<Result<TNewValue>> BindAsync<TValue, TNewValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Result<TNewValue>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    /// <summary>
    /// Asynchronously binds a Task&lt;Result&lt;T&gt;&gt; to another async Result-returning operation.
    /// </summary>
    public static async Task<Result<TNewValue>> BindAsync<TValue, TNewValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<Result<TNewValue>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    /// <summary>
    /// Performs a side effect if successful, then returns the result unchanged.
    /// Async version of Tap.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Action<TValue> onSuccess)
    {
        var result = await resultTask;
        return result.Tap(onSuccess);
    }

    /// <summary>
    /// Performs an async side effect if successful, then returns the result unchanged.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task> onSuccess)
    {
        var result = await resultTask;
        if (result.IsSuccess)
            await onSuccess(result.Value!);

        return result;
    }

    /// <summary>
    /// Combines multiple Result values, returning the first failure or success if all succeed.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (!result.IsSuccess)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Combines multiple Result&lt;T&gt; values, collecting all values or returning the first error.
    /// </summary>
    public static Result<IEnumerable<TValue>> Combine<TValue>(params Result<TValue>[] results)
    {
        var values = new List<TValue>();

        foreach (var result in results)
        {
            if (!result.IsSuccess)
                return Result<IEnumerable<TValue>>.Failure(result.Error!);

            values.Add(result.Value!);
        }

        return Result<IEnumerable<TValue>>.Success(values);
    }
}
