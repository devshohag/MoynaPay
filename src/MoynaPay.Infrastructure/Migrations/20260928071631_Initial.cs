using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoynaPay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "merchants");

            migrationBuilder.EnsureSchema(
                name: "moynapay");

            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.CreateTable(
                name: "api_credentials",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    secret_cipher = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    key_ring_id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_credentials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_pairing_tokens",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_pairing_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "merchants",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    msisdn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    support_msisdn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_events",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    actor = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    actor_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    from = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    to = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    detail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    msisdn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    charged_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fields_json = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    digit = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    call_attempts = table.Column<int>(type: "integer", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    trx_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    courier = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    tracking_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    callback_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    shipped_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    claimed_by = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    claimed_until = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    last_failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parser_templates",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<string>(type: "text", nullable: false),
                    pattern = table.Column<string>(type: "text", nullable: false),
                    field_map_json = table.Column<string>(type: "text", nullable: false),
                    is_credit = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    base_confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    hit_count = table.Column<long>(type: "bigint", nullable: false),
                    last_hit_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parser_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "request_nonces",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    nonce = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    seen_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request_nonces", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    calls = table.Column<bool>(type: "boolean", nullable: false),
                    payments = table.Column<bool>(type: "boolean", nullable: false),
                    courier = table.Column<bool>(type: "boolean", nullable: false),
                    plan = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_endpoints",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    secret_cipher = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    key_ring_id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    last_failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    last_delivered_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_endpoints", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: false),
                    account_type = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: true),
                    declared_monthly_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    merchant_id1 = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallets", x => x.id);
                    table.ForeignKey(
                        name: "FK_wallets_merchants_merchant_id1",
                        column: x => x.merchant_id1,
                        principalSchema: "merchants",
                        principalTable: "merchants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fingerprint = table.Column<string>(type: "text", nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    app_version = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_heartbeat_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    permission_state = table.Column<int>(type: "integer", nullable: false),
                    battery_percent = table.Column<int>(type: "integer", nullable: true),
                    network_type = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.id);
                    table.ForeignKey(
                        name: "FK_devices_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "moynapay",
                        principalTable: "wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_ref = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    charged_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    grace_until = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: true),
                    customer_email = table.Column<string>(type: "text", nullable: true),
                    customer_msisdn = table.Column<string>(type: "text", nullable: true),
                    redirect_url = table.Column<string>(type: "text", nullable: true),
                    callback_url = table.Column<string>(type: "text", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoices_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "moynapay",
                        principalTable: "wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "raw_events",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    device_received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    server_received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    dedupe_hash = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_raw_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_raw_events_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "moynapay",
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_claims",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_text = table.Column<string>(type: "text", nullable: false),
                    normalised_trx_id = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    client_ip = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_claims_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "moynapay",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_sessions",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expected_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_sessions_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "moynapay",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_sessions_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "moynapay",
                        principalTable: "wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parsed_transactions",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    trx_id = table.Column<string>(type: "text", nullable: false),
                    sender_msisdn = table.Column<string>(type: "text", nullable: true),
                    balance_after = table.Column<decimal>(type: "numeric", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parsed_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_parsed_transactions_parser_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "moynapay",
                        principalTable: "parser_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_parsed_transactions_raw_events_raw_event_id",
                        column: x => x.raw_event_id,
                        principalSchema: "moynapay",
                        principalTable: "raw_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_matches",
                schema: "moynapay",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parsed_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    strategy = table.Column<int>(type: "integer", nullable: false),
                    matched_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    operator_id = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_matches", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_matches_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "moynapay",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_matches_parsed_transactions_parsed_transaction_id",
                        column: x => x.parsed_transaction_id,
                        principalSchema: "moynapay",
                        principalTable: "parsed_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_credentials_key_id",
                schema: "merchants",
                table: "api_credentials",
                column: "key_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devices_wallet_id",
                schema: "moynapay",
                table: "devices",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_wallet_id",
                schema: "moynapay",
                table: "invoices",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchants_msisdn",
                schema: "merchants",
                table: "merchants",
                column: "msisdn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_events_order_id_at",
                schema: "orders",
                table: "order_events",
                columns: new[] { "order_id", "at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id_reference",
                schema: "orders",
                table: "orders",
                columns: new[] { "tenant_id", "reference" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id_status_created_at",
                schema: "orders",
                table: "orders",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_next_attempt_at",
                schema: "orders",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "delivered_at is null");

            migrationBuilder.CreateIndex(
                name: "ix_parsed_transactions_raw_event_id",
                schema: "moynapay",
                table: "parsed_transactions",
                column: "raw_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_parsed_transactions_template_id",
                schema: "moynapay",
                table: "parsed_transactions",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_claims_invoice_id",
                schema: "moynapay",
                table: "payment_claims",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_matches_invoice_id",
                schema: "moynapay",
                table: "payment_matches",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_matches_parsed_transaction_id",
                schema: "moynapay",
                table: "payment_matches",
                column: "parsed_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_sessions_invoice_id",
                schema: "moynapay",
                table: "payment_sessions",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_sessions_wallet_id",
                schema: "moynapay",
                table: "payment_sessions",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_events_device_id",
                schema: "moynapay",
                table: "raw_events",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_request_nonces_key_id_nonce",
                schema: "merchants",
                table: "request_nonces",
                columns: new[] { "key_id", "nonce" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_request_nonces_seen_at",
                schema: "merchants",
                table: "request_nonces",
                column: "seen_at");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_tenant_id",
                schema: "merchants",
                table: "subscriptions",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallets_merchant_id1",
                schema: "moynapay",
                table: "wallets",
                column: "merchant_id1");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_endpoints_tenant_id",
                schema: "merchants",
                table: "webhook_endpoints",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_credentials",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "device_pairing_tokens",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "order_events",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "payment_claims",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "payment_matches",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "payment_sessions",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "request_nonces",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "webhook_endpoints",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "parsed_transactions",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "parser_templates",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "raw_events",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "wallets",
                schema: "moynapay");

            migrationBuilder.DropTable(
                name: "merchants",
                schema: "merchants");
        }
    }
}
