namespace LibrarySystem.Domain.Common;

public sealed record Error(
    string Code,
    string Message,
    IReadOnlyList<ErrorDetail>? Details = null);
