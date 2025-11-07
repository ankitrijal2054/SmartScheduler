# Pre-Development Checklist

**Status:** ✅ Must complete ALL items before starting Epic 1

**Estimated Time:** 30-45 minutes

**Purpose:** Ensure all external dependencies, credentials, and local environment are properly configured before any development begins.

---

## Section 1: AWS Account & Infrastructure Setup

### ✅ 1.1 AWS Account Creation

**Task:** Create AWS account if not already existing.

**Steps:**

1. Visit https://aws.amazon.com
2. Click "Create an AWS Account"
3. Complete account setup:
   - Email address (preferably project-specific)
   - Password (strong, 16+ chars, stored in password manager)
   - AWS account name (e.g., "smartscheduler-prod")
   - Billing information (credit card required)
4. Verify email address
5. Complete phone verification

**Verification:**

- [ ] AWS account active and accessible
- [ ] Email verified
- [ ] Billing information on file

**Credentials to Save:**

- AWS Account ID: `________________`
- Root email: `________________`
- AWS region (default): `us-east-1`

---

### ✅ 1.2 IAM User & Access Key Configuration

**Task:** Create IAM user with programmatic access for local development.

**Steps:**

1. Log in to AWS Console with root account
2. Navigate to IAM → Users → Create User
3. User details:
   - User name: `smartscheduler-dev`
   - Enable programmatic access (✓)
   - Enable AWS Management Console access (optional)
4. Permissions:
   - Attach policies:
     - `AmazonEC2ContainerRegistryPowerUser` (for ECR)
     - `AmazonRDSFullAccess` (for RDS)
     - `AmazonS3FullAccess` (for S3)
     - `AWSSecretsManagerReadWrite` (for Secrets Manager)
     - `CloudFrontFullAccess` (for CDN)
     - `AppRunnerFullAccess` (for App Runner)
5. Create access key:
   - Access Key ID: `________________`
   - Secret Access Key: `________________` (download CSV, save securely)
6. Store credentials in environment variables or AWS credentials file:

```bash
# Linux/macOS: ~/.aws/credentials
[smartscheduler-dev]
aws_access_key_id = YOUR_ACCESS_KEY_ID
aws_secret_access_key = YOUR_SECRET_ACCESS_KEY
region = us-east-1
```

**Verification:**

- [ ] IAM user `smartscheduler-dev` created
- [ ] Programmatic access key generated
- [ ] Credentials stored locally in `~/.aws/credentials`
- [ ] Test: `aws sts get-caller-identity` returns user ARN

**Command to Test:**

```bash
aws sts get-caller-identity --profile smartscheduler-dev
# Output should show your IAM user ARN
```

---

### ✅ 1.3 AWS Secrets Manager Setup

**Task:** Set up Secrets Manager for storing sensitive configuration.

**Steps:**

1. Navigate to AWS Secrets Manager console
2. Create secret: `smartscheduler/dev/database-password`
   - Value: PostgreSQL password (generate strong password, 16+ chars)
   - Store securely in password manager
3. Create secret: `smartscheduler/dev/jwt-secret`
   - Value: Random 32-character string for JWT signing
   - Generate: `openssl rand -base64 32`
4. Create secret: `smartscheduler/dev/google-maps-api-key`
   - Value: (populated after Step 2: Google Maps API setup)
5. Note ARN for each secret (needed in application config)

**Secrets to Store:**

| Secret Name                              | Purpose                  | Value              |
| ---------------------------------------- | ------------------------ | ------------------ |
| `smartscheduler/dev/database-password`   | PostgreSQL connection    | `________________` |
| `smartscheduler/dev/jwt-secret`          | JWT token signing        | `________________` |
| `smartscheduler/dev/google-maps-api-key` | Distance/travel time API | `________________` |
| `smartscheduler/dev/aws-ses-email`       | Email sender address     | `________________` |

**Verification:**

- [ ] All 4 secrets created in Secrets Manager
- [ ] Accessible via AWS CLI: `aws secretsmanager get-secret-value --secret-id smartscheduler/dev/database-password --profile smartscheduler-dev`

---

## Section 2: Google Maps API Setup

### ✅ 2.1 Google Cloud Project Creation

**Task:** Set up Google Cloud project and enable Maps API.

**Steps:**

1. Visit https://console.cloud.google.com
2. Create new project:
   - Project name: `SmartScheduler`
   - Organization: (leave blank or select your organization)
3. Enable APIs:
   - Navigate to "APIs & Services" → "Library"
   - Search for "Distance Matrix API"
   - Click "Enable"
   - Search for "Maps JavaScript API"
   - Click "Enable"

**Verification:**

- [ ] Google Cloud project created
- [ ] Distance Matrix API enabled
- [ ] Maps JavaScript API enabled

---

### ✅ 2.2 API Key Generation

**Task:** Generate API key for Distance Matrix API.

**Steps:**

1. Navigate to "APIs & Services" → "Credentials"
2. Click "Create Credentials" → "API Key"
3. Copy API key (automatically generated)
4. Restrict API key:
   - Click "Restrict Key"
   - Application restrictions: Select "HTTP referrers (web sites)"
   - Website restrictions: Add your frontend domain (e.g., `https://smartscheduler.example.com`)
   - API restrictions: Select "Distance Matrix API" + "Maps JavaScript API"
5. Save API key in password manager

**API Key:** `________________`

**Store in AWS Secrets Manager:**

```bash
aws secretsmanager update-secret \
  --secret-id smartscheduler/dev/google-maps-api-key \
  --secret-string "YOUR_API_KEY" \
  --profile smartscheduler-dev
```

**Verification:**

- [ ] API key generated and restricted
- [ ] Stored in AWS Secrets Manager
- [ ] Test (from command line):

```bash
curl "https://maps.googleapis.com/maps/api/distancematrix/json?origins=New%20York&destinations=Los%20Angeles&key=YOUR_API_KEY"
# Should return distance data (not "API key not valid")
```

---

### ✅ 2.3 Set Up Google Maps Billing

**Task:** Enable billing for Google Maps API.

**Steps:**

1. Navigate to Google Cloud Console → "Billing"
2. Link billing account:
   - Create new billing account (if not existing)
   - Link payment method (credit card)
   - Set budget alerts (e.g., $50/month to prevent runaway costs)
3. Verify API access in Distance Matrix API documentation

**Verification:**

- [ ] Billing account active
- [ ] Payment method configured
- [ ] Budget alert set

**Note:** Distance Matrix API costs ~$5 per 1000 queries. MVP usage should be minimal (caching reduces cost).

---

## Section 3: GitHub Repository Setup

### ✅ 3.1 Create GitHub Repository

**Task:** Initialize GitHub repository for the project.

**Steps:**

1. Visit https://github.com (create account if needed)
2. Click "New repository"
3. Repository details:
   - Owner: Your GitHub account/organization
   - Repository name: `smartscheduler`
   - Description: "Intelligent field service marketplace - portfolio project"
   - Visibility: Public (portfolio demonstration)
   - Initialize with README (check)
4. Clone repository to local machine:

```bash
git clone https://github.com/YOUR_USERNAME/smartscheduler.git
cd smartscheduler
```

**Verification:**

- [ ] Repository created on GitHub
- [ ] Cloned to local machine
- [ ] Remote URL: `git remote -v` shows correct origin

---

### ✅ 3.2 Configure GitHub Secrets for CI/CD

**Task:** Store AWS credentials in GitHub for automated deployments.

**Steps:**

1. Navigate to repository → Settings → Secrets and Variables → Actions
2. Create secrets:
   - `AWS_ACCESS_KEY_ID`: (from Section 1.2)
   - `AWS_SECRET_ACCESS_KEY`: (from Section 1.2)
   - `AWS_REGION`: `us-east-1`
   - `DATABASE_PASSWORD`: (from Section 1.3)
   - `JWT_SECRET`: (from Section 1.3)
   - `GOOGLE_MAPS_API_KEY`: (from Section 2.2)

**Verification:**

- [ ] All secrets configured in GitHub
- [ ] No secrets committed to repository
- [ ] `.gitignore` includes sensitive files

---

## Section 4: Local Development Environment

### ✅ 4.1 Install Required Tools

**Task:** Install all development tools locally.

**Prerequisites:**

| Tool           | Purpose                  | Min Version | Install                                        |
| -------------- | ------------------------ | ----------- | ---------------------------------------------- |
| Git            | Version control          | 2.40+       | https://git-scm.com                            |
| .NET SDK       | Backend framework        | 8.0.21      | https://dotnet.microsoft.com/en-us/download    |
| Node.js        | Frontend package manager | 20.x LTS    | https://nodejs.org                             |
| npm            | Package manager          | 10.x        | Included with Node.js                          |
| PostgreSQL     | Local database           | 16.6        | https://www.postgresql.org/download            |
| Docker         | Containerization         | 27.x        | https://www.docker.com/products/docker-desktop |
| VS Code or IDE | Code editor              | Latest      | https://code.visualstudio.com                  |

**Verification:**

```bash
# Run these commands to verify installations
git --version              # git version 2.40.0 (or higher)
dotnet --version           # .NET 8.0.xxx
node --version             # v20.x.x
npm --version              # 10.x.x
psql --version             # PostgreSQL 16.x
docker --version           # Docker version 27.x
```

**Commands to Record Versions:**

- Git version: `________________`
- .NET version: `________________`
- Node version: `________________`
- PostgreSQL version: `________________`
- Docker version: `________________`

---

### ✅ 4.2 Configure PostgreSQL Locally

**Task:** Set up local PostgreSQL database for development.

**Steps (macOS/Linux):**

1. Install PostgreSQL:

   ```bash
   # macOS (using Homebrew)
   brew install postgresql@16
   brew services start postgresql@16

   # Linux (Ubuntu/Debian)
   sudo apt-get install postgresql postgresql-contrib
   sudo systemctl start postgresql
   ```

2. Create development database:

   ```bash
   psql -U postgres

   # In psql prompt:
   CREATE DATABASE smartscheduler_dev;
   CREATE USER smartscheduler_dev WITH PASSWORD 'YOUR_PASSWORD';
   ALTER ROLE smartscheduler_dev WITH CREATEDB;
   GRANT ALL PRIVILEGES ON DATABASE smartscheduler_dev TO smartscheduler_dev;
   \q
   ```

3. Verify connection:
   ```bash
   psql -U smartscheduler_dev -d smartscheduler_dev -h localhost
   \dt  # Should show no tables (empty database)
   \q
   ```

**Verification:**

- [ ] PostgreSQL service running
- [ ] Database `smartscheduler_dev` created
- [ ] User `smartscheduler_dev` has permissions
- [ ] Connection successful via psql

**Connection String for .NET:**

```
Server=localhost;Port=5432;Database=smartscheduler_dev;User Id=smartscheduler_dev;Password=YOUR_PASSWORD;
```

---

### ✅ 4.3 Configure Environment Variables

**Task:** Set up local environment variables for development.

**Steps:**

1. Create `.env.local` in repository root (never commit):

   ```bash
   # Backend Configuration
   DOTNET_ENVIRONMENT=Development
   DATABASE_CONNECTION_STRING="Server=localhost;Port=5432;Database=smartscheduler_dev;User Id=smartscheduler_dev;Password=YOUR_PASSWORD;"
   JWT_SECRET="YOUR_JWT_SECRET_FROM_SECRETS_MANAGER"
   GOOGLE_MAPS_API_KEY="YOUR_GOOGLE_MAPS_API_KEY"
   AWS_ACCESS_KEY_ID="YOUR_ACCESS_KEY"
   AWS_SECRET_ACCESS_KEY="YOUR_SECRET_KEY"
   AWS_REGION="us-east-1"

   # Frontend Configuration
   VITE_API_BASE_URL="http://localhost:5000"
   VITE_SIGNALR_HUB_URL="http://localhost:5000/hubs/jobs"
   ```

2. Add to `.gitignore` (ensure secrets not committed):

   ```
   .env.local
   .env.*.local
   .aws/
   .secrets/
   ```

3. Verify `.gitignore` is in repository:
   ```bash
   git ls-files -o --exclude-standard
   # Should NOT list .env.local
   ```

**Verification:**

- [ ] `.env.local` created (not committed)
- [ ] `.gitignore` includes `.env.local`
- [ ] Test: `echo $DATABASE_CONNECTION_STRING` shows value

---

## Section 5: Repository Structure Verification

### ✅ 5.1 Verify Project Folder Structure

**Task:** Ensure repository has correct folder structure before development.

**Expected Structure:**

```
smartscheduler/
├── .github/
│   └── workflows/           (CI/CD pipeline files - created in Epic 1.6)
├── backend/                 (Created in Epic 1.1)
│   ├── API/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   └── .gitignore
├── frontend/                (Created in Epic 1.1)
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── vite.config.ts
├── shared/                  (Created in Epic 1.1)
│   └── types/
├── infrastructure/          (Created for AWS CDK)
│   └── cdk/
├── docs/                    (Already exists)
├── scripts/                 (Setup scripts)
├── .gitignore               (Created)
├── package.json             (Root workspace)
├── README.md                (Project overview)
└── .env.example             (Template, no secrets)
```

**Current Status (Check as you go):**

- [ ] `/docs/` exists with architecture documentation
- [ ] `.gitignore` configured for .NET + Node.js projects
- [ ] `README.md` exists with project overview
- [ ] Other folders will be created during Epic 1

---

## Section 6: AWS Infrastructure Provisioning (Deferred to Epic 1.5)

### ⏭️ 6.1 AWS Resources - NOT YET

**Note:** The following AWS resources are created during **Story 1.5 (AWS Infrastructure & Deployment Foundation)**:

- [ ] RDS PostgreSQL instance (dev and prod)
- [ ] ECR repository (Docker image storage)
- [ ] App Runner service (backend hosting)
- [ ] S3 bucket (frontend static assets)
- [ ] CloudFront distribution (CDN)
- [ ] AWS Secrets Manager secret objects

**You will configure these in Epic 1, Story 1.5. This checklist only covers prerequisites.**

---

## Section 7: Verification Checklist

### ✅ 7.1 Pre-Development System Check

**Run these commands to verify everything is set up correctly:**

```bash
# 1. Git configuration
git config --list | grep user.name        # Should show your name

# 2. AWS credentials
aws sts get-caller-identity --profile smartscheduler-dev
# Output should show your IAM user ARN

# 3. Database connection
psql -U smartscheduler_dev -d smartscheduler_dev -h localhost -c "SELECT version();"
# Output should show PostgreSQL version

# 4. Node.js and npm
node --version && npm --version

# 5. .NET SDK
dotnet --info | grep "Version"

# 6. Docker
docker run hello-world
# Output should show "Hello from Docker!"

# 7. Secrets Manager access
aws secretsmanager list-secrets --profile smartscheduler-dev
# Output should list your 4 secrets
```

**Verification Checklist:**

- [ ] Git user configured
- [ ] AWS credentials working
- [ ] PostgreSQL connection successful
- [ ] Node.js/npm available
- [ ] .NET 8.0.21+ installed
- [ ] Docker installed and working
- [ ] AWS Secrets Manager accessible

---

### ✅ 7.2 Final Readiness Check

**Before starting Epic 1, confirm:**

| Requirement                | Status | Notes                               |
| -------------------------- | ------ | ----------------------------------- |
| AWS account active         | ✓      | —                                   |
| IAM user created           | ✓      | `smartscheduler-dev`                |
| Secrets Manager configured | ✓      | 4 secrets stored                    |
| Google Maps API enabled    | ✓      | API key restricted                  |
| GitHub repository created  | ✓      | Public, cloned locally              |
| GitHub secrets configured  | ✓      | 6 secrets for CI/CD                 |
| PostgreSQL running locally | ✓      | Database `smartscheduler_dev`       |
| .env.local created         | ✓      | Never commit                        |
| All tools installed        | ✓      | Git, .NET, Node, PostgreSQL, Docker |
| System verification passed | ✓      | All commands successful             |

---

## Section 8: Troubleshooting

### Common Issues & Solutions

#### Issue: "AWS credentials not found"

**Solution:**

```bash
# Verify credentials file exists
cat ~/.aws/credentials

# If not, add them:
aws configure --profile smartscheduler-dev
# Then enter: Access Key ID, Secret Access Key, region (us-east-1), format (json)
```

---

#### Issue: "PostgreSQL connection refused"

**Solution:**

```bash
# macOS: Start PostgreSQL service
brew services start postgresql@16

# Linux: Start PostgreSQL service
sudo systemctl start postgresql

# Verify it's running
ps aux | grep postgres

# Test connection
psql -U smartscheduler_dev -d smartscheduler_dev -h localhost
```

---

#### Issue: "Google Maps API returns 'API key not valid'"

**Solution:**

1. Verify API key is correct: `echo $GOOGLE_MAPS_API_KEY`
2. Check API key is restricted to correct APIs (Distance Matrix, Maps JavaScript)
3. Verify billing account is active (no past-due balance)
4. Wait 2-5 minutes for API key restrictions to propagate

---

#### Issue: "Docker: permission denied while trying to connect to Docker daemon"

**Solution (Linux):**

```bash
# Add your user to docker group
sudo usermod -aG docker $USER

# Log out and back in, then verify
docker run hello-world
```

---

#### Issue: ".env.local accidentally committed"

**Solution:**

```bash
# Remove from git history (WARNING: rewrites history)
git rm --cached .env.local
git commit --amend --no-edit

# Or if already pushed:
git push origin main --force-with-lease
```

---

## Section 9: Getting Started with Epic 1

### ✅ Once All Prerequisites Complete

You are now ready to start **Epic 1: Foundation & Infrastructure**.

**Next steps:**

1. Read: `/docs/prd/epic-details.md` - Epic 1 overview
2. Start: **Story 1.1 - Project Setup & Clean Architecture**

   - Create .NET 8 project structure
   - Configure dependency injection
   - Set up Serilog logging
   - Initialize GitHub repo with first commit

3. Follow: Architecture documentation in `/docs/architecture/` for guidance

---

## Sign-Off

**Developer Name:** **\*\***\_\_\_\_**\*\***

**Date Completed:** **\*\***\_\_\_\_**\*\***

**All Prerequisites Completed:** ✅ YES

**If NO, list remaining items:**

```

```

**Ready for Epic 1:** ✅ YES

---

**Document Version:** 1.0  
**Last Updated:** November 7, 2025  
**Author:** Product Owner (Sarah)
