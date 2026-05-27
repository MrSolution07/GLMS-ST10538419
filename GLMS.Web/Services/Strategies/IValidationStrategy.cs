namespace GLMS.Web.Services.Strategies;

/// <summary>
/// Strategy pattern (GoF) — encapsulates a validation algorithm so it can be
/// swapped or extended without changing calling code.
/// </summary>
public interface IValidationStrategy<T>
{
    ValidationResult Validate(T subject);
}

public record ValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string message) => new(false, message);
}
