namespace LibrarySystem.Domain.Common;

public sealed record ErrorDetail(Guid BookId, string Title, int Available);
