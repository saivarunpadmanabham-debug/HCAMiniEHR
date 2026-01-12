# HCA Mini EHR - Healthcare Management System
## Capstone Project: ASP.NET Core + EF Core + SQL Server + LINQ

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0.10-purple)

## 📋 Project Overview

HCA Mini EHR is a comprehensive healthcare management system built as a capstone project to demonstrate proficiency in:
- **Git & GitHub** workflows
- **SQL Server** database design and management
- **Entity Framework Core** for data access
- **LINQ** for complex queries and reporting
- **ASP.NET Core Razor Pages** for web UI

This mini Electronic Health Records (EHR) system manages patients, appointments, and lab orders with full CRUD operations, advanced reporting, database triggers, and stored procedures.

---

## 🎯 Project Requirements Met

### ✅ Core Requirements
- [x] **EF Core Models** - Patient, Appointment, LabOrder, AuditLog with relationships
- [x] **Migrations** - Initial migration creating Healthcare schema tables
- [x] **CRUD Services** - PatientService, AppointmentService, LabOrderService
- [x] **LINQ Reporting** - 3 comprehensive reports using Where, GroupBy, Join, OrderBy
- [x] **DB Audit Trigger** - Logs INSERT, UPDATE, DELETE on Appointment table
- [x] **Stored Procedure** - `usp_CreateAppointment` invoked from C#

### ✅ Deliverables
- [x] GitHub repository with feature branches and Pull Requests
- [x] Database tables in `[Healthcare]` schema
- [x] 12 patient records, 8 appointments, 8 lab orders (seed data)
- [x] Trigger logging changes to AuditLog table
- [x] Stored procedure for appointment creation
- [x] Running web application with Razor Pages UI
- [x] 3 LINQ reports with filtering and aggregation

---

## 🏗️ Architecture & Design

### Database Schema

```
[Healthcare].[Patient]
├── PatientId (PK)
├── FirstName, LastName
├── DateOfBirth, Gender
├── Phone, Email, Address
└── CreatedDate

[Healthcare].[Appointment]
├── AppointmentId (PK)
├── PatientId (FK → Patient)
├── AppointmentDate, AppointmentTime
├── DoctorName, Reason, Status
└── CreatedDate

[Healthcare].[LabOrder]
├── LabOrderId (PK)
├── AppointmentId (FK → Appointment)
├── TestName, OrderDate
├── Status, Results
└── CompletedDate

[Healthcare].[AuditLog]
├── AuditLogId (PK)
├── TableName, Operation
├── RecordId, OldValue, NewValue
└── ChangedDate, ChangedBy
```

### Project Structure

```
HCAMiniEHR/
├── Models/                  # Entity models
│   ├── Patient.cs
│   ├── Appointment.cs
│   ├── LabOrder.cs
│   └── AuditLog.cs
├── Data/
│   ├── EhrDbContext.cs     # DbContext with Healthcare schema
│   └── Repositories/       # Generic repository pattern
│       ├── IRepository.cs
│       └── Repository.cs
├── Services/               # Business logic layer
│   ├── PatientService.cs
│   ├── AppointmentService.cs
│   └── LabOrderService.cs
├── Pages/                  # Razor Pages UI
│   ├── Patients/          # Patient CRUD pages
│   ├── Appointments/      # Appointment management
│   ├── LabOrders/         # Lab order management
│   └── Reports/           # LINQ reporting
└── SQL/                    # Database scripts
    ├── CreateAuditTrigger.sql
    ├── CreateAppointmentStoredProc.sql
    └── SeedData.sql
```

---

## 🚀 Setup Instructions

### Prerequisites
- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - LocalDB, Express, or Developer Edition
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone <your-repo-url>
   cd HCAMiniEHR
   ```

2. **Update Connection String**
   
   The connection string is already configured in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "EHRConnection": "Server=.;Database=EHR2;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```
   
   Modify if needed for your SQL Server instance.

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```
   
   This creates the `EHR2` database with all tables in the `Healthcare` schema.

5. **Run SQL Scripts (in SSMS)**
   
   Execute the following scripts in order:
   
   a. **Create Trigger** - `SQL/CreateAuditTrigger.sql`
   ```sql
   -- Creates trg_Appointment_Audit trigger
   ```
   
   b. **Create Stored Procedure** - `SQL/CreateAppointmentStoredProc.sql`
   ```sql
   -- Creates usp_CreateAppointment procedure
   ```
   
   c. **Seed Data** - `SQL/SeedData.sql`
   ```sql
   -- Inserts 12 patients, 8 appointments, 8 lab orders
   ```

6. **Run the Application**
   ```bash
   dotnet run
   ```
   
   Navigate to `https://localhost:5001` (or the URL shown in console)

---

## 🎨 Features

### 1. Patient Management
- **List** all patients with search and pagination
- **Create** new patient records with validation
- **Edit** patient information
- **Delete** patients (with cascade delete warning)
- **View Details** including appointment history

### 2. Appointment Scheduling
- **List** appointments with patient and doctor info
- **Create** appointments using **stored procedure** or EF Core
- **View Details** with associated lab orders
- Filter by status (Scheduled, Completed, Cancelled)

### 3. Lab Order Management
- **List** all lab orders with patient and test info
- **Create** lab orders linked to appointments
- **Update Status** (Pending → In Progress → Completed)
- Add test results when completing orders

### 4. LINQ Reports

#### Report 1: Pending Lab Orders
**LINQ Operations:** `Where`, `Include`, `OrderBy`, `Select`
```csharp
_context.LabOrders
    .Include(lo => lo.Appointment)
        .ThenInclude(a => a.Patient)
    .Where(lo => lo.Status == "Pending")
    .OrderBy(lo => lo.OrderDate)
    .Select(lo => new PendingLabOrderReport { ... })
```
Shows all pending lab tests with patient and appointment details.

#### Report 2: Patients Without Follow-Up
**LINQ Operations:** `Where` with `Any`, `Select`, `OrderBy`
```csharp
_context.Patients
    .Where(p => !p.Appointments.Any(a => a.AppointmentDate > today))
    .Select(p => new PatientWithoutFollowUpReport { ... })
    .OrderBy(p => p.PatientName)
```
Identifies patients needing follow-up appointments.

#### Report 3: Appointments by Month
**LINQ Operations:** `GroupBy`, `Select` with aggregations, `OrderByDescending`
```csharp
_context.Appointments
    .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
    .Select(g => new AppointmentsByMonthReport {
        TotalAppointments = g.Count(),
        ScheduledCount = g.Count(a => a.Status == "Scheduled"),
        CompletedCount = g.Count(a => a.Status == "Completed"),
        ...
    })
```
Monthly appointment statistics with completion rates.

---

## 🔧 Design Decisions

### 1. Repository Pattern
**Why?** Separates data access logic from business logic, making the code more testable and maintainable.

**Implementation:**
- Generic `IRepository<T>` interface for common CRUD operations
- `Repository<T>` base class with EF Core implementation
- Service layer (PatientService, etc.) uses repositories

### 2. Healthcare Schema
**Why?** Organizes database objects logically and follows healthcare industry best practices.

**Benefits:**
- Clear separation from other potential schemas (e.g., Security, Billing)
- Easier to manage permissions at schema level
- Professional database organization

### 3. Stored Procedure for Appointments
**Why?** Demonstrates ability to integrate T-SQL with C# and provides performance benefits.

**Advantages:**
- Validation logic in database (patient exists, date not in past)
- Atomic operation with transaction support
- Can be called from multiple applications
- Showcases `ExecuteSqlRaw` with output parameters

**Implementation:**
```csharp
await _context.Database.ExecuteSqlRawAsync(
    "EXEC [Healthcare].[usp_CreateAppointment] @PatientId, ..., @NewAppointmentId OUTPUT",
    parameters);
```

### 4. Audit Trigger
**Why?** Automatic change tracking without application code modifications.

**Trigger Logic:**
- Detects INSERT, UPDATE, DELETE operations
- Captures old and new values
- Records system user and timestamp
- Demonstrates database-level auditing

### 5. Bootstrap 5 UI
**Why?** Modern, responsive design with minimal custom CSS.

**Features:**
- Card-based dashboard with statistics
- Color-coded status badges
- Bootstrap Icons for visual clarity
- Mobile-responsive layout

---

## 📊 LINQ Operations Demonstrated

| Operation | Usage | Example |
|-----------|-------|---------|
| `Where` | Filtering | Pending lab orders, future appointments |
| `Select` | Projection | Custom DTOs for reports |
| `GroupBy` | Aggregation | Appointments by month |
| `Join` (via Include) | Related data | Lab orders with patients |
| `OrderBy` / `OrderByDescending` | Sorting | All reports |
| `Any` | Existence check | Patients without future appointments |
| `Count` | Aggregation | Statistics in all reports |
| `ThenInclude` | Multi-level joins | Lab orders → Appointments → Patients |

---

## 🧪 Testing & Verification

### Manual Testing Checklist

1. **CRUD Operations**
   - [ ] Create a new patient
   - [ ] Edit patient information
   - [ ] View patient details with appointments
   - [ ] Delete patient (verify cascade delete)
   - [ ] Create appointment using stored procedure
   - [ ] Create lab order for appointment
   - [ ] Update lab order status to "Completed"

2. **Database Verification (SSMS)**
   ```sql
   -- Verify tables exist
   SELECT TABLE_SCHEMA, TABLE_NAME 
   FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_SCHEMA = 'Healthcare';
   
   -- Check seed data
   SELECT COUNT(*) FROM [Healthcare].[Patient];  -- Should be 12
   SELECT COUNT(*) FROM [Healthcare].[Appointment];  -- Should be 8
   SELECT COUNT(*) FROM [Healthcare].[LabOrder];  -- Should be 8
   
   -- Test trigger
   UPDATE [Healthcare].[Appointment] 
   SET Status = 'Completed' 
   WHERE AppointmentId = 1;
   
   SELECT * FROM [Healthcare].[AuditLog] 
   ORDER BY ChangedDate DESC;  -- Should show UPDATE entry
   
   -- Test stored procedure
   EXEC [Healthcare].[usp_CreateAppointment] 
       @PatientId = 1,
       @AppointmentDate = '2026-02-15',
       @AppointmentTime = '14:30',
       @DoctorName = 'Dr. Test',
       @Reason = 'Testing SP';
   ```

3. **LINQ Reports**
   - [ ] Navigate to Reports page
   - [ ] Verify "Pending Lab Orders" shows correct data
   - [ ] Verify "Patients Without Follow-Up" identifies patients correctly
   - [ ] Verify "Appointments by Month" aggregates properly

---

## 📸 Screenshots

### Dashboard
![Dashboard with statistics cards and quick actions]

### Patient List
![Patient management with CRUD operations]

### Reports
![LINQ-powered reports with filtering]

---

## 🤝 Git Workflow

### Branching Strategy
```bash
# Main branch
main

# Feature branches
feature/patient-management
feature/appointment-scheduling
feature/lab-orders
feature/reporting
```

### Commit Message Convention
```
feat: Add patient CRUD operations
fix: Resolve foreign key constraint error
docs: Update README with setup instructions
refactor: Implement repository pattern
```

### Pull Request Process
1. Create feature branch from `main`
2. Make commits with descriptive messages
3. Push branch to GitHub
4. Create Pull Request with description
5. Request review from teammate
6. Merge after approval

---

## 📝 Submission Checklist

- [x] GitHub repo with merged PR
- [x] SSMS screenshots (tables, data, trigger log)
- [x] Running application (local)
- [x] 3 LINQ reports working
- [x] Stored procedure called from code
- [x] Trigger audit working
- [x] README with design decisions

---

## 🛠️ Technologies Used

- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8.0.10** - ORM
- **SQL Server** - Database
- **Bootstrap 5** - UI framework
- **Bootstrap Icons** - Icon library
- **Razor Pages** - View engine
- **LINQ** - Query language

---

## 👨‍💻 Author

**Capstone Project**  
Healthcare IT Academy  
January 2026

---

## 📄 License

This project is created for educational purposes as part of the HCA capstone project.

---

## 🔗 Additional Resources

- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/)
- [LINQ Query Syntax](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [SQL Server Triggers](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-trigger-transact-sql)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/)
