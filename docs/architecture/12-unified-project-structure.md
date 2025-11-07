# 12. Unified Project Structure

```text
smartscheduler/
├── .github/workflows/        # CI/CD pipelines
├── backend/                  # .NET 8 API (modular monolith)
├── frontend/                 # React 18+ TypeScript
├── shared/types/             # Shared TypeScript types
├── infrastructure/cdk/       # AWS CDK infrastructure code
├── docs/                     # Documentation
├── scripts/                  # Setup and deployment scripts
├── package.json              # Root (npm workspaces)
└── README.md
```

---
