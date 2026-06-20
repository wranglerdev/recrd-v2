using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Security.Principal;
using Recrd.Application.Abstractions;

namespace Recrd.Infrastructure.Auth;

/// <summary>
/// Identidade real do usuário Windows logado via <see cref="WindowsIdentity.GetCurrent"/>
/// (PRD §5). Registrada na DI somente no build Windows.
/// </summary>
// ponytail: kept in the cross-platform Infra project but Windows-guarded (SupportedOSPlatform),
// so it compiles on Linux and only ever runs on Windows. Split into a separate
// Recrd.Infrastructure.Windows project only if more Windows-only infra appears.
[SupportedOSPlatform("windows")]
[ExcludeFromCodeCoverage(Justification = "Windows-only; coberto pela etapa Windows da pipeline.")]
public sealed class WindowsUserContext : IUserContext
{
    private readonly WindowsIdentity _identity = WindowsIdentity.GetCurrent();

    // _identity.Name => "DOMAIN\\user"
    public string Username => _identity.Name;
    public string DisplayName => _identity.Name;
    public string Domain => _identity.Name.Split('\\') is [var domain, _] ? domain : _identity.Name;
}
