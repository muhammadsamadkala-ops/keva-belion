# KIVA BELION - Cash on Delivery orders: what exists and what is still needed

STATUS: the C# code in this document was written WITHOUT a .NET SDK available. It has NOT been compiled or run by the author.
Build and test it on your PC first (`dotnet build`, `dotnet test`) before it goes anywhere near the live site.

## The contract (website -> API)
`POST {API}/api/v1/orders`, header `Idempotency-Key: <uuid>` (same key = same order, never a duplicate).
Body (no prices are sent; the server looks them up): customer {fullName, phone, email}, shipping {line1, city}, note,
paymentMethod "CashOnDelivery", expectedTotal, items [{productSlug, quantity}].
Answers: 201 order created, 200 same Idempotency-Key seen before (original order returned), 400 field errors,
409 product unavailable / price changed, 429 too many attempts, 503 ordering switched off. The website shows a confirmation
ONLY for 200/201 with a real `orderNumber` in the body.

## What was added to the backend
- `Orders/OrderRequestValidator.cs`: server-side validation (name, Pakistani phone formats, email, address >= 10 chars, city, items 1-20, quantity 1-99, only Cash on Delivery).
- `Orders/OrderPricing.cs`: the only place money is calculated (subtotal + shipping; prices come from the Products table).
- `Orders/OrderNumberGenerator.cs`: references such as KB-7F3K9Q2M.
- `Orders/OrderService.cs`: validates, de-duplicates by Idempotency-Key, loads products + prices from the database, checks stock only if an inventory row exists, refuses if `expectedTotal` differs from the server total, upserts the customer by email, saves Order + OrderItems with the address copied in.
- `Controllers/OrdersController.cs`: the public endpoint (rate limited, `checkout` policy, default 10/min per IP).
- `Controllers/AdminOrdersController.cs`: staff-only `GET /api/v1/admin/orders`, `GET .../{publicId}`, admin-only `PATCH .../{publicId}/status`.
- `Common/StartupChecks.cs`: live orders are now allowed in any environment, but only when `Cors:AllowedOrigins` is not empty. Online payments are still refused at startup.
- `scripts/seed-products.sql`: loads the 8 website products (ids, names, prices copied from data.js). The API cannot sell anything until this is run.
- Tests: `tests/KivaBelion.API.Tests/OrderTests.cs` (validator, pricing, order numbers, 503 while off, admin endpoints need sign-in).

## Settings the live API needs (host environment variables, never in files)
- `Features__OrdersEnabled=true` (default is false: the endpoint answers 503 until you switch it on)
- `Features__TestMode=false` (so saved orders are marked as real; while true they are flagged IsTestOrder)
- `Cors__AllowedOrigins__0=https://muhammadsamadkala-ops.github.io` (the website address, no trailing slash)
- `Orders__ShippingFeePkr=0` (delivery charge in PKR; 0 until you decide the policy)
- `Features__OnlinePaymentsEnabled=false` (must stay false)

## How orders are received and managed today
There is NO admin screen and NO notification. Orders are rows in the database. To see them:
1. Sign in with the admin endpoint and call `GET /api/v1/admin/orders` with the token, or
2. In Neon's SQL editor: `SELECT "OrderNumber","Status","Total","ShipRecipientName","ShipPhone","ShipLine1","ShipCity","CreatedAtUtc" FROM "Orders" ORDER BY "CreatedAtUtc" DESC;`
Nobody is told when an order arrives. Before launch you need at least an email or WhatsApp alert to the owner (not built).

## Missing before real orders can be accepted safely
1. The hosted API must actually run. It last returned a connection reset on /health/live and the cause was never found.
2. Build + run the tests of this code on your PC (it is uncompiled).
3. Deploy the new API, run the migrations already in the project, then run `scripts/seed-products.sql` on Neon.
4. Set the environment variables above, create the first admin (`scripts/create-admin.ps1`).
5. Decide the delivery charge and delivery times, then set `Orders__ShippingFeePkr` and update the FAQ/Terms.
6. Order alerts to the owner (email/WhatsApp) and, ideally, a small admin page for the orders list.
7. Stock: no inventory rows exist, so stock is not limited; bottles are not reserved when an order is placed.
8. Update Terms, Privacy and FAQ: they still say orders are sent by WhatsApp and are not stored on a server.
