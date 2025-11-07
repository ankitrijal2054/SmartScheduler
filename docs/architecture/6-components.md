# 6. Components

SmartScheduler's backend and frontend are organized into logical components with clear responsibilities.

## 6.1 Backend Components

**API Layer:**

- Controllers (HTTP request handling, validation)
- Middleware (exception handling, logging)

**Application Layer:**

- MediatR Handlers (CQRS command/query processing)
- Scoring Engine (contractor ranking algorithm)
- Application Services (business logic orchestration)

**Domain Layer:**

- Domain Entities (Job, Contractor, Assignment, Review)
- Value Objects (Location, TimeSlot)
- Domain Events (JobAssigned, ContractorAccepted)

**Infrastructure Layer:**

- EF Core Repositories (data access)
- External Services (Google Maps API, AWS SES)
- SignalR Hub (real-time notifications)

## 6.2 Frontend Components

**UI Components:**

- Shared components (JobCard, ContractorCard, StatusBadge)
- Layout components (Header, Sidebar, Layout)
- Common components (LoadingSpinner, ErrorMessage)

**Feature Components:**

- Dispatcher features (JobList, RecommendationModal, ContractorListManager)
- Customer features (JobSubmissionForm, JobTracker, RatingForm)
- Contractor features (AssignmentList, JobActionButtons, ProfileStats)

**Services Layer:**

- API Client (Axios with interceptors)
- SignalR Client (real-time connection management)
- Service implementations (dispatcherService, customerService, contractorService)

**State Management:**

- Context Providers (AuthContext, NotificationContext)
- Custom Hooks (useAuth, useJobs, useContractors, useSignalR)

---
