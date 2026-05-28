# GLMS — Global Logistics Management System

**Student:** Christian Emungu Bulabula  
**Student Number:** ST10538419  
**Module:** Enterprise Application Development (EAPD7111/w) — PROG7311  
**Repository:** [https://github.com/MrSolution07/GLMS-ST10538419](https://github.com/MrSolution07/GLMS-ST10538419)

ASP.NET Core MVC prototype for TechMove's logistics platform. Part 2 delivers a working monolith with SQL Server persistence, design pattern implementations, currency conversion, PDF contract uploads, and a 41-test xUnit suite.

---

## Features

- **Client management** — register and manage logistics clients
- **Contract management** — create contracts with service levels (Standard, Premium, Enterprise), PDF signed agreements, and status workflow (Draft, Active, Expired, OnHold)
- **Service requests** — raise requests against active contracts only; USD amounts convert to ZAR via live exchange rates
- **Search & filter** — filter contracts by title, status, and date range
- **Unit tests** — 41 automated tests covering currency conversion, file validation, workflow rules, and factory pattern behaviour
- **CI pipeline** — GitHub Actions runs build and tests on every push to `main`

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core MVC (.NET 10) |
| Database | SQL Server / LocalDB via Entity Framework Core (code-first) |
| Testing | xUnit, Moq |
| Frontend | Bootstrap 5, Bootstrap Icons |
| External API | [ExchangeRate-API](https://open.er-api.com) (USD → ZAR) |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **Windows:** SQL Server LocalDB (included with Visual Studio)
- **macOS / Linux:** SQL Server instance or Docker (LocalDB is Windows-only)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/MrSolution07/GLMS-ST10538419.git
cd GLMS-ST10538419
```

### 2. Configure the database

The default connection string in `GLMS.Web/appsettings.json` uses LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=GLMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true
```

For Docker or a remote SQL Server, update this string to match your environment.

### 3. Run the application

```bash
cd GLMS.Web
dotnet run
```

Migrations are applied automatically on startup. Open the URL shown in the terminal (typically `https://localhost:7xxx`).

**Visual Studio:** Open `GLMS.slnx`, set `GLMS.Web` as the startup project, and press **F5**.

### 4. Apply migrations manually (optional)

```bash
cd GLMS.Web
dotnet ef database update
```

---

## Running Tests

From the solution root:

```bash
dotnet test
```

Expected output:

```
Passed!  - Failed: 0, Passed: 41, Skipped: 0, Total: 41
```

**Visual Studio:** Test → Test Explorer → Run All Tests

| Test class | Tests | Covers |
|------------|-------|--------|
| `CurrencyServiceTests` | 12 | USD → ZAR conversion |
| `FileValidationTests` | 12 | PDF upload validation |
| `ContractWorkflowTests` | 10 | Active-contract business rule |
| `FactoryPatternTests` | 7 | Factory Method pattern |

---

## Solution Structure

```
GLMS/
├── GLMS.Web/                    # ASP.NET Core MVC application
│   ├── Controllers/             # Clients, Contracts, ServiceRequests, Home
│   ├── Data/                    # ApplicationDbContext (EF Core)
│   ├── Migrations/              # Code-first migration scripts
│   ├── Models/                  # Client, Contract, ServiceRequest
│   ├── Services/
│   │   ├── Factories/           # Factory Method pattern
│   │   ├── Observers/           # Observer pattern
│   │   └── Strategies/          # Strategy pattern
│   ├── ViewModels/
│   └── Views/
├── GLMS.Tests/                  # xUnit test project
├── .github/workflows/ci.yml     # GitHub Actions CI
├── PART2_EXPLANATION.txt      # Technical documentation
└── README.md
```

---

## Design Patterns

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Factory Method** | `Services/Factories/` | Creates contracts pre-configured per service level (Standard, Premium, Enterprise) |
| **Observer** | `Services/Observers/` | Notifies audit log and email observers when contract status changes |
| **Strategy** | `Services/Strategies/` | Encapsulates validation rules for service requests and PDF file uploads |

All patterns are registered in `Program.cs` via dependency injection.

---

## Key Business Rules

- Service requests can **only** be raised against contracts with **Active** status
- Signed agreements must be **PDF** files, max **10 MB**
- USD amounts are converted to ZAR using the live exchange rate at submission time
- The exchange rate used is stored on each service request for audit purposes

---

## Documentation

See [`PART2_EXPLANATION.txt`](PART2_EXPLANATION.txt) for the full technical write-up covering architecture, database design, workflow logic, and test coverage.

---

## License

Academic project — Rosebank College, 2026.
