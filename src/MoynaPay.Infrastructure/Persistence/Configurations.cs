using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoynaPay.Domain.Merchants;
using MoynaPay.Domain.Orders;

namespace MoynaPay.Infrastructure.Persistence;

/// <summary>
/// Shared shape for every table.
///
/// Enums are stored as their names, not their numbers. The number is smaller and faster
/// and wrong: insert a status into the middle of an enum and every existing row means
/// something else, silently, with no migration that could have caught it. Names do not
/// renumber themselves.
///
/// The concurrency token is not set here. It is BaseEntity.Version, attached to every
/// entity by the reflection pass in MoynaPayDbContext, for the same reason the query
/// filter is: doing it per configuration works until somebody adds an entity and forgets.
/// </summary>
internal static class Common
{
    public static void Standard<T>(EntityTypeBuilder<T> builder, string schema) where T : class =>
        builder.Metadata.SetSchema(schema);
}

internal sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        Common.Standard(builder, "merchants");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Msisdn).HasMaxLength(20).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.SupportMsisdn).HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        // A merchant logs in with this number, so two merchants cannot share one.
        builder.HasIndex(x => x.Msisdn).IsUnique();
    }
}

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        Common.Standard(builder, "merchants");

        builder.Property(x => x.Plan).HasMaxLength(40).IsRequired();

        // One row per merchant. Two would mean the pipeline's shape depends on which one
        // a query happened to read first.
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}

internal sealed class ApiCredentialConfiguration : IEntityTypeConfiguration<ApiCredential>
{
    public void Configure(EntityTypeBuilder<ApiCredential> builder)
    {
        Common.Standard(builder, "merchants");

        builder.Property(x => x.KeyId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SecretCipher).HasMaxLength(512).IsRequired();
        builder.Property(x => x.KeyRingId).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(120).IsRequired();

        // Looked up on every signed request, before the merchant is known - so this index
        // is global, not per merchant.
        builder.HasIndex(x => x.KeyId).IsUnique();

        builder.Ignore(x => x.IsActive);
    }
}

internal sealed class WebhookEndpointConfiguration : IEntityTypeConfiguration<WebhookEndpoint>
{
    public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
    {
        Common.Standard(builder, "merchants");

        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SecretCipher).HasMaxLength(512).IsRequired();
        builder.Property(x => x.KeyRingId).HasMaxLength(40).IsRequired();
        builder.Property(x => x.LastFailureReason).HasMaxLength(500);

        builder.HasIndex(x => x.TenantId);
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        Common.Standard(builder, "orders");

        builder.Property(x => x.Reference).HasMaxLength(80).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Msisdn).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Digit).HasMaxLength(4);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.TrxId).HasMaxLength(64);
        builder.Property(x => x.Courier).HasMaxLength(40);
        builder.Property(x => x.TrackingCode).HasMaxLength(80);
        builder.Property(x => x.CallbackUrl).HasMaxLength(500);
        builder.Property(x => x.ClaimedBy).HasMaxLength(120);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();

        // Money. 18,2 rather than a float, because a float cannot hold 1250.10 exactly
        // and a payment that is out by a hundredth matches nothing.
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ChargedAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.Property(x => x.FieldsJson).HasColumnType("jsonb");

        // The index that makes POST /v1/orders idempotent under load.
        //
        // MemoryOrderStore checks for a clash in code; that is a check, not a guarantee -
        // two requests arriving at once can both pass it. This is the guarantee. Filtered
        // on IsDeleted so a merchant can reuse a reference after removing an order.
        builder.HasIndex(x => new { x.TenantId, x.Reference })
            .IsUnique()
            .HasFilter("is_deleted = false");

        // The worker's question: what is open and due attention, oldest first.
        builder.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });

        builder.Ignore(x => x.MerchantId);
    }
}

internal sealed class OrderEventConfiguration : IEntityTypeConfiguration<OrderEvent>
{
    public void Configure(EntityTypeBuilder<OrderEvent> builder)
    {
        Common.Standard(builder, "orders");

        builder.Property(x => x.Type).HasMaxLength(60).IsRequired();
        builder.Property(x => x.ActorName).HasMaxLength(120);
        builder.Property(x => x.Detail).HasMaxLength(1000);
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb");

        builder.Property(x => x.Actor).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.From).HasConversion<string>().HasMaxLength(24);
        builder.Property(x => x.To).HasConversion<string>().HasMaxLength(24);

        // The timeline, in order, for one order. Read on every order detail screen.
        builder.HasIndex(x => new { x.OrderId, x.At });
    }
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        Common.Standard(builder, "orders");

        builder.Property(x => x.EventType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.LastFailureReason).HasMaxLength(500);

        // What the dispatcher asks for, every few seconds, forever: undelivered and due.
        // Partial so the index stays small - delivered rows are the overwhelming majority
        // and none of them is ever read by this query again.
        builder.HasIndex(x => x.NextAttemptAt)
            .HasFilter("delivered_at is null");
    }
}
