# Technical Assumptions

## Repository Structure

**Decision: Monorepo** (backend + frontend in same repo)

**Rationale:** Simpler for portfolio review, single CI/CD pipeline, clear organization.

---

## Service Architecture

**Decision: Modular Monolith**

Single .NET 8 backend + React frontend. Backend organized by features (Contractors, Jobs, Scoring, Notifications, etc.) but deployed as one service.

---

## Technology Stack

| Layer                 | Technology                                        | Why                                                          |
| --------------------- | ------------------------------------------------- | ------------------------------------------------------------ |
| **Backend API**       | C# with .NET 8 ✅ MANDATORY                       | Focus your deep skill here; showcase C# expertise            |
| **Frontend**          | React 18+ with TypeScript                         | Standard choice; fast to build with libraries                |
| **Styling**           | Tailwind CSS                                      | Utility-first; fast to prototype                             |
| **UI Components**     | shadcn/ui or similar library (Mantine, Chakra UI) | Pre-built, professional, saves time. Don't reinvent buttons. |
| **Database**          | PostgreSQL                                        | Open-source, simple setup, standard choice                   |
| **Real-Time**         | SignalR                                           | Built into .NET; perfect for real-time coordination          |
| **Email**             | AWS SES                                           | AWS-native, simple, cost-effective                           |
| **API Documentation** | Swagger/OpenAPI                                   | Auto-generated from .NET; looks professional                 |
| **Frontend State**    | React Context API + useReducer                    | Sufficient for this app; Redux overkill                      |
| **Form Handling**     | React Hook Form                                   | Lightweight, no bloat                                        |
| **HTTP Client**       | Axios                                             | Simple, batteries included                                   |
| **Cloud Platform**    | AWS ✅ MANDATORY                                  | EC2/App Runner for backend, S3+CloudFront for frontend       |

## Frontend Stack (Simplified)

- **Framework**: React 18+ with Hooks (functional components)
- **Language**: TypeScript (strict mode for type safety)
- **UI Library**: **shadcn/ui** (or Mantine/Chakra UI — pick one and use it for all components)
- **Styling**: Tailwind CSS for custom layouts + shadcn components
- **Routing**: React Router v6
- **State Management**: React Context API + useReducer (no Redux)
- **Forms**: React Hook Form + shadcn form components
- **Date Picker**: shadcn Calendar component (built-in)
- **Notifications/Toasts**: react-hot-toast or shadcn sonner (quick, professional)
- **HTTP**: Axios with simple auth interceptor
- **Icons**: lucide-react (or shadcn icons)

## Backend Stack (Your Showcase)

- **Framework**: .NET 8 (latest)
- **Architecture**: Clean Architecture with layered approach
  - API Layer (controllers, DTOs)
  - Application Layer (services, business logic)
  - Domain Layer (entities, domain logic)
  - Infrastructure Layer (DB, email, external APIs)
- **Database**: Entity Framework Core (Code First)
- **Validation**: FluentValidation (expressive, reusable)
- **Dependency Injection**: Built-in .NET DI
- **Logging**: Serilog to CloudWatch (structured logging)
- **Authentication**: JWT + cookie-based sessions
- **SignalR**: Built-in real-time hub (no external deps)
- **Email Queue**: Simple in-memory background job (or Hangfire if you want to impress)

## Database Schema (Simple)

```
Contractors (id, name, location, tradeType, avgRating, isActive)
Customers (id, name, location, email)
Jobs (id, customerId, location, jobType, dateTime, status, assignedContractorId)
Assignments (id, jobId, contractorId, acceptedAt, completedAt)
Reviews (id, jobId, contractorId, rating, comment)
DispatcherContractorList (id, dispatcherId, contractorId)
```

Use Entity Framework Code First migrations to manage schema.

## Real-Time (SignalR - Keep It Simple)

- **Hub Groups**: By role (`dispatcher-123`, `customer-456`, `contractor-789`)
- **Events Sent**:
  - `JobAssigned` → notify affected parties
  - `JobStatusChanged` → notify affected parties
  - `ContractorAccepted` → notify affected parties

## Email Notifications

- **Service**: AWS SES (no additional setup needed beyond AWS account)
- **Approach**: Simple synchronous email on events (backend sends email directly)
- **Alternative**: Add AWS SQS if you want to show async patterns (email jobs queued, sent asynchronously)
- **Templates**: Simple HTML email templates (or use AWS SES template service)
- **Events**: JobAssigned, JobCancelled, JobCompleted, RatingPosted

## Testing (Reasonable Amount, Not Excessive)

- **Unit Tests**: Core business logic only
  - Scoring algorithm
  - Availability calculation
  - Job assignment logic
- **Integration Tests**: 3-5 happy path workflows
  - Job submission → Assignment → Completion
  - Contractor accept/decline
  - Customer rating
- **Tool**: xUnit for backend, Jest or Vitest for frontend

## Deployment (Simple & Fast)

- **Backend**:
  - Docker container
  - Deploy to AWS App Runner (easiest managed container option)
  - Auto-scales, no EC2 management
- **Frontend**:
  - Build React app (`npm run build`)
  - Deploy to S3 + CloudFront (2 minutes to set up)
- **Database**: AWS RDS PostgreSQL (managed, backups included)
- **CI/CD**: GitHub Actions
  - Run tests on PR
  - Deploy on merge to main

## Security (Basics, Not Overkill)

- HTTPS everywhere (CloudFront handles it)
- JWT tokens for API auth
- Role-based access control in backend (simple if/checks)
- API CORS configured for frontend domain
- AWS Secrets Manager for sensitive config (DB password, API keys)

## Documentation (Just Enough)

- **Swagger/OpenAPI**: Auto-generated from .NET (shows API contracts)
- **README**: How to run locally, deploy to AWS, tech stack
- **Comments in Code**: Only where logic is non-obvious (business logic, edge cases)

---
