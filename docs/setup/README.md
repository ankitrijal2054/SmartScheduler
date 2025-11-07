# Setup & Getting Started Guide

Welcome to **SmartScheduler**! This folder contains everything you need to set up your local development environment and understand the project structure.

---

## 📋 Start Here: Three Documents

### 1. **[Pre-Development Checklist](./pre-development-checklist.md)** ← READ FIRST

**Time:** 30-45 minutes | **Status:** ✅ Before starting any code

**What it covers:**

- AWS account setup (IAM, Secrets Manager)
- Google Maps API configuration
- GitHub repository initialization
- Local development tools installation (Git, .NET, Node, PostgreSQL, Docker)
- Environment variables and credentials management

**When to use:** Before starting Epic 1. Verify all prerequisites are complete.

**Key checklist items:**

- [ ] AWS account created with IAM user
- [ ] Google Maps API key generated and restricted
- [ ] GitHub repository cloned locally
- [ ] PostgreSQL running on localhost:5432
- [ ] .env.local created with all secrets
- [ ] `aws sts get-caller-identity` returns your user ARN
- [ ] `psql` connects to local database

---

### 2. **[Quick Start Guide](./quick-start-guide.md)** ← RUN COMMANDS

**Time:** 10 minutes | **Status:** ✅ After pre-development checklist

**What it covers:**

- Copy-paste commands to start backend (.NET 8)
- Copy-paste commands to start frontend (React + Vite)
- How to verify everything is running
- Creating test users
- Troubleshooting common issues

**When to use:** When you want to run the app locally. Keep 3 terminals open:

1. Backend: `cd backend && dotnet run`
2. Frontend: `cd frontend && npm run dev`
3. Utility: For database commands, tests, etc.

**Quick reference:**

```bash
# Backend (Terminal 1)
cd backend && dotnet run        # Runs on http://localhost:5000

# Frontend (Terminal 2)
cd frontend && npm run dev      # Runs on http://localhost:5173

# Verify (Terminal 3)
curl http://localhost:5000/health
open http://localhost:5173
```

---

### 3. **[Epic Dependencies Map](./epic-dependencies-map.md)** ← UNDERSTAND ARCHITECTURE

**Time:** 15-20 minutes | **Status:** ✅ Before starting development

**What it covers:**

- Why epics are sequenced 1→2→3→4→5→6→7
- Data flow between epics (how they integrate)
- Which features enable which other features
- Risk points and mitigation strategies
- Parallel development opportunities (if team expands)

**When to use:** When starting Epic 1, or when confused about why a certain epic comes first.

**Key insights:**

- **Epic 1** (Foundation) → Blocks nothing; blocks everything else
- **Epic 2** (Scoring) → Enables dispatchers, customers, contractors
- **Epics 3-5** (Three portals) → Can start in parallel after Epic 2
- **Epic 6** (Events) → Ties all portals together
- **Epic 7** (Testing) → Only after Epics 1-6 complete

---

## 🚀 Quick Navigation

| Need                       | Document                                                                                                        | Time   |
| -------------------------- | --------------------------------------------------------------------------------------------------------------- | ------ |
| Set up AWS, tools, secrets | [Pre-Dev Checklist](./pre-development-checklist.md)                                                             | 45 min |
| Run app locally            | [Quick Start](./quick-start-guide.md)                                                                           | 10 min |
| Understand epic order      | [Dependencies Map](./epic-dependencies-map.md)                                                                  | 20 min |
| Start coding Epic 1        | [/docs/prd/epic-details.md](../prd/epic-details.md)                                                             | —      |
| Coding standards           | [/docs/architecture/17-coding-standards.md](../architecture/17-coding-standards.md)                             | —      |
| Implementation guides      | [/docs/architecture/20-ai-agent-implementation-guides.md](../architecture/20-ai-agent-implementation-guides.md) | —      |

---

## 🎯 Development Workflow

### Before You Code

1. ✅ Complete [Pre-Development Checklist](./pre-development-checklist.md)
2. ✅ Verify setup with [Quick Start Guide](./quick-start-guide.md)
3. ✅ Understand epic order from [Dependencies Map](./epic-dependencies-map.md)

### During Development

1. Read epic story from [/docs/prd/epic-details.md](../prd/epic-details.md)
2. Follow [Coding Standards](../architecture/17-coding-standards.md)
3. Reference [Implementation Guides](../architecture/20-ai-agent-implementation-guides.md)
4. Start backend: `cd backend && dotnet run`
5. Start frontend: `cd frontend && npm run dev`
6. Run tests: `dotnet test` (backend) or `npm run test` (frontend)

### Key Tools

- **Backend:** .NET 8, Entity Framework Core, MediatR (CQRS)
- **Frontend:** React 18, Vite, shadcn/ui, TypeScript
- **Database:** PostgreSQL 16, RDS (production)
- **Real-time:** SignalR (WebSocket)
- **Deployment:** GitHub Actions → AWS App Runner (backend), S3 + CloudFront (frontend)

---

## 📊 Project Structure

```
smartscheduler/
├── docs/
│   ├── setup/               ← You are here
│   │   ├── README.md        ← This file
│   │   ├── pre-development-checklist.md
│   │   ├── quick-start-guide.md
│   │   └── epic-dependencies-map.md
│   ├── prd/                 ← Product requirements
│   │   ├── index.md
│   │   ├── requirements.md
│   │   ├── epic-list.md
│   │   └── epic-details.md  ← Stories with acceptance criteria
│   └── architecture/        ← Technical documentation
│       ├── 1-introduction.md
│       ├── 2-high-level-architecture.md
│       ├── 3-tech-stack.md
│       ├── 12-unified-project-structure.md
│       ├── 17-coding-standards.md
│       ├── 20-ai-agent-implementation-guides.md
│       └── ... (17 more architecture docs)
├── backend/                 ← .NET 8 API (Epic 1)
│   ├── API/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   ├── Tests/
│   └── smartscheduler.sln
├── frontend/                ← React 18 (Epics 3, 4, 5)
│   ├── src/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── pages/
│   │   ├── services/
│   │   └── App.tsx
│   ├── package.json
│   └── vite.config.ts
├── shared/                  ← Shared TypeScript types
│   └── types/
├── infrastructure/          ← AWS CDK (Infrastructure as Code)
│   └── cdk/
├── .github/workflows/       ← CI/CD pipeline (Epic 1.6)
├── .gitignore
├── package.json             ← Root workspace config
└── README.md
```

---

## ✅ Sign-Off Checklist

Before starting Epic 1, confirm:

- [ ] **Pre-Development Checklist:** All items marked complete

  - [ ] AWS account and IAM user created
  - [ ] Google Maps API configured
  - [ ] GitHub repo cloned locally
  - [ ] PostgreSQL running
  - [ ] All tools installed (Git, .NET, Node, Docker)
  - [ ] `.env.local` created with secrets

- [ ] **Quick Start Guide:** App runs successfully

  - [ ] Backend starts: `cd backend && dotnet run`
  - [ ] Frontend starts: `cd frontend && npm run dev`
  - [ ] Can access http://localhost:5173
  - [ ] Backend health check: `curl http://localhost:5000/health`

- [ ] **Dependencies Map:** Understand epic order
  - [ ] Know why Epic 1 comes first
  - [ ] Understand Epic 2 enables Epics 3-5
  - [ ] Can explain data flow between epics

---

## 🆘 Troubleshooting

### Backend won't start

```bash
# Check port 5000 is available
lsof -i :5000

# Check database connection
cd backend
dotnet ef dbcontext info

# Check .env.local is loaded
echo $DATABASE_CONNECTION_STRING
```

### Frontend won't connect to backend

```bash
# Verify backend is running on port 5000
curl http://localhost:5000/health

# Check .env.local in frontend folder
cat frontend/.env.local
# Should show: VITE_API_BASE_URL=http://localhost:5000
```

### Database connection failed

```bash
# Verify PostgreSQL is running
psql -U smartscheduler_dev -d smartscheduler_dev -h localhost

# Check connection string
echo $DATABASE_CONNECTION_STRING
```

**For more troubleshooting:** See "Troubleshooting" section in [Pre-Development Checklist](./pre-development-checklist.md#section-8-troubleshooting).

---

## 📚 Next Steps

1. **Complete Pre-Development Checklist** (if not already)

   - Gives you: AWS setup, secrets, local database, tools
   - Time: 45 minutes

2. **Run Quick Start Guide** (if you want to see it running)

   - Gives you: Working local development environment
   - Time: 10 minutes

3. **Read Epic Dependencies Map** (to understand architecture)

   - Gives you: Why epics are ordered this way, data flow between them
   - Time: 20 minutes

4. **Start Epic 1, Story 1.1** (begin coding)
   - Read: [Epic Details](../prd/epic-details.md) — Story 1.1 acceptance criteria
   - Follow: [Coding Standards](../architecture/17-coding-standards.md)
   - Reference: [Implementation Guides](../architecture/20-ai-agent-implementation-guides.md)
   - Time: 2-3 days

---

## 🎓 Learning Resources

If you're unfamiliar with any tech stack:

- **Clean Architecture:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **CQRS Pattern:** https://martinfowler.com/bliki/CQRS.html
- **Domain-Driven Design:** https://martinfowler.com/bliki/DomainDrivenDesign.html
- **.NET 8:** https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8
- **React 18:** https://react.dev
- **SignalR:** https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction
- **Entity Framework Core:** https://learn.microsoft.com/en-us/ef/core/

---

## 📞 Support

For questions or issues:

1. Check the **Troubleshooting** section in each document
2. Review the **Architecture Documentation** in `/docs/architecture/`
3. Consult **Implementation Guides** for pattern-specific questions
4. Check **Coding Standards** for conventions

---

## 🎯 Success Criteria

You're ready to start Epic 1 when:

- ✅ Backend runs on http://localhost:5000
- ✅ Frontend runs on http://localhost:5173
- ✅ PostgreSQL database `smartscheduler_dev` exists and connects
- ✅ AWS credentials in `~/.aws/credentials`
- ✅ All secrets stored in AWS Secrets Manager or `.env.local`
- ✅ You've read and understand [Epic Dependencies Map](./epic-dependencies-map.md)
- ✅ You can explain why Epic 1 comes before Epic 2

---

**Document Version:** 1.0  
**Last Updated:** November 7, 2025  
**Author:** Product Owner (Sarah)

---

## Quick Links

- 📋 **Pre-Development Checklist:** [./pre-development-checklist.md](./pre-development-checklist.md)
- 🚀 **Quick Start Guide:** [./quick-start-guide.md](./quick-start-guide.md)
- 📊 **Epic Dependencies Map:** [./epic-dependencies-map.md](./epic-dependencies-map.md)
- 📖 **Product Requirements:** [../prd/index.md](../prd/index.md)
- 🏗️ **Architecture Documentation:** [../architecture/index.md](../architecture/index.md)
