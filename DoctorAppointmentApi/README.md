# DoctorAppointmentApi

.NET 9 / ASP.NET Core Web API backend for the two React apps you uploaded
(patient-facing `frontend` app + `admin`/doctor dashboard app — the classic
doctor-appointment booking project). Built so **neither frontend needs any
code changes** — every endpoint path, request shape, and response JSON shape
matches what `AppContext.jsx`, `AdminContext.jsx`, and `DoctorContext.jsx`
already expect.

## Stack

- .NET 9, ASP.NET Core Web API
- EF Core 9 + SQL Server
- JWT authentication (three schemes — see "Auth model" below)
- Local image upload storage under `wwwroot/uploads`

## Project layout

```
DoctorAppointmentApi/
  Controllers/        AdminController, DoctorController, UserController
  Entities/            User, Doctor, Appointment (EF Core models)
  Dtos/                Request/response shapes, grouped by area
  Data/                ApplicationDbContext
  Services/            JWT issuing, password hashing, file storage,
                       slot booking helper, mock payment gateway
  Program.cs           DI, auth schemes, CORS, middleware pipeline
  appsettings.json     Connection string, JWT key, admin creds, CORS origins
```

## 1. Configure

Edit `appsettings.json` (or better, use `dotnet user-secrets` /
environment variables for anything sensitive):

- `ConnectionStrings:DefaultConnection` — your SQL Server instance
- `Jwt:Key` — replace with a long random secret (32+ chars)
- `Admin:Email` / `Admin:Password` — **seed values only**. On first run, if the
  `Admins` table is empty, a row is created from these two config values
  (password is hashed, not stored in plain text). After that, admin login is
  validated against the database — this config is no longer consulted, so
  changing it later has no effect unless you delete the row.
- `Payments:FrontendUrl` — should match wherever the patient app runs
  (`http://localhost:5173` by default)
- `Cors:AllowedOrigins` — already set to `5173` (frontend) and `5174` (admin),
  matching both apps' `vite.config.js`

## 2. Install EF Core tools & create the database

```bash
dotnet tool install --global dotnet-ef   # if you don't have it already
cd DoctorAppointmentApi
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> If you already ran a migration before the `Admin` table was added, run
> `dotnet ef migrations add AddAdminTable` and `dotnet ef database update`
> again instead of recreating `InitialCreate`.

## 3. Run

```bash
dotnet run
```

Swagger UI opens automatically in Development at `/swagger`. The API listens
on `http://localhost:4000` by default (see `Properties/launchSettings.json`).

## 4. Point the frontends at it

Both React apps read `import.meta.env.VITE_BACKEND_URL`. Create a `.env` file
in each app's root:

**frontend/.env**
```
VITE_BACKEND_URL=http://localhost:4000
VITE_RAZORPAY_KEY_ID=rzp_test_xxxxxxxxxxxx   # only needed if you wire up real Razorpay
```

**admin/.env**
```
VITE_BACKEND_URL=http://localhost:4000
```

Then `npm install && npm run dev` in each app as usual.

## Auth model

The frontends send tokens in plain custom headers, not
`Authorization: Bearer ...`:

| App              | Header    | Role   |
|------------------|-----------|--------|
| Patient frontend | `token`   | User   |
| Doctor dashboard | `dToken`  | Doctor |
| Admin dashboard  | `aToken`  | Admin  |

To support this without touching the frontend code, the API registers three
JWT bearer schemes (`UserScheme`, `DoctorScheme`, `AdminScheme`), each reading
its own header and rejecting tokens whose `role` claim doesn't match — so a
patient token can't be replayed as a doctor token even though all three are
signed with the same key. See `Services/AuthConstants.cs` and `Program.cs`.

## Endpoints implemented

All paths, verbs, and JSON field names match the existing frontend code
exactly (including quirks like `slots_booked`, `_id`, and nested
`address.line1/line2`).

**`/api/user`** — register, login, get-profile, update-profile (multipart),
book-appointment, appointments, cancel-appointment, payment-razorpay,
verifyRazorpay, payment-stripe, verifyStripe

**`/api/doctor`** — login, list (public), appointments, cancel-appointment,
complete-appointment, dashboard, profile, update-profile

**`/api/admin`** — login, add-doctor (multipart), all-doctors,
change-availability, appointments, cancel-appointment, dashboard

## ⚠️ Payments are stubbed — read before going live

`Services/PaymentGatewayService.cs` contains `MockPaymentGatewayService`,
which returns Razorpay-order-shaped and Stripe-checkout-URL-shaped responses
**without calling either provider**. This lets the booking flow work
end-to-end in local development with no payment credentials. Before
production:

1. Add the `Razorpay` and `Stripe.net` NuGet packages.
2. Implement `IPaymentGatewayService` for real, using your live API keys.
3. Swap the DI registration in `Program.cs`:
   `builder.Services.AddScoped<IPaymentGatewayService, MockPaymentGatewayService>();`
   → your real implementation.
4. For Razorpay specifically, also verify the HMAC signature
   (`razorpay_signature`) in `UserController.VerifyRazorpay` before trusting
   the payment — the stub currently trusts the order-id match alone.

## Notes on data modeling choices

- IDs are `int` in SQL but serialized as strings under `_id` in JSON
  (`[JsonPropertyName("_id")]`) so the frontend's Mongo-style `doc._id`
  usage keeps working unchanged.
- `Doctor.SlotsBookedJson` stores the `{ "5_8_2026": ["10:30 AM", ...] }` map
  as JSON text in a single column — same shape the frontend already builds
  and reads client-side, just persisted server-side instead of trusting the
  client.
- Appointment `docData`/`userData` in API responses are built live from the
  current `Doctor`/`User` rows (via `MappingExtensions`), not point-in-time
  snapshots — simpler than the original app's copy-on-booking approach, at
  the cost of past appointments showing a doctor's *current* profile info
  rather than what it was at booking time. Flag if you'd rather I add
  snapshotting back in.
