# 13. Development Workflow

## 13.1 Local Development Setup

**Prerequisites:**

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 16
- Redis

**Initial Setup:**

```bash
# Clone repository
git clone https://github.com/yourorg/smartscheduler.git
cd smartscheduler

# Install all dependencies
npm install

# Setup backend
cd backend
dotnet restore
dotnet build

# Setup frontend
cd ../frontend
npm install

# Create database
createdb smartscheduler_dev
cd ../backend/SmartScheduler.Infrastructure
dotnet ef database update --startup-project ../SmartScheduler.API
```

**Development Commands:**

```bash
# Start backend
cd backend/SmartScheduler.API
dotnet watch run

# Start frontend
cd frontend
npm run dev

# Run tests
npm run test:frontend
npm run test:backend
```

## 13.2 Environment Configuration

**Frontend (.env.local):**

```bash
VITE_API_BASE_URL=http://localhost:5000/api/v1
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/notifications
VITE_GOOGLE_MAPS_API_KEY=your_key
```

**Backend (appsettings.Development.json):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=smartscheduler_dev;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars-long",
    "Issuer": "SmartScheduler",
    "Audience": "SmartScheduler.Client"
  }
}
```

---
