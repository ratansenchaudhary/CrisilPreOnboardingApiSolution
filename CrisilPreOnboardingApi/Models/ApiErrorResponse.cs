namespace CrisilPreOnboardingApi.Models;

public sealed class ApiErrorResponse
{
    public string Code { get; init; } = "VALIDATION_FAILED";
    public string Message { get; init; } = "One or more validation errors occurred.";
    public List<ApiFieldError> Errors { get; init; } = new();
    public string TraceId { get; init; } = "";
}

public sealed class ApiFieldError
{
    public string Field { get; init; } = "";
    public string ErrorCode { get; init; } = "INVALID";
    public string Message { get; init; } = "";
}
