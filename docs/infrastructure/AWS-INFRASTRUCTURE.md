# AWS Infrastructure Setup & Deployment Guide

## Overview

SmartScheduler uses the following AWS services for deployment:

| Service             | Purpose                 | Configuration                           |
| ------------------- | ----------------------- | --------------------------------------- |
| **RDS PostgreSQL**  | Managed database        | `db.t3.micro`, 20GB, Single AZ          |
| **Lightsail**       | Backend hosting         | Ubuntu 24.04 LTS, Docker container      |
| **S3**              | Frontend static assets  | `smartscheduler-frontend` bucket        |
| **CloudFront**      | Global CDN for frontend | Distribution ID: see deployment section |
| **ECR**             | Docker image registry   | `smartscheduler-backend` repository     |
| **Secrets Manager** | Credential storage      | 4 secrets for DB, JWT, etc.             |

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     CloudFront CDN                       │
│        d14t4lhpynqwav.cloudfront.net                    │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │   S3 Frontend Bucket  │
         │ smartscheduler-frontend
         └───────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  Lightsail Instance                      │
│              3.239.203.224:8080                          │
│    Backend API (Docker Container - Port 8080)           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │  RDS PostgreSQL DB    │
         │  smartscheduler-db    │
         └───────────────────────┘
```

---

## Prerequisites

- AWS Account
- AWS CLI configured: `aws configure`
- Docker installed: `docker --version`
- PostgreSQL client (optional): `psql`

---

## Database Setup (RDS PostgreSQL)

### Create RDS Instance

1. Navigate to **RDS Console** → **Databases** → **Create database**
2. **Engine:** PostgreSQL 16.6
3. **Template:** Dev/Test
4. **Settings:**

   - Identifier: `smartscheduler-db-instance`
   - Master username: `postgres`
   - Master password: [secure password]
   - Instance class: `db.t3.micro`
   - Storage: 20 GB
   - Backup retention: 7 days
   - Public access: Yes

5. **Security Group:** Allow inbound port 5432 from:
   - Your local IP (for local development)
   - Lightsail security group (for backend)

### Retrieve Endpoint

RDS Endpoint: `smartscheduler-db.xxxxxxxx.us-east-1.rds.amazonaws.com`

### Test Connection

```bash
psql -h smartscheduler-db.xxxxxxxx.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d postgres \
     -p 5432
```

---

## Secrets Manager Setup

Create the following secrets:

### 1. Database Credentials

- **Secret Name:** `smartscheduler/db/credentials`
- **Key-Value:**
  - `username`: `postgres`
  - `password`: [your-rds-password]

### 2. JWT Secret

- **Secret Name:** `smartscheduler/jwt-secret`
- **Value:** [32+ character secure random string]

```bash
# Generate JWT secret
openssl rand -base64 32
```

### 3. Google Maps API Key (Placeholder)

- **Secret Name:** `smartscheduler/google-maps-api-key`
- **Value:** `placeholder-key-for-portfolio`

### 4. AWS SES Credentials (Placeholder)

- **Secret Name:** `smartscheduler/aws-ses-credentials`
- **Key-Value:**
  - `access-key-id`: `placeholder-access-key`
  - `secret-access-key`: `placeholder-secret-key`

### Verify Secrets

```bash
aws secretsmanager list-secrets
```

---

## Docker Setup

### Build Docker Image

```bash
cd /path/to/SmartScheduler

docker build -t smartscheduler-backend:latest \
  -f backend/SmartScheduler.API/Dockerfile \
  backend/
```

### Verify Image Size

```bash
docker images smartscheduler-backend
# Expected size: ~200-250 MB
```

### Test Locally

```bash
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e DATABASE_HOST=smartscheduler-db.xxxxxxxx.us-east-1.rds.amazonaws.com \
  -e DATABASE_PORT=5432 \
  -e DATABASE_NAME=smartscheduler \
  -e DATABASE_USER=postgres \
  -e DATABASE_PASSWORD=[your-password] \
  -e Jwt__SecretKey=[your-jwt-secret] \
  -e Jwt__Issuer=SmartScheduler \
  -e Jwt__Audience=SmartSchedulerClient \
  smartscheduler-backend:latest

# In another terminal, test health endpoint
curl http://localhost:8080/health
```

---

## ECR Repository Setup

### Create ECR Repository

1. Navigate to **ECR Console** → **Repositories** → **Create repository**
2. **Repository name:** `smartscheduler-backend`
3. **Tag immutability:** Mutable
4. **Scan on push:** Enabled

### Push Docker Image to ECR

```bash
# Login to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  971422717446.dkr.ecr.us-east-1.amazonaws.com

# Tag image
docker tag smartscheduler-backend:latest \
  971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

# Push to ECR
docker push \
  971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

# Verify in ECR Console
```

---

## Backend Deployment (Lightsail)

### Create Lightsail Instance

1. Navigate to **Lightsail Console** → **Instances** → **Create instance**
2. **Location:** `us-east-1a`
3. **Image:** Ubuntu 24.04 LTS
4. **Instance plan:** `$3.50/month` (512 MB RAM, 1 vCPU)
5. **Instance name:** `smartscheduler-backend`
6. Click **Create instance**

### Setup Docker on Lightsail

```bash
# SSH into instance
ssh -i ~/path/to/lightsail-key.pem ubuntu@3.239.203.224

# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
sudo apt install -y docker.io

# Start Docker
sudo systemctl start docker

# Add user to docker group
sudo usermod -aG docker ubuntu

# Verify Docker
docker --version
```

### Deploy Backend Container

```bash
# Login to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  971422717446.dkr.ecr.us-east-1.amazonaws.com

# Pull latest image
docker pull \
  971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

# Run container with auto-restart
docker run -d \
  --restart=always \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DATABASE_HOST=smartscheduler-db.xxxxxxxx.us-east-1.rds.amazonaws.com \
  -e DATABASE_PORT=5432 \
  -e DATABASE_NAME=smartscheduler \
  -e DATABASE_USER=postgres \
  -e DATABASE_PASSWORD=[your-rds-password] \
  -e Jwt__SecretKey=[your-jwt-secret] \
  -e Jwt__Issuer=SmartScheduler \
  -e Jwt__Audience=SmartSchedulerClient \
  --name smartscheduler-api \
  971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

# Verify container is running
docker ps

# Check logs
docker logs smartscheduler-api

# Test health endpoint
curl http://localhost:8080/health
```

### Configure Firewall

1. In **Lightsail Console**, click instance
2. Go to **Networking** tab
3. Under **IPv4 Firewall**, click **+ Add rule**
4. **Protocol:** HTTP, **Port:** 8080
5. Click **Create**

### Backend URL

```
API Endpoint: http://3.239.203.224:8080
Health Check: http://3.239.203.224:8080/health
```

---

## Frontend Deployment (S3 + CloudFront)

### Create S3 Bucket

1. Navigate to **S3 Console** → **Create bucket**
2. **Bucket name:** `smartscheduler-frontend`
3. **Region:** `us-east-1`
4. **Block public access:** Enabled
5. **Versioning:** Enabled
6. **Encryption:** Default (AES-256)
7. Click **Create bucket**

### Create CloudFront Distribution

1. Navigate to **CloudFront Console** → **Create distribution**
2. **Origin:** Select S3 bucket (`smartscheduler-frontend`)
3. **OAI:** Create new (`smartscheduler-frontend-oai`)
4. **Default root object:** `index.html`
5. **Compress objects:** Enabled
6. **Viewer protocol policy:** Redirect HTTP to HTTPS
7. Click **Create distribution**

### CloudFront Details

```
Domain: d14t4lhpynqwav.cloudfront.net
Distribution ID: E1234ABCDEF5G (example)
```

### Deploy Frontend

After building your frontend (React):

```bash
# Build frontend
cd frontend
npm run build

# Upload to S3
aws s3 sync dist/ s3://smartscheduler-frontend/ --delete

# Invalidate CloudFront cache
aws cloudfront create-invalidation \
  --distribution-id E1234ABCDEF5G \
  --paths "/*"
```

### Frontend URL

```
https://d14t4lhpynqwav.cloudfront.net
```

---

## Database Schema & Migrations

### Automatic Migrations on Startup

The backend automatically runs EF Core migrations on startup:

```csharp
// Program.cs - automatically runs all pending migrations
await context.Database.MigrateAsync();
```

### Manual Migration (if needed)

```bash
# From backend/SmartScheduler.API directory
dotnet ef database update --project ../SmartScheduler.Infrastructure
```

### Verify Database Schema

```bash
psql -h smartscheduler-db.xxxxxxxx.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d smartscheduler \
     -p 5432

# List tables
\dt

# View table schema
\d contractors
\d jobs
\d assignments
```

---

## Troubleshooting

### Issue: Container can't connect to RDS

**Solution:**

- Verify RDS endpoint in DATABASE_HOST environment variable
- Confirm RDS security group allows inbound port 5432
- Test connection: `psql -h [endpoint] -U postgres`

### Issue: Health check fails

**Solution:**

- Check Lightsail logs: `docker logs smartscheduler-api`
- Verify all environment variables are set
- Ensure database is accessible
- Check RDS credentials

### Issue: CloudFront returns 403

**Solution:**

- Verify S3 bucket has OAI policy
- Check bucket is not publicly accessible
- Verify CloudFront distribution is deployed

### Issue: Database migrations fail

**Solution:**

- Ensure RDS instance is running and accessible
- Check database user has proper permissions
- Verify migrations are idempotent

---

## Monitoring & Logs

### View Backend Logs

```bash
# SSH into Lightsail instance
ssh -i ~/path/to/key.pem ubuntu@3.239.203.224

# View logs
docker logs smartscheduler-api

# Follow logs in real-time
docker logs -f smartscheduler-api
```

### View RDS Logs

1. Navigate to **RDS Console** → **Databases** → select instance
2. Go to **Logs & events** tab
3. View PostgreSQL logs

### CloudWatch (Optional)

- Backend logs are sent to CloudWatch via Serilog
- Log group: `/aws/apprunner/smartscheduler` (configured in appsettings.Production.json)

---

## Costs

**Monthly Cost Estimate (Free Tier):**

- RDS PostgreSQL: $0 (free tier for 1 year)
- Lightsail: $3.50/month
- S3: $0 (5GB free tier)
- CloudFront: $0 (50GB free tier)
- **Total: ~$3.50/month**

---

## Next Steps

1. ✅ Database setup complete
2. ✅ Backend deployed to Lightsail
3. ✅ Frontend ready for deployment
4. **Next:** Build and deploy frontend React app to S3/CloudFront

---

## Useful Commands

```bash
# SSH into Lightsail
ssh -i ~/path/to/key.pem ubuntu@3.239.203.224

# View running containers
docker ps

# View all containers
docker ps -a

# Check container logs
docker logs smartscheduler-api

# Restart container
docker restart smartscheduler-api

# Stop container
docker stop smartscheduler-api

# Remove container
docker rm smartscheduler-api

# Test health endpoint
curl http://3.239.203.224:8080/health

# SSH into RDS database
psql -h smartscheduler-db.xxxxxxxx.us-east-1.rds.amazonaws.com \
     -U postgres \
     -d smartscheduler

# Sync frontend to S3
aws s3 sync dist/ s3://smartscheduler-frontend/ --delete

# Invalidate CloudFront
aws cloudfront create-invalidation --distribution-id E1234ABCDEF5G --paths "/*"
```

---

## References

- [AWS RDS Documentation](https://docs.aws.amazon.com/rds/)
- [AWS Lightsail Documentation](https://docs.aws.amazon.com/lightsail/)
- [AWS S3 Documentation](https://docs.aws.amazon.com/s3/)
- [AWS CloudFront Documentation](https://docs.aws.amazon.com/cloudfront/)
- [Docker Documentation](https://docs.docker.com/)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
