using Microsoft.EntityFrameworkCore;
using Villsource.Modules.Oidc.Data.Entities;
using Villsource.Modules.Oidc.Domain;

namespace Villsource.Modules.Oidc.Data;

public sealed class OidcDbContext(DbContextOptions<OidcDbContext> options) : DbContext(options)
{
    public const string Schema = "oidc";

    public DbSet<OidcApplication> Applications => Set<OidcApplication>();
    public DbSet<OidcAuthorization> Authorizations => Set<OidcAuthorization>();
    public DbSet<OidcScope> Scopes => Set<OidcScope>();
    public DbSet<OidcToken> Tokens => Set<OidcToken>();
    public DbSet<Ping> Pings => Set<Ping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OidcDbContext).Assembly);
        modelBuilder.UseOpenIddict<OidcApplication, OidcAuthorization, OidcScope, OidcToken, string>();
        modelBuilder.Entity<OidcApplication>().ToTable("OpenIddictApplications", Schema);
        modelBuilder.Entity<OidcAuthorization>().ToTable("OpenIddictAuthorizations", Schema);
        modelBuilder.Entity<OidcScope>().ToTable("OpenIddictScopes", Schema);
        modelBuilder.Entity<OidcToken>().ToTable("OpenIddictTokens", Schema);
        base.OnModelCreating(modelBuilder);
    }
}