# Quick Start Guide — Local Development

**Purpose:** Get SmartScheduler running locally in 10 minutes (assumes pre-development checklist complete)

**Prerequisites:** [Complete Pre-Development Checklist](./pre-development-checklist.md) first ✅

---

## Step 1: Clone & Navigate

```bash
# Clone repository
git clone https://github.com/YOUR_USERNAME/smartscheduler.git
cd smartscheduler

# Verify structure
ls -la
# Output should show: docs/, .gitignore, README.md, etc.
```

---

## Step 2: Set Up Backend (.NET 8)

```bash
# Navigate to backend folder
cd backend

# Restore NuGet packages
dotnet restore

# Create local user secrets (stores sensitive config, never committed)
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=5432;Database=smartscheduler_dev;User Id=smartscheduler_dev;Password=YOUR_PASSWORD;"

# Set JWT secret
dotnet user-secrets set "Jwt:SecretKey" "YOUR_JWT_SECRET_FROM_SECRETS_MANAGER"

# Set Google Maps API key
dotnet user-secrets set "GoogleMaps:ApiKey" "YOUR_GOOGLE_MAPS_API_KEY"

# Apply database migrations
dotnet ef database update

# Start backend server (runs on http://localhost:5000)
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
#       Now listening on: http://localhost:5000
```

**Keep this terminal open. Backend will run at `http://localhost:5000`.**

---

## Step 3: Set Up Frontend (React + Vite)

**In a new terminal:**

```bash
# Navigate to frontend folder
cd smartscheduler/frontend

# Install dependencies
npm install

# Create .env.local for frontend config
cat > .env.local << EOF
VITE_API_BASE_URL=http://localhost:5000
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/jobs
EOF

# Start development server (runs on http://localhost:5173)
npm run dev

# Expected output:
#   VITE v6.0.7  ready in 234 ms
#   ➜  Local:   http://localhost:5173/
#   ➜  press h to show help
```

**Keep this terminal open. Frontend will run at `http://localhost:5173`.**

---

## Step 4: Verify Everything is Running

**In a new terminal:**

```bash
# Test backend API
curl http://localhost:5000/health
# Expected output: {"status":"healthy"}

# Test frontend
open http://localhost:5173
# Or navigate in browser: http://localhost:5173

# Check database connection
cd smartscheduler/backend
dotnet ef dbcontext info
# Expected output shows: Provider name: Npgsql.EntityFrameworkCore.PostgreSQL
```

---

## Step 5: Create First Test User

**In a new terminal:**

```bash
# Navigate to backend
cd smartscheduler/backend

# Create a test user via API (requires authentication endpoint - implement in Story 1.3)
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "dispatcher@test.com",
    "password": "TestPassword123!",
    "role": "Dispatcher"
  }'

# Expected output: JWT token and user details
```

---

## Step 6: Browser Test

1. Open browser to `http://localhost:5173`
2. Log in with:
   - Email: `dispatcher@test.com`
   - Password: `TestPassword123!`
3. You should see the Dispatcher dashboard (or login error if auth not yet implemented)

---

## Troubleshooting Quick Reference

### Backend won't start

```bash
# Issue: Port 5000 already in use
# Solution: Kill existing process
lsof -i :5000
kill -9 <PID>

# Or use different port
dotnet run --urls="http://localhost:5001"
```

### Database migrations fail

```bash
# Issue: Migration already applied or connection error
# Solution: Check connection string
dotnet ef dbcontext info

# Or reset database (DEV ONLY!)
dotnet ef database drop
dotnet ef database update
```

### Frontend won't connect to backend

```bash
# Issue: CORS error or wrong URL
# Solution: Verify .env.local
cat frontend/.env.local

# Should show: VITE_API_BASE_URL=http://localhost:5000

# Check backend is running on port 5000
curl http://localhost:5000/health
```

### Node modules issues

```bash
# Clear and reinstall
rm -rf frontend/node_modules
npm install
```

---

## Development Workflow

### Making Backend Changes

1. Edit code in `backend/` folder
2. Backend auto-recompiles (if using `watch` mode: `dotnet watch run`)
3. Test via: `curl http://localhost:5000/endpoint`

### Making Frontend Changes

1. Edit code in `frontend/src/` folder
2. Frontend hot-reloads automatically (Vite HMR)
3. Refresh browser to see changes

### Running Tests

```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend
npm run test
```

---

## Useful Commands Reference

| Task                | Command                                                               |
| ------------------- | --------------------------------------------------------------------- |
| Start backend       | `cd backend && dotnet run`                                            |
| Start frontend      | `cd frontend && npm run dev`                                          |
| Run migrations      | `cd backend && dotnet ef database update`                             |
| Seed test data      | `cd backend && dotnet ef database update --context "SeedDataContext"` |
| Run tests           | `dotnet test` (backend) or `npm run test` (frontend)                  |
| Check logs          | `dotnet run --verbosity debug`                                        |
| Database GUI        | `psql -U smartscheduler_dev -d smartscheduler_dev`                    |
| Check running ports | `lsof -i :5000` or `lsof -i :5173`                                    |

---

## What's Running

Once everything starts, you should see:

```
✅ Backend API:       http://localhost:5000
   └─ Swagger UI:     http://localhost:5000/swagger
   └─ Health check:   http://localhost:5000/health

✅ Frontend SPA:      http://localhost:5173
   └─ Hot reload:     Enabled

✅ Database:          localhost:5432
   └─ Database:       smartscheduler_dev
   └─ Psql CLI:       psql -U smartscheduler_dev -d smartscheduler_dev

✅ SignalR Hub:       ws://localhost:5000/hubs/jobs
   └─ Real-time:      Ready for WebSocket connections
```

---

## Next Steps

- Read: [Epic 1 Stories](../prd/epic-details.md)
- Implement: Story 1.1 (Project Setup & Clean Architecture)
- Reference: [Coding Standards](../architecture/17-coding-standards.md)
- Follow: [Implementation Guides](../architecture/20-ai-agent-implementation-guides.md)

---

## Support

For issues or questions:

1. Check [Pre-Development Checklist Troubleshooting](./pre-development-checklist.md#section-8-troubleshooting)
2. Review [Architecture Documentation](../architecture/index.md)
3. Consult [Coding Standards](../architecture/17-coding-standards.md)

---

**Document Version:** 1.0  
**Last Updated:** November 7, 2025
