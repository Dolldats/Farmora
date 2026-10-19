# Farmora — Farm Nigeria Backend

Two-sided agricultural marketplace API. Farmers publish products and manage fulfillment; customers discover, basket/wishlist, order, pay, track delivery, and review. Admins operate trust, catalog, users, promotions, reports, and analytics.

## Amendment 1.1 (Oct 2026, approved — also patched into the requirements .docx)
- **Runtime: .NET 10** (doc v1.0 said .NET 9).
- **Persistence: PostgreSQL via Npgsql + EF Core** (doc v1.0 said MySQL). Migrations remain the source of schema change.
- Everything else in `Farm_Nigeria_Backend_Implementation_Requirements.docx` is unchanged.

## Stack
ASP.NET Core Web API (.NET 10), EF Core + Npgsql (PostgreSQL), JWT access + rotating refresh tokens, FluentValidation, Swagger + JWT, health checks, problem-details errors.

## Projects
- `Farmora.APi` — controllers (`api/v1`), `Middleware/ExceptionMiddleware`, `Extensions` (auth/policies/CORS/health/Swagger), `Infrastructure/HttpCurrentUser`.
- `Farmora.Application` — DTOs, `Interfaces/{Services,Repositories}`, `Common` (PagedResult, ICurrentUser), validators.
- `Farmora.Domain` — entities, enums, `Common/Entity` (GUID ids, UTC timestamps, `RowVersion` concurrency).
- `Farmora.Infastructure` (spelling kept for compat) — `Data` (DbContext + 13 configuration files + migrations), `Repositories`, `Services`, `Identity` (JwtOptions/JwtTokenService).

## Module coverage (all 27 doc modules)
User, Auth, Authorization (policies: CustomerOnly/FarmerOnly/VerifiedFarmer/AdminOnly/ResourceOwnerOrAdmin), Farmer, Customer (profile-only delete, `Restrict` on User FK), Category, Product, ProductImage, Search/Filter, Cart (one active/customer), Wishlist (idempotent), Order (snapshot + idempotency-key + status history), OrderItem, Payment (intent + idempotent webhooks + refunds), Address (owned + order snapshot), Delivery (method/fee/tracking/timeline), Review (verified-purchase, one-per-item, moderation), Report, Notification, Admin, FarmerVerification (+documents, publishing gate, expiry), Dashboard (customer/farmer/admin), Files (MIME/size-checked local store `wwwroot/uploads`, storage keys in DB), Messaging (conversations/participants/messages + block/rate-limit), Following/Favorites, Promotions (coupons/windows/limits/redemption audit), Analytics (events + KPIs).

## Key rules enforced
- Customer/Farmer are profiles of User; deleting a Customer only deactivates the profile, never the User/UserRoles.
- Account endpoints resolve identity from the JWT (`/users/me`, `/customers/me`, `/farmers/me`); clients never submit arbitrary UserIds.
- Prices/totals/discounts/stock computed server-side; order items snapshot commercial facts; out-of-stock checkout creates no partial order.
- Checkout + payment webhooks idempotent (`Idempotency-Key`, unique `PaymentEvent.EventId`).
- Product edits never mutate order history.

## Run
```powershell
# connection + dev JWT secret live in Farmora.APi/appsettings.Development.json
dotnet build Farmora.slnx
dotnet ef database update --project Farmora.Infastructure --startup-project Farmora.APi
dotnet run --project Farmora.APi
# swagger: https://localhost:<port>/swagger ; health: /health/live /health/ready
```

## API surface (`/api/v1`)
`auth/*`, `users/me`, `customers/me`, `farmers/me`, `categories`, `products` (+`/farmer/products`), `search/products`, `cart(/items)`, `wishlist`, `addresses/me`, `orders(/quote,/me,/{id}/cancel)`, `orders/{id}/payments` + `payments/webhooks`, `deliveries`, `reviews`, `reports`, `notifications`, `verifications`, `conversations`, `follows`, `promotions`, `analytics`, `admin/*`.

## Notes / next hardening
- Dev JWT secret in config is dev-only; use a secret manager in staging/prod.
- Payment provider is a stub adapter + signature-checked webhook shape; plug a real gateway behind `IPaymentGateway`.
- Known typo: `Farmora.Infastructure` (missing “r”) kept intentionally to avoid a rename churn; rename when convenient.
- `Product.MinRating` filter and realtime SignalR, container/CI files are still open follow-ups.
