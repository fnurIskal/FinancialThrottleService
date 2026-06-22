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
