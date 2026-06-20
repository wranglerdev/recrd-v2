using Recrd.Application.Abstractions;

namespace Recrd.Infrastructure.Auth;

/// <summary>
/// Identidade fixa para desenvolvimento em Linux (PRD §5, §29).
/// Substituída por <see cref="WindowsUserContext"/> na DI do build Windows.
/// </summary>
public sealed class MockUserContext : IUserContext
{
    public string Username => "dev";
    public string DisplayName => "Linux Developer";
    public string Domain => "LOCAL";
}
