namespace Recrd.Domain.Entities;

/// <summary>
/// Base de auditoria — todos os objetos têm rastreabilidade (PRD §16).
/// Os campos são preenchidos automaticamente no SaveChanges.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
