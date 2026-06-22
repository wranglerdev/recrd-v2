using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Recrd.Application.Abstractions;
using Recrd.Domain.Entities;
using Recrd.Infrastructure.Data;

namespace Recrd.Infrastructure.Tests.Data;

public sealed class AuditStampingTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"recrd-audit-{Guid.NewGuid():N}.db");
    private readonly IUserContext _user;

    public AuditStampingTests()
    {
        _user = Substitute.For<IUserContext>();
        _user.Username.Returns("DOMAIN\\jose.silva");
    }

    private RecrdDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<RecrdDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        var ctx = new RecrdDbContext(options, _user);
        ctx.Database.Migrate();
        return ctx;
    }

    [Fact]
    public void Stamps_created_fields_on_insert_and_updated_fields_on_change()
    {
        var id = Guid.NewGuid();
        using (var ctx = NewContext())
        {
            ctx.Projects.Add(new Project { Id = id, Name = "XYZ" });
            ctx.SaveChanges();

            var saved = ctx.Projects.Single();
            saved.CreatedBy.Should().Be("DOMAIN\\jose.silva");
            saved.CreatedAt.Should().NotBe(default);
            saved.UpdatedAt.Should().BeNull();
        }

        using (var ctx = NewContext())
        {
            var project = ctx.Projects.Single(p => p.Id == id);
            project.Name = "XYZ v2";
            ctx.SaveChanges();

            project.UpdatedBy.Should().Be("DOMAIN\\jose.silva");
            project.UpdatedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task SaveChangesAsync_also_stamps_audit()
    {
        await using var ctx = NewContext();
        ctx.Projects.Add(new Project { Name = "Async" });
        await ctx.SaveChangesAsync();

        ctx.Projects.Single().CreatedBy.Should().Be("DOMAIN\\jose.silva");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}
