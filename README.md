 > Güvenli yapılandırma ve public paylaşım öncesi kalan işlemler: [SECURITY.md](SECURITY.md). Gerçek parolaları aşağıdaki örnek dosyalara yazmayın; ortam değişkenleri kullanın.

# Financial Throttle Service

## 1. Purpose

A backend service that reads financial data from SQL Server queue tables, processes data that meets specified conditions in parallel, and logs all operations to MongoDB.

**Old System:** .NET Framework 4.5 Windows Service  
**New System:** .NET 10 ASP.NET Core (Single container, API + Worker integrated)

---

## 2. Technology Stack

| Technology                | Usage                                              |
| ------------------------- | -------------------------------------------------- |
| **.NET 10**               | Backend framework                                  |
| **ASP.NET Core**          | Web API & dependency injection                     |
| **SQL Server**            | 3 databases (RAS_STAJ, RAS_STAJ107, RAS_STAJ32501) |
| **Entity Framework Core** | Database access (DB-First)                         |
| **MongoDB**               | Audit logging                                      |
| **REST API**              | HTTP endpoints + Swagger                           |
| **BackgroundService**     | Worker cycle (every 30 seconds)                    |

---

## 3. Configuration

### Update Connection Strings

**FinancialThrottle.Apii/appsettings.Development.json:**

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:5059"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_IP,1433;Database=RAS_STAJ;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
    "RasStaj107": "Server=YOUR_SERVER_IP,1433;Database=RAS_STAJ107;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
    "RasStaj32501": "Server=YOUR_SERVER_IP,1433;Database=RAS_STAJ32501;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "financial_throttle_logs"
  },
  "ThrottleOptions": {
    "UseDummyData": false,
    "MaxParallelGroups": 5,
    "WorkerIntervalSeconds": 30
  }
}
```

---

## 4. Local Development (Visual Studio)

**In Visual Studio:**

1. Right-click Solution → **Set Startup Projects**
2. Select **Multiple startup projects**
3. Mark as **Start:**
   - ✅ `FinancialThrottle.Apii`
   - ✅ `FinancialThrottle.Worker`
4. **Apply** → **OK**

**Via CLI (alternative):**

````bash
# Terminal 1
dotnet run --project FinancialThrottle.Worker

# Terminal 2
dotnet run --project FinancialThrottle.Apii


**Access:**

- Swagger UI: http://localhost:5059
- API Status: http://localhost:5059/api/status
- API Queue: http://localhost:5059/api/queue

---

## 5. Docker Deployment

### Prerequisites

- Docker Desktop installed and running

### Files Required

- `Dockerfile` (in solution root)
- `docker-compose.yml` (in solution root)
- `appsettings.Development.json` (configured above)

### Build & Run

```bash
docker-compose up --build
````

**Services:**

- Frontend: http://localhost:3000
- Swagger UI: http://localhost:5059
- API Status: http://localhost:5059/api/status
- API Queue: http://localhost:5059/api/queue


### Useful Commands

```bash
# Run in background
docker-compose up --build -d

# View logs
docker-compose logs api

# Stop all containers
docker-compose down

# Stop and remove data
docker-compose down -v
```

---

## 6. Mobile App (Expo) — Local Setup

The backend is not deployed to a public server yet. Until it is, anyone running the mobile app must point it at a backend reachable on their **own local network** — the app cannot reach a backend running on someone else's machine.

### Prerequisites

- Node.js LTS + npm
- Backend running on your machine (Docker — see Section 5, or `dotnet run` — see Section 4)
- Expo Go app (quick testing) or an Android emulator/device

### Steps

1. Start the backend and confirm it's reachable at `http://localhost:5059/api/status`.
2. Find your machine's LAN IP:
   - Windows: `ipconfig` → IPv4 Address (WiFi adapter)
   - macOS/Linux: `ifconfig` or `ip addr`
3. Update `financial-throttle-mobile/constants/api.ts`:
   ```ts
   EXPO_PUBLIC_API_URL=http://localhost:5059/api
   ```
4. Make sure your phone/emulator and the backend machine are on the **same WiFi network**.
5. If Windows Firewall blocks the connection, add an inbound rule allowing TCP port 5059.
6. Install and run:
   ```bash
   cd financial-throttle-mobile
   npm install
   npm start        # Expo Go / dev client
   # or
   npm run android  # run on connected device/emulator
   ```

### Building a distributable APK

```bash
npm install -g eas-cli
eas login
eas build --platform android --profile preview
```

The `preview` profile in `eas.json` is configured to output a `.apk` (default profiles output `.aab`).

If installing the release APK on a device (not via Expo Go), your LAN IP must also be listed in `android/app/src/main/res/xml/network_security_config.xml` — Android blocks plain `http://` traffic by default in release builds.

### Note

This whole setup only works while the phone and the backend are on the same local network. Once the backend is deployed to a public server, replace `LOCAL_IP` in `constants/api.ts` with the production URL (ideally HTTPS) — at that point the network security config and LAN restrictions above no longer apply.

---

## 7. Testing

### What We Use

**xUnit** — unit test framework, via `FinancialThrottleService.Application.Tests` (project lives under `/tests/` in the solution). No database, HTTP, or mocking libraries are required for the current tests — the classes under test have no I/O dependencies.

### What Is Tested

The suite targets the pure business-logic classes in `FinancialThrottleService.Application/Logic`, since they contain the actual decision rules and have no external dependencies:

| Class                     | Covered by                          | What it verifies                                                                 |
| ------------------------- | ------------------------------------ | ---------------------------------------------------------------------------------- |
| `TemplateTableTypeConfig` | `TemplateTableTypeConfigTests.cs`    | `Resolve()` returns the correct required `TableTypeId` list per database/templateId (RAS_STAJ107, ms-source, unknown templateIds); `IsInflationTemplate()` correctly classifies known/unknown template IDs |
| `GroupRetryTracker`       | `GroupRetryTrackerTests.cs`          | Suspend threshold (9 failures = not suspended, 10th = suspended, 11th doesn't re-trigger); `RecordSuccess` clears state; `RecordWait`/`ClearWait`; `MarkForceSend` immediately un-suspends a group; `IsRetryDue` timing; `GetSuspendedGroups` parses the group key correctly |


### How to Run

```bash
dotnet test FinancialThrottleService.Application.Tests
```

Or run every test project in the solution:

```bash
dotnet test
```

To run a single test by name:

```bash
dotnet test --filter "FullyQualifiedName~RecordFailure_CalledTenthTime_SuspendsGroup"
```
