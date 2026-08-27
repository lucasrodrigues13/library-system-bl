using LibrarySystem.Application.Abstractions;

namespace LibrarySystem.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
