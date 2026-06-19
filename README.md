# Financial Throttle Service

## 1. Purpose

A backend service that reads financial data from SQL Server queue tables, processes data that meets specified conditions in parallel, and logs all operations to MongoDB.

## 2. Technology Stack

| Technology                | Usage                                              |
| ------------------------- | -------------------------------------------------- |
| **.NET 10**               | Backend framework                                  |
| **ASP.NET Core**          | Web API & dependency injection                     |
| **SQL Server**            | 3 databases (RAS_STAJ, RAS_STAJ107, RAS_STAJ32501) |
| **Entity Framework Core** | Database access (DB-First)                         |
| **MongoDB Atlas**         | Audit logging                                      |
| **REST API**              | HTTP endpoints + Swagger                           |
| **gRPC**                  | Worker-Api communication                           |
| **BackgroundService**     | Worker cycle (every 30 seconds)                    |

---

## 3. How to Run

### Step 1: Update Connection Strings

**Api/appsettings.Development.json:**

{
"ConnectionStrings": {
"DefaultConnection": "Server=YOUR_SERVER;Database=RAS_STAJ;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
"RasStaj107": "Server=YOUR_SERVER;Database=RAS_STAJ107;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
"RasStaj32501": "Server=YOUR_SERVER;Database=RAS_STAJ32501;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
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

````

### Step 2: Configure Multiple Startup Projects

**In Visual Studio:**
1. Right-click Solution → **Set Startup Projects**
2. Select **Multiple startup projects**
3. Mark as **Start:**
   - ✅ `FinancialThrottle.Apii`
   - ✅ `FinancialThrottle.Worker`
4. **Apply** → **OK**

**Via CLI (alternative):**
```bash
# Terminal 1
dotnet run --project FinancialThrottle.Worker

# Terminal 2
dotnet run --project FinancialThrottle.Apii
````

### Step 3: Run

```bash
Ctrl + F5  (Visual Studio)
```

**Access:**

- Swagger UI: http://localhost:5059
- API Status: http://localhost:5059/api/status
- API Queue: http://localhost:5059/api/queue

---

**Version:** 1.0.0 | **Date:** June 19, 2026
