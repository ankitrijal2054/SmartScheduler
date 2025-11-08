# SmartScheduler

A full-stack contractor job scheduling application built with **ASP.NET Core 8**, **React 18**, **PostgreSQL**, and **AWS**.

## Overview

SmartScheduler helps dispatchers efficiently schedule jobs for contractors by providing intelligent recommendations based on:

- Contractor location and availability
- Job requirements and complexity
- Real-time job status tracking
- RBAC-based access control (Dispatcher, Contractor, Customer roles)

## Tech Stack

### Backend

- **Runtime:** .NET 9.0
- **Web Framework:** ASP.NET Core 8.0
- **Database:** PostgreSQL 16.6 (AWS RDS)
- **ORM:** Entity Framework Core 8.0
- **Authentication:** JWT Bearer Token
- **API Docs:** Swagger/OpenAPI
- **Logging:** Serilog (structured JSON logging)

### Frontend

- **Framework:** React 18.x
- **Language:** TypeScript
- **Build Tool:** Vite
- **State Management:** React Context / TBD
- **Styling:** CSS Modules / Tailwind CSS

### Infrastructure

- **Hosting:** AWS Lightsail (Backend), S3 + CloudFront (Frontend)
- **Database:** AWS RDS PostgreSQL
- **Container Registry:** AWS ECR
- **Secrets:** AWS Secrets Manager
- **Monitoring:** CloudWatch (optional)

---

## Project Structure

```
SmartScheduler/
├── backend/
│   ├── SmartScheduler.API/           # ASP.NET Core Web API
│   ├── SmartScheduler.Application/   # Business logic & services
│   ├── SmartScheduler.Domain/        # Domain entities & rules
│   ├── SmartScheduler.Infrastructure/# Data access & external services
│   ├── SmartScheduler.API.Tests/     # API endpoint tests
│   └── SmartScheduler.sln            # Solution file
├── frontend/                         # React TypeScript application (TBD)
├── docs/
│   ├── architecture/                 # Technical architecture docs
│   ├── infrastructure/               # Deployment & AWS setup
│   ├── prd/                          # Product requirements
│   └── stories/                      # User stories & tasks
└── README.md                         # This file
```

---

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+
- Docker & Docker Compose
- PostgreSQL client (psql)
- AWS Account (for deployment)

### Local Development

#### 1. Setup Database

```bash
# Ensure PostgreSQL is running locally or use RDS
export DATABASE_HOST=localhost
export DATABASE_PORT=5432
export DATABASE_NAME=smartscheduler
export DATABASE_USER=postgres
export DATABASE_PASSWORD=yourpassword
```

#### 2. Start Backend

```bash
cd backend
dotnet restore
dotnet run --project SmartScheduler.API
```

Backend runs on: `http://localhost:5000`
API Docs: `http://localhost:5000/swagger`

#### 3. Start Frontend (TBD)

```bash
cd frontend
npm install
npm run dev
```

Frontend runs on: `http://localhost:3000` (TBD)

---

## Deployment

### Backend Deployment (Lightsail)

See detailed deployment instructions: [`docs/infrastructure/AWS-INFRASTRUCTURE.md`](docs/infrastructure/AWS-INFRASTRUCTURE.md)

**Quick Deploy:**

```bash
# Build Docker image
docker build -t smartscheduler-backend:latest \
  -f backend/SmartScheduler.API/Dockerfile \
  backend/

# Push to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  971422717446.dkr.ecr.us-east-1.amazonaws.com

docker tag smartscheduler-backend:latest \
  971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

docker push \
  971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

# Deploy to Lightsail (see AWS-INFRASTRUCTURE.md for full steps)
```

### Frontend Deployment (S3 + CloudFront)

```bash
# Build frontend
cd frontend
npm run build

# Upload to S3
aws s3 sync dist/ s3://smartscheduler-frontend/ --delete

# Invalidate CloudFront cache
aws cloudfront create-invalidation --distribution-id E1234ABCDEF5G --paths "/*"
```

**Production URLs:**

- **Backend API:** `http://3.239.203.224:8080`
- **Frontend:** `https://d14t4lhpynqwav.cloudfront.net`

---

## Environment Variables

### Development

```bash
ASPNETCORE_ENVIRONMENT=Development
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=smartscheduler
DATABASE_USER=postgres
DATABASE_PASSWORD=yourpassword
Jwt__SecretKey=your-secret-key-32-chars-minimum
Jwt__Issuer=SmartScheduler
Jwt__Audience=SmartSchedulerClient
```

### Production (Lightsail)

All environment variables are set in the Docker container via Lightsail instance configuration (see `AWS-INFRASTRUCTURE.md`).

---

## API Endpoints

### Health Check

```
GET /health
Response: {"status":"healthy","timestamp":"..."}
```

### Authentication

```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
```

### Jobs

```
GET    /api/jobs
POST   /api/jobs
GET    /api/jobs/{id}
PUT    /api/jobs/{id}
DELETE /api/jobs/{id}
```

### Contractors

```
GET    /api/contractors
POST   /api/contractors
GET    /api/contractors/{id}
PUT    /api/contractors/{id}
```

### Assignments

```
GET    /api/assignments
POST   /api/assignments
PUT    /api/assignments/{id}
```

**Full API Documentation:** See Swagger UI at `/swagger`

---

## Database Schema

### Key Tables

- **Users** - System users (Dispatcher, Contractor, Customer)
- **Contractors** - Contractor profiles with location & availability
- **Jobs** - Job listings with status (Pending, Assigned, In Progress, Completed)
- **Assignments** - Job-to-contractor assignments with status tracking
- **RefreshTokens** - JWT refresh token storage

See: [`docs/architecture/9-database-schema.md`](docs/architecture/9-database-schema.md)

---

## Testing

### Run Backend Tests

```bash
# Run all tests
dotnet test backend/

# Run specific test project
dotnet test backend/SmartScheduler.API.Tests

# With coverage
dotnet test /p:CollectCoverage=true
```

### Run Frontend Tests (TBD)

```bash
cd frontend
npm test
npm run test:coverage
```

---

## Documentation

- **Architecture:** [`docs/architecture/`](docs/architecture/)
- **Deployment:** [`docs/infrastructure/AWS-INFRASTRUCTURE.md`](docs/infrastructure/AWS-INFRASTRUCTURE.md)
- **Product Requirements:** [`docs/prd/`](docs/prd/)
- **User Stories:** [`docs/stories/`](docs/stories/)

---

## Development Workflow

1. **Pick a story:** `docs/stories/1.x.story.md`
2. **Read requirements:** Story tasks and acceptance criteria
3. **Implement:** Follow the task checklist
4. **Test:** Run unit/integration tests
5. **Document:** Update API docs and README
6. **Deploy:** Follow deployment steps

---

## Common Tasks

### Add a New Database Migration

```bash
cd backend/SmartScheduler.Infrastructure
dotnet ef migrations add YourMigrationName -p ../SmartScheduler.Infrastructure
dotnet ef database update
```

### Add a New API Endpoint

1. Create controller in `backend/SmartScheduler.API/Controllers/`
2. Add service in `backend/SmartScheduler.Application/Services/`
3. Add tests in `backend/SmartScheduler.API.Tests/`
4. Restart backend: `dotnet run`

### Deploy New Backend Version

```bash
# Rebuild Docker image
docker build -t smartscheduler-backend:latest \
  -f backend/SmartScheduler.API/Dockerfile \
  backend/

# Push to ECR
docker push 971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest

# SSH into Lightsail and pull latest
docker pull 971422717446.dkr.ecr.us-east-1.amazonaws.com/smartscheduler-backend:latest
docker restart smartscheduler-api
```

---

## Troubleshooting

### Backend won't start

- Check database connection: `psql -h [endpoint] -U postgres`
- Verify environment variables are set
- Check logs: `docker logs smartscheduler-api`

### Frontend can't reach backend

- Ensure backend is running: `curl http://3.239.203.224:8080/health`
- Check CORS headers in backend
- Verify API URL in frontend config

### Database migration fails

- Ensure RDS instance is running
- Check database user permissions
- Review migration code for syntax errors

### CloudFront returns 403

- Verify S3 bucket has OAI policy
- Check CloudFront distribution is deployed
- Clear browser cache

---

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Follow code standards: See [`docs/architecture/17-coding-standards.md`](docs/architecture/17-coding-standards.md)
3. Write tests for new features
4. Commit with clear messages: `git commit -m "feat: add new feature"`
5. Push and create Pull Request

---

## License

Proprietary - SmartScheduler Portfolio Project

---

## Contact

For questions or issues, see project documentation in `docs/` directory.

---

## Deployment Checklist

Before going to production:

- [ ] Backend Docker image builds and runs locally
- [ ] All environment variables configured in Lightsail
- [ ] Database migrations run successfully
- [ ] Health endpoint responds: `curl http://3.239.203.224:8080/health`
- [ ] Frontend builds without errors
- [ ] Frontend deployed to S3 + CloudFront
- [ ] CloudFront distribution is operational
- [ ] API endpoints tested manually (Postman/curl)
- [ ] Authentication works (JWT tokens issued)
- [ ] Database backups enabled
- [ ] Monitoring configured (optional)
- [ ] Documentation complete

---

## Status

**Current Version:** 1.5 (AWS Infrastructure & Deployment Foundation)

**Completed Features:**

- ✅ User authentication & RBAC
- ✅ Job management (CRUD)
- ✅ Contractor profiles
- ✅ Job assignment system
- ✅ AWS infrastructure setup
- ✅ Docker containerization
- ✅ Database migrations

**In Progress:**

- 🔄 Frontend React application

**Planned:**

- [ ] Real-time job updates (SignalR)
- [ ] AI-powered job recommendations
- [ ] Mobile app
- [ ] Advanced reporting & analytics
- [ ] Multi-region deployment

---

Last Updated: November 8, 2025
