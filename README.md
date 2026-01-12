# HCA Mini EHR

A simple Electronic Health Records (EHR) system built with ASP.NET Core and SQL Server.

## Features

- Patient management
- Appointment scheduling
- Lab order tracking
- Audit logging

## Setup

1. Clone the repository
2. Update the connection string in `appsettings.json` if needed
3. Run migrations: `dotnet ef database update`
4. Execute SQL scripts in the `SQL` folder
5. Run the application: `dotnet run`

## Tech Stack

- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- Razor Pages

## Database

The application uses a SQL Server database with the following tables:
- Patient
- Appointment
- LabOrder
- AuditLog
