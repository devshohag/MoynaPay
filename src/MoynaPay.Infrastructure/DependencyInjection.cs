using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoynaPay.Application.Abstractions;
using MoynaPay.Infrastructure.Memory;

namespace MoynaPay.Infrastructure;

/// <summary>
/// Everything that touches the outside world is registered here, in one place.
///
/// The hosts - the API and the three workers - do not choose a store, a clock or a
/// protector. They ask for MoynaPay, and what they get depends on configuration. That is
/// what lets the same four processes run against an in-memory store on a laptop and
/// Postgres on a server without a line of difference in them.
/// </summary>
public static class DependencyInjection
{
    public const string ConnectionName = "Postgres";

    /// <summary>
    /// True when no database is configured, so the service is running on the development
    /// fallback. Hosts say so out loud at start-up: a service holding a merchant's orders
    /// in memory looks healthy right up until it restarts.
    /// </summary>
    public static bool IsInMemory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return string.IsNullOrWhiteSpace(configuration.GetConnectionString(ConnectionName));
    }

    public static IServiceCollection AddMoynaPay(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IClock, SystemClock>();

        // Phase 5 replaces this with AES-GCM over a key ring. Until then a deployed
        // instance must refuse to start; see the guard in the hosts.
        services.AddSingleton<ISecretProtector, PlaintextSecretProtector>();

        if (IsInMemory(configuration))
        {
            services.AddScoped<ITenantContext, TenantContext>();

            services.AddSingleton<MemoryDatabase>();
            services.AddSingleton<IMerchantStore, MemoryMerchantStore>();
            services.AddSingleton<IOrderStore, MemoryOrderStore>();
            services.AddSingleton<INonceStore, MemoryNonceStore>();

            return services;
        }

#if USE_POSTGRES
        // Same ports, same registrations, different rows. Nothing above this line knows
        // which it got.
        return Persistence.PersistenceSetup.AddPostgres(
            services, configuration.GetConnectionString(ConnectionName)!);
#else
        throw new InvalidOperationException(
            "A Postgres connection string is set, but this build has no database in it. " +
            "Set the UsePostgres environment variable to true and rebuild, or remove " +
            "the connection string to run on " +
            "the in-memory fallback.");
#endif
    }
}
