# MoynaPay — rules for AI coding agents

Read this before every task. It applies to ChatGPT (Codex), Gemini and any other agent.

## The product
MoynaPay is ONE .NET 10 solution with ONE backend and ONE Postgres database: order confirmation for
e-commerce shops, with a payments module (bKash matching, ported from YoPay) and a voice module
(confirmation calls over Asterisk ARI, ported from YoVoiceAgent). A merchant subscribes to payments,
calls, courier or any mix; the API and app only expose what they subscribed to.

## Layout (do not rearrange)
- `src/MoynaPay.Domain` — entities and the order lifecycle. No packages.
- `src/MoynaPay.Application` — services, rules, ports (`Abstractions/Ports.cs`). No packages.
- `src/MoynaPay.Infrastructure` — stores (Memory + EF under `Persistence/`), HTTP, ARI.
- `src/MoynaPay.Api` — shop API `/v1/...` (signed) and, from stage C, app API `/app/v1/...`.
- `src/MoynaPay.Worker.Outbox|Payments|Voice` — background loops.
- `tests/MoynaPay.Check` — the assertion runner (`Check("name", () => bool)`), no test framework.

## Hard rules
1. No new NuGet packages. EF Core + Npgsql (behind `UsePostgres`) are the only ones allowed.
2. `TreatWarningsAsErrors` is on. The build must stay warning-free.
3. Everything that decides something lives above a port and runs on the in-memory stores.
4. Every entity derives from `BaseEntity`; the tenant query filter must cover it. Enums are stored as names.
5. A machine may confirm an order and may record a customer's keypress; it may NEVER decide to reject or
   cancel. Unclear, silent or failed cases go to `NeedsHuman` / a review queue.
6. Every action that touches the outside world (webhook, invoice, courier, call) is idempotent with a
   stable id. Retrying must be safe.
7. Do not rewrite ported YoPay / YoVoiceAgent code from memory; extend it.
8. Secrets are never stored or logged in plaintext.
9. Each task ends with: `dotnet run --project tests\MoynaPay.Check` → `all N checks passed`, with new
   checks for the new rules. Show that at least one new check fails when its rule is broken.
10. Schema changes come with a named migration (`dotnet ef migrations add <Name> ...`, see README).
11. Update `README.md` for anything a developer must know. Keep answers short: what changed, files, how verified.
