# 11. Backend Architecture

## 11.1 Service Architecture (Modular Monolith)

**Project Structure:**

- **SmartScheduler.API** - Controllers, Middleware
- **SmartScheduler.Application** - Commands, Queries, Handlers, Services
- **SmartScheduler.Domain** - Entities, Value Objects, Events, Interfaces
- **SmartScheduler.Infrastructure** - Repositories, External Services, SignalR Hub

## 11.2 Clean Architecture Layers

**Dependency Flow:** API → Application → Domain ← Infrastructure

- **Domain Layer:** Pure business logic, no dependencies
- **Application Layer:** Use cases (CQRS commands/queries), orchestrates domain
- **Infrastructure Layer:** Data access, external APIs, real-time messaging
- **API Layer:** HTTP endpoints, authentication, validation

## 11.3 CQRS with MediatR

- Commands: `AssignJobCommand`, `SubmitReviewCommand`, `AcceptAssignmentCommand`
- Queries: `GetContractorRecommendationsQuery`, `GetJobsQuery`, `GetAssignmentsQuery`
- Handlers: Process commands/queries, publish domain events

---
