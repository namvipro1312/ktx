# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

delegated: ASP.NET Core MVC (.NET 9), Entity Framework Core 9, and SQL Server, chosen to match the existing local SQL Server database and available .NET SDK.

## Users

- Dormitory administrators and staff manage buildings, rooms, beds, applications, approvals, contracts, services, repairs, rules, invoices, payments, and reports.
- Students manage their accommodation application and see their room, contract, service, and payment information.

## Product Purpose

Manage the student dormitory lifecycle in one web application, from room application and approval through occupancy, services, repairs, billing, and reporting. Success means staff can operate these workflows with consistent records and students can complete the tasks relevant to their stay.

## Positioning

No distinct market position or differentiating mechanism was supplied. Do not invent competitive claims.

## Operating Context

The initial feature scope is organized into five work groups: account/login and access control plus buildings/rooms/beds (sp1); room application and approval (sp2); contracts (sp3); services, repairs, and rules (sp4); payments and statistics (sp5). The existing SQL Server database is named `qlktx` on the developer's local machine.

## Capabilities and Constraints

- Role-based access for administrators/staff and students.
- Dormitory operations include room applications, approval, bed allocation, contracts, check-in/check-out, services and utilities, repairs, rules, invoices, payments, and reporting.
- ASP.NET Core MVC and SQL Server are the implementation target. The GitHub repository was empty when inspected.
- The current local SQL Server database is for development only; do not commit credentials or machine-specific connection strings.

## Evidence on Hand

- An existing SQL Server schema at `database/SQLServer/schema.sql` and a MySQL reference schema at `database/Legacy/schema_mysql.sql`.
- No supplied branding, logo, real resident records, or production metrics. Use clearly identified synthetic demo content and do not imply it is real.

## Product Principles

- Keep a traceable history for approvals, occupancy, and financial transactions.
- Make the next operational action and its current state clear.
- Restrict student access to that student's own records.
- Treat financial values and resident information as sensitive.
