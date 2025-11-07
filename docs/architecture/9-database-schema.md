# 9. Database Schema

## 9.1 PostgreSQL Schema (DDL)

```sql
-- Users table (authentication)
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Role" VARCHAR(50) NOT NULL CHECK ("Role" IN ('Dispatcher', 'Customer', 'Contractor')),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" TIMESTAMP NULL
);

CREATE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Users_Role" ON "Users" ("Role");

-- Contractors table
CREATE TABLE "Contractors" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Name" VARCHAR(255) NOT NULL,
    "PhoneNumber" VARCHAR(20) NOT NULL,
    "Location" TEXT NOT NULL,
    "Latitude" DECIMAL(10, 8) NOT NULL,
    "Longitude" DECIMAL(11, 8) NOT NULL,
    "TradeType" VARCHAR(50) NOT NULL,
    "WorkingHoursStart" TIME NOT NULL,
    "WorkingHoursEnd" TIME NOT NULL,
    "AverageRating" DECIMAL(3, 2) NULL,
    "ReviewCount" INT NOT NULL DEFAULT 0,
    "TotalJobsCompleted" INT NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX "IX_Contractors_IsActive_TradeType" ON "Contractors" ("IsActive", "TradeType");

-- Additional tables: Customers, Jobs, Assignments, Reviews, DispatcherContractorLists
-- (See full schema in Section 9)
```

## 9.2 Key Indexes for Performance

- `IX_Contractors_IsActive_TradeType` - Recommendation query optimization
- `IX_Assignments_ContractorId_Status` - Availability check optimization
- `IX_Jobs_Status` + `IX_Jobs_DesiredDateTime` - Job list query optimization

---
