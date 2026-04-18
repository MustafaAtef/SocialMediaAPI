using System.Diagnostics.CodeAnalysis;
using SocialMedia.Core.Errors;

namespace SocialMedia.Core.Abstractions;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    Error Error { get; }
    IReadOnlyList<Error> Errors { get; }
}

public class Result : IResult
{
    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
        Errors = [error];
    }

    protected internal Result(IReadOnlyList<Error> errors)
    {
        if (errors is null || errors.Count == 0)
            throw new InvalidOperationException();

        IsSuccess = false;
        Errors = errors;
        Error = errors[0];
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public IReadOnlyList<Error> Errors { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result ValidationFailure(IReadOnlyList<Error> errors) => new(errors);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static Result<TValue> ValidationFailure<TValue>(IReadOnlyList<Error> errors) => new(default, errors);

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    protected internal Result(TValue? value, IReadOnlyList<Error> errors)
        : base(errors)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}