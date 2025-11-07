# 10. Frontend Architecture

## 10.1 Component Organization

```text
frontend/
├── src/
│   ├── components/           # Shared UI components
│   │   ├── ui/               # shadcn/ui components
│   │   ├── layout/           # Header, Sidebar, Layout
│   │   ├── common/           # LoadingSpinner, ErrorMessage
│   │   └── shared/           # JobCard, ContractorCard
│   ├── features/             # Role-specific features
│   │   ├── dispatcher/
│   │   ├── customer/
│   │   └── contractor/
│   ├── hooks/                # Custom React hooks
│   ├── contexts/             # React Context providers
│   ├── services/             # API client services
│   ├── types/                # TypeScript type definitions
│   └── routes/               # Route definitions
```

## 10.2 State Management

- **AuthContext:** User authentication state, login/logout actions
- **NotificationContext:** Real-time SignalR notifications
- **Custom Hooks:** useAuth, useJobs, useContractors, useSignalR

## 10.3 Routing

- Role-based routing with `<ProtectedRoute allowedRoles={['Dispatcher']} />`
- Lazy-loaded routes for code splitting
- Automatic role-based dashboard redirect after login

---
