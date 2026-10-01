# MoynaPay

One product, one solution, two modules inside it: **payments** and **voice**.

There is no YoPay and no YoVoiceAgent any more. Their code lives here — ported, not
retyped, because the parser and the amount allocator are running code with real messages
behind them and rewriting them from memory would have thrown that away.

## The shape

```
MoynaPay.sln
  src/
    MoynaPay.Domain              orders, merchants, payments, voice
    MoynaPay.Application         services, signing, the parser, the call rules
    MoynaPay.Infrastructure      stores, ARI client — everything outside
    MoynaPay.Api                 the shop API, and later the app API
    MoynaPay.Worker.Outbox       delivers results to shops
    MoynaPay.Worker.Payments     turns bKash notifications into paid orders
    MoynaPay.Worker.Voice        rings customers, brings back the keypress
  tests/
    MoynaPay.Check
```

One solution, one database. The workers run as their own processes because each is a
queue being drained or a stream being listened to, not a request being answered - but
they are the same codebase, deployed together.

The three workers exist and start; their loops are filled in phases 2, 4 and 5. They are
here now so nothing has to be rearranged later, and so a `dotnet build` today builds the
same tree it will build in three months.

## Run it

```powershell
dotnet run --project tests\MoynaPay.Check
dotnet run --project src\MoynaPay.Api
```

The checks need no database, no network and no key. The API starts with one seeded
merchant (`mp_dev_demo`) so it can be driven the moment it is up.

## What is here

| Project | Packages | Built and run on a machine with no NuGet feed |
|---|---|---|
| `MoynaPay.Domain` | none | yes |
| `MoynaPay.Application` | none | yes |
| `MoynaPay.Infrastructure` | none yet | yes |
| `MoynaPay.Api` | none (framework only) | yes |
| `MoynaPay.Worker.*` | none (framework only) | yes |
| `tests/MoynaPay.Check` | none | yes — 100 assertions |

Everything that decides something lives above a port, so the whole service runs against
in-memory stores. That is not a stub for testing: it is what lets the rules be exercised
thousands of times a second instead of a few times a minute, and what lets anyone clone
this and see it work without SQL Server.

## The database

Postgres, behind a build switch, because EF Core and Npgsql are the only packages in the
solution and everything else must stay buildable without a feed:

```powershell
docker compose up -d

$env:UsePostgres = "true"
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5434;Database=moynapay;Username=moynapay;Password=moynapay"

dotnet build
dotnet ef migrations add Initial -p src\MoynaPay.Infrastructure -s src\MoynaPay.Api
dotnet ef database update  -p src\MoynaPay.Infrastructure -s src\MoynaPay.Api
```

The switch is an environment variable rather than `-p:UsePostgres=true` because `dotnet ef`
does not forward `-p:` to MSBuild: it would build the project without the provider in it
and then fail to find a context. MSBuild reads environment variables as properties, so one
`$env:` line reaches the build and both `ef` commands alike.

Without the switch and without a connection string, the same API runs on in-memory
stores. With both, the same ports are served by EF against real tables, and nothing above
`MoynaPay.Infrastructure` knows the difference.

Postgres rather than SQL Server because the payment matcher is built on
`FOR UPDATE SKIP LOCKED` and advisory locks. Those have SQL Server equivalents, but not
identical ones, and that is the one place where a subtle difference costs money.

### What the schema does that the code cannot

**`unique (tenant_id, reference) where is_deleted = false`.** `MemoryOrderStore` checks for
a clash in code, which is a check, not a guarantee: two requests arriving together can both
pass it. The index is the guarantee, and `EfOrderStore` treats a `23505` as "somebody else
won the race" rather than as a fault.

**A query filter on every entity**, applied by reflection rather than one at a time,
because applying it by hand works until somebody adds an entity and forgets — which is
exactly the day it matters. `ITenantContext` is set once, by the signing middleware, after
a request has been attributed; a null value means "no merchant in particular" and is what
lets the outbox worker drain every merchant's queue.

**Enums stored as names.** The number is smaller and wrong: insert a status into the
middle of an enum and every existing row means something else, with no migration that
could have caught it.

**`row_version` as the concurrency token**, on every entity, so two workers editing one
order cannot both win: the second one's update matches no row and raises, rather than
overwriting a decision it never saw.

This started as `xmin`, Postgres' own hidden row version, which costs no column at all.
The provider's helper for it (`UseXminAsConcurrencyToken`) is not in Npgsql 10, so it is
now one `bigint`, bumped in `SaveChanges`. It behaves identically and does not depend on a
provider version; the one thing it asks in return is that raw SQL updates bump it too.

**Nonces in the database, not in memory.** With two API instances behind a load balancer,
a captured request replayed against the other one would be accepted, because that instance
never saw the first.

### Not verified here

`Persistence/**` is written on a machine with no package feed, so it is not compiled here
and no migration has ever been generated from it. Everything outside that folder is checked
by the 100 assertions, which still run and still pass with the switch off.

The first build with the switch on reported one error, `UseXminAsConcurrencyToken` not
existing, and nothing else — which means the compiler read the rest of the folder and found
it sound. That error is fixed above, along with two things it had hidden behind it: the
startup project did not reference `Microsoft.EntityFrameworkCore.Design`, which `dotnet ef`
requires, and `-p:UsePostgres=true` was never reaching `dotnet ef` at all.

## What is NOT here yet

**The worker loops.** All three start and log; none of them does anything yet.

**Everything in `/app/v1`.** Stage C.

## The rules that matter

Three things are asserted over and over in the checks, because each one costs real money
when it breaks.

**`NeedsHuman` is not a cancellation.** The machine may confirm an order and it may record
a customer pressing 0, but it may never *decide* to reject one. Silence, a wrong key,
three unanswered calls — none of those is a customer saying no. They go to review and a
person decides. A worker that could reject would delete real sales every day, quietly.

**A rejection can be undone, by the merchant only.** A thumb lands on the wrong key often
enough that a machine-recorded "no" has to stay recoverable.

**A repeat does nothing.** `POST /v1/orders` with the same reference returns the order
that exists. Asking for a state an order is already in writes no event and sends no
notification. A shop's call can time out *after* we committed, and from their side that is
indistinguishable from failing before — so retrying has to be safe, and it is the right
move rather than a risk.

## The pact with YoPay

MoynaPay signs requests the same way YoPay does, so a shop that has integrated one has
written the hard part of the other and a single client library serves both.

That claim is now checked rather than asserted. `tests/MoynaPay.Check/fixtures/yopay-vectors.json`
was generated by YoPay's own server and is the same file YoPay's suite verifies against —
so the question answered here is not "does my HMAC work" but "do two independent
implementations produce the same bytes", which is the only version of the question that
matters when one of them lives in another repository.

Header names differ (`X-MoynaPay-Key`, not `X-YoPay-Key`) and that is fine: header names
are not inside the MAC.

The fixture includes a body with Bangla in it on purpose. UTF-8 is where two
implementations of one MAC usually part company, and the ASCII cases pass either way.

## The two modules

| came from | what came over |
|---|---|
| **YoPay** | the bKash parser and its corpus, `AmountAllocator`, `InvoiceWindow`, `ReferenceCode`, `TrxIdInput`, and the payment entities |
| **YoVoiceAgent** | the ARI client, the AI provider interfaces, the call entities |

Namespaces and the base class changed; nothing else did. The payment entities now derive
from the same `BaseEntity` as everything else, so one global query filter covers the whole
schema — two base classes would mean the filter catches one of them, and one forgotten
`WHERE` is one shop reading another's customers.

`BaseEntity.MerchantId` maps onto `TenantId` rather than being a second column. The tenant
here *is* the merchant, and renaming several hundred references to a concept that did not
change would have risked a lot to gain nothing.

What YoVoiceAgent did **not** have, and so had to be written: DTMF meaning, the calling
window, the redial policy and the script renderer. They are in
`Application/Voice/CallRules.cs`, and they are pure - decided from a keypress and a clock -
so the whole script runs thousands of times in a test without a phone ringing.

## Verified

```
all 100 checks passed
```

The checks were confirmed to be capable of failing, twice. Lower-casing the body digest in
the canonical string — one character — breaks five of them, including every YoPay vector.
Misspelling one word in a bKash template breaks the corpus replay and the field reading.
A suite that cannot fail is a suite that proves nothing.

and, against the running service:

```
create           201
same reference   200   one order, not two
landline number  400   refused at the door, not after three failed calls
cancel           200
cancel again     200   unchanged, no second event
nonce replayed   401
body edited      401
stale timestamp  401
unsigned         401
```
