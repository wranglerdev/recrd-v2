namespace Recrd.Application.Abstractions;

/// <summary>
/// Identidade do usuário Windows logado, usada para auditoria e logs (PRD §5).
/// Não há tela de login; a identidade vem do SO.
/// </summary>
public interface IUserContext
{
    string Username { get; }
    string DisplayName { get; }
    string Domain { get; }
}
