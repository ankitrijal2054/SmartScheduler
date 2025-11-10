-- Fix migration history for existing database
-- This script marks the InitialMigration as already applied without running it
-- Run this script against your production database before restarting the API

-- Insert the migration record if it doesn't exist
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251109194309_InitialMigration', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Verify the migration was recorded
SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251109194309_InitialMigration';

