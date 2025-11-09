-- SmartScheduler Database Migration Script
-- Run this script directly on your hosted database if EF migrations fail
-- This creates all tables with proper schema and indexes

-- Drop existing tables if they exist (be careful in production!)
-- DROP TABLE IF EXISTS "RefreshTokens" CASCADE;
-- DROP TABLE IF EXISTS "DispatcherContractorLists" CASCADE;
-- DROP TABLE IF EXISTS "Assignments" CASCADE;
-- DROP TABLE IF EXISTS "Reviews" CASCADE;
-- DROP TABLE IF EXISTS "Jobs" CASCADE;
-- DROP TABLE IF EXISTS "Contractors" CASCADE;
-- DROP TABLE IF EXISTS "Customers" CASCADE;
-- DROP TABLE IF EXISTS "Users" CASCADE;

-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" serial PRIMARY KEY,
    "Email" varchar(256) NOT NULL UNIQUE,
    "PasswordHash" varchar(512) NOT NULL,
    "Role" integer NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "LastLoginAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users"("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_Role" ON "Users"("Role");
CREATE INDEX IF NOT EXISTS "IX_Users_IsDeleted" ON "Users"("IsDeleted");

-- Create Contractors table
CREATE TABLE IF NOT EXISTS "Contractors" (
    "Id" serial PRIMARY KEY,
    "UserId" integer NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Name" varchar(256) NOT NULL,
    "PhoneNumber" varchar(20) NOT NULL,
    "Location" varchar(512) NOT NULL,
    "Latitude" numeric(10,8) NOT NULL,
    "Longitude" numeric(11,8) NOT NULL,
    "TradeType" integer NOT NULL,
    "WorkingHoursStart" time NOT NULL,
    "WorkingHoursEnd" time NOT NULL,
    "AverageRating" numeric(3,2),
    "ReviewCount" integer NOT NULL DEFAULT 0,
    "TotalJobsCompleted" integer NOT NULL DEFAULT 0,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Contractors_IsActive_TradeType" ON "Contractors"("IsActive", "TradeType");
CREATE INDEX IF NOT EXISTS "IX_Contractors_IsActive" ON "Contractors"("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Contractors_Latitude" ON "Contractors"("Latitude");
CREATE INDEX IF NOT EXISTS "IX_Contractors_Longitude" ON "Contractors"("Longitude");

-- Create Customers table
CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" serial PRIMARY KEY,
    "UserId" integer NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Name" varchar(256) NOT NULL,
    "PhoneNumber" varchar(20) NOT NULL,
    "Location" varchar(512) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

-- Create Jobs table
CREATE TABLE IF NOT EXISTS "Jobs" (
    "Id" serial PRIMARY KEY,
    "CustomerId" integer NOT NULL REFERENCES "Customers"("Id") ON DELETE CASCADE,
    "JobType" integer NOT NULL,
    "Location" varchar(512) NOT NULL,
    "Latitude" numeric(10,8) NOT NULL,
    "Longitude" numeric(11,8) NOT NULL,
    "DesiredDateTime" timestamp with time zone NOT NULL,
    "EstimatedDurationHours" numeric(5,2) NOT NULL,
    "Description" varchar(2048) NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "AssignedContractorId" integer,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Jobs_Status_DesiredDateTime" ON "Jobs"("Status", "DesiredDateTime");

-- Create Assignments table
CREATE TABLE IF NOT EXISTS "Assignments" (
    "Id" serial PRIMARY KEY,
    "JobId" integer NOT NULL REFERENCES "Jobs"("Id") ON DELETE CASCADE,
    "ContractorId" integer NOT NULL REFERENCES "Contractors"("Id") ON DELETE CASCADE,
    "AssignedAt" timestamp with time zone NOT NULL,
    "AcceptedAt" timestamp with time zone,
    "DeclinedAt" timestamp with time zone,
    "StartedAt" timestamp with time zone,
    "CompletedAt" timestamp with time zone,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Assignments_ContractorId_Status" ON "Assignments"("ContractorId", "Status");

-- Create Reviews table
CREATE TABLE IF NOT EXISTS "Reviews" (
    "Id" serial PRIMARY KEY,
    "JobId" integer NOT NULL UNIQUE REFERENCES "Jobs"("Id") ON DELETE CASCADE,
    "ContractorId" integer NOT NULL REFERENCES "Contractors"("Id"),
    "CustomerId" integer NOT NULL REFERENCES "Customers"("Id"),
    "Rating" integer NOT NULL,
    "Comment" varchar(2048),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Reviews_JobId_Unique" ON "Reviews"("JobId");

-- Create DispatcherContractorLists table
CREATE TABLE IF NOT EXISTS "DispatcherContractorLists" (
    "Id" serial PRIMARY KEY,
    "DispatcherId" integer NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "ContractorId" integer NOT NULL REFERENCES "Contractors"("Id") ON DELETE CASCADE,
    "AddedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_DispatcherContractorLists_DispatcherId_ContractorId" ON "DispatcherContractorLists"("DispatcherId", "ContractorId");
CREATE INDEX IF NOT EXISTS "IX_DispatcherContractorLists_DispatcherId" ON "DispatcherContractorLists"("DispatcherId");
CREATE INDEX IF NOT EXISTS "IX_DispatcherContractorLists_ContractorId" ON "DispatcherContractorLists"("ContractorId");

-- Create RefreshTokens table
CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    "Id" serial PRIMARY KEY,
    "UserId" integer NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Token" varchar(256) NOT NULL UNIQUE,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_Token" ON "RefreshTokens"("Token");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_ExpiresAt" ON "RefreshTokens"("ExpiresAt");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId" ON "RefreshTokens"("UserId");

-- Record migrations in history table
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20251107000000_Initial', '9.0.10'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251107000000_Initial');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20251107120000_AddRefreshTokenEntity', '9.0.10'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251107120000_AddRefreshTokenEntity');

-- Verify all tables were created
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
ORDER BY table_name;

