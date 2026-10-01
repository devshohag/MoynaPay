using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoynaPay.Application.Abstractions;
using MoynaPay.Domain.Merchants;

namespace MoynaPay.Infrastructure.Persistence;

internal sealed class RequestNonceConfiguration : IEntityTypeConfiguration<RequestNonce>
{
    public void Configure(EntityTypeBuilder<RequestNonce> builder)
    {
        Common.Standard(builder, "merchants");

        // The schema is repeated here, and it has to be: ToTable(name) on its own does
        // not leave the schema alone, it resets it to the default. Without the second
        // argument this table is created as moynapay.request_nonces while EfNonceStore
        // inserts into merchants.request_nonces - which builds, migrates, and then fails
        // on every signed request the service ever receives.
        builder.ToTable("request_nonces", "merchants");

        builder.Property(x => x.KeyId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Nonce).HasMaxLength(120).IsRequired();

        // The index that makes a signature usable exactly once. A timestamp window alone
        // does not stop replay: inside those five minutes the same signed request can be
        // sent again and again.
        builder.HasIndex(x => new { x.KeyId, x.Nonce }).IsUnique();

        // Swept by age; nothing older than the tolerance can be replayed anyway.
        builder.HasIndex(x => x.SeenAt);
    }
}

public static class PersistenceSetup
{
    public static IServiceCollection AddPostgres(
        this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ITenantContext, TenantContext>();

        services.AddDbContext<MoynaPayDbContext>(options => options.UseMoynaPay(connectionString));

        services.AddScoped<IMerchantStore, EfMerchantStore>();
        services.AddScoped<IOrderStore, EfOrderStore>();
        services.AddScoped<INonceStore, EfNonceStore>();

        return services;
    }

    /// <summary>
    /// One place where the provider is configured, used by the running service and by
    /// `dotnet ef` alike.
    ///
    /// It was two places, and the first migration proved why that is wrong: the history
    /// table was named here and not in the design-time factory, so `dotnet ef` wrote its
    /// bookkeeping to public."__EFMigrationsHistory" while the service would have looked
    /// for moynapay.__migrations, found nothing, and believed no migration had ever run.
    /// </summary>
    internal static void UseMoynaPay(this DbContextOptionsBuilder options, string connectionString)
    {
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__migrations", "moynapay");

            // Transient faults happen: a failover, a restart, a network blip. What
            // this must NOT do is hide a user-initiated transaction - the retrying
            // strategy forbids those, and every place that needs one wraps the work
            // in CreateExecutionStrategy().ExecuteAsync instead. That was learned the
            // hard way in YoPay, where messages sat in Claimed forever.
            npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null);
        });
    }
}

/// <summary>
/// Used only by `dotnet ef`, which has to build a context without starting the app.
///
/// The connection string comes from the environment rather than a default that looks
/// plausible. A default that lies costs more than it saves: YoPay once spent an evening
/// on a design-time password that was wrong but looked deliberate, and the error it
/// produced said "28P01 password authentication failed" with no hint of where the
/// password had come from.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MoynaPayDbContext>
{
    public MoynaPayDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__Postgres before running dotnet ef. " +
                "There is no default: a wrong one that looks right is worse than none.");

        var options = new DbContextOptionsBuilder<MoynaPayDbContext>();
        options.UseMoynaPay(connectionString);

        return new MoynaPayDbContext(options.Options, new TenantContext());
    }
}
