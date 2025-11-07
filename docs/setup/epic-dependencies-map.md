# Epic Dependencies & System Integration Map

**Purpose:** Visualize how epics depend on each other and explain why sequencing matters.

**Audience:** Developers, project managers, stakeholders.

---

## High-Level Epic Sequence

```
┌─────────────────────────────────────────────────────────┐
│ Epic 1: Foundation & Infrastructure (6 stories)        │
│ ✓ Project setup, Database, Auth, RBAC, AWS, CI/CD     │
│ ⏱️ Estimated: Week 1-2                                 │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│ Epic 2: Contractor Management & Scoring (6 stories)    │
│ ✓ CRUD, Availability Engine, Mapping API, Scoring    │
│ ⏱️ Estimated: Week 2-3                                 │
└──────────────────┬──────────────────────────────────────┘
                   │
         ┌─────────┼─────────┬──────────┐
         ▼         ▼         ▼          ▼
    Epic 3    Epic 4    Epic 5    Epic 6
   (Disp)    (Cust)    (Contr)  (Notif)
    │         │         │        │
    └─────────┼─────────┼────────┘
              ▼
         Epic 7: Testing, Performance, Deployment
         ✓ Integration tests, Performance tuning, Production setup
         ⏱️ Estimated: Week 4-5
```

---

## Detailed Dependency Analysis

### **Epic 1: Foundation & Infrastructure** 🏗️

**Duration:** 1.5-2 weeks | **Stories:** 6 | **Blockers for:** All others

#### Why Epic 1 First?

- **Project Structure:** Everything depends on clean architecture layers
- **Database Foundation:** Data schema required for all features
- **Authentication:** Every protected endpoint needs JWT + RBAC
- **CI/CD Pipeline:** Enables automated testing throughout project
- **Cannot proceed without:** No development possible without base infrastructure

#### Epic 1 Stories

```
1.1 Project Setup & Clean Architecture (Foundations)
  ├─ Creates: /API, /Application, /Domain, /Infrastructure layers
  ├─ Enables: All subsequent development
  └─ Duration: 2-3 days

1.2 Database Schema & Entity Framework Core (Data)
  ├─ Creates: Contractors, Customers, Jobs, Reviews tables
  ├─ Enables: Data persistence for all features
  └─ Duration: 2-3 days

1.3 Authentication & JWT (Security)
  ├─ Creates: Login endpoint, JWT tokens, refresh tokens
  ├─ Enables: Protected endpoints in Epics 2+
  └─ Duration: 2-3 days

1.4 Role-Based Access Control (Security)
  ├─ Creates: [Authorize(Roles = "...")] enforcement
  ├─ Enables: Data isolation (dispatcher ≠ customer ≠ contractor)
  └─ Duration: 1-2 days

1.5 AWS Infrastructure & Deployment (Infrastructure)
  ├─ Creates: RDS, App Runner, S3, CloudFront, Secrets Manager
  ├─ Enables: Production deployment pipeline
  └─ Duration: 2-3 days

1.6 CI/CD Pipeline (DevOps)
  ├─ Creates: GitHub Actions workflow, automated testing
  ├─ Enables: Confidence in deployments
  └─ Duration: 2-3 days
```

**End of Epic 1:** System can authenticate users and persist data to database.

---

### **Epic 2: Contractor Management & Scoring Engine** 🎯

**Duration:** 1.5-2 weeks | **Stories:** 6 | **Depends on:** Epic 1 | **Blockers for:** Epics 3, 4, 5

#### Why Epic 2 Second?

- **Business Logic Core:** Scoring algorithm is the system differentiator
- **Contractor Data:** All recommendations depend on contractor profiles
- **Performance Critical:** Needs optimization before building UIs that call it
- **Enables Epics 3+:** Dispatchers, customers, contractors all need contractor data

#### Epic 2 Stories

```
2.1 Contractor CRUD & Profile Management (API)
  ├─ Creates: /api/contractors endpoints
  ├─ Enables: Stories 2.2-2.6, Epic 3 recommendations
  └─ Dependencies: Epic 1 (Auth, Database)
  └─ Duration: 2 days

2.2 Availability Engine (Business Logic)
  ├─ Creates: CalculateAvailability() function
  ├─ Enables: Accurate scoring in Story 2.4
  ├─ Test: No double-bookings, buffer times respected
  └─ Duration: 3 days

2.3 Mapping API Integration (External Service)
  ├─ Creates: Google Maps Distance Matrix wrapper
  ├─ Enables: Distance scoring in 2.4
  ├─ Caches: 24h Redis cache to reduce API calls
  └─ Duration: 2 days

2.4 Intelligent Scoring & Ranking Algorithm (Core)
  ├─ Creates: /api/recommendations endpoint
  ├─ Score = 0.4×availability + 0.3×rating + 0.3×distance
  ├─ Returns: Top 5 contractors in <500ms
  ├─ Performance: Critical path, needs integration test baseline
  └─ Duration: 3 days

2.5 Dispatcher Contractor List Management (Feature)
  ├─ Creates: Favorite contractors per dispatcher
  ├─ Enables: Contractor filtering in Epic 3
  └─ Duration: 1-2 days

2.6 Rating Aggregation (Data Service)
  ├─ Creates: Average rating calculation
  ├─ Enables: Fair contractor ratings in scoring
  └─ Duration: 1-2 days
```

**End of Epic 2:** Core matching algorithm complete and performant.

---

### **Epic 3: Dispatcher Portal & Job Assignment** 👤

**Duration:** 1.5-2 weeks | **Stories:** 6 | **Depends on:** Epic 1, 2 | **Enables:** Epic 6 (notifications)

#### Why Epic 3?

- **UI for Dispatcher:** Frontend consuming contractor recommendations from Epic 2
- **Job Assignment:** Core workflow triggering notifications in Epic 6
- **Unblocks Customer:** Stories 3.1-3.3 create job assignment flow customers depend on

#### Epic 3 Stories

```
3.1 Dispatcher Dashboard UI & Job List View (Frontend)
  ├─ Creates: React component showing open jobs
  ├─ Displays: Job status, assigned contractor, date/time
  ├─ Depends: Epic 2.1 (contractor data), Epic 1.4 (RBAC)
  └─ Duration: 3 days

3.2 Contractor Recommendations & Ranking Display (Frontend)
  ├─ Creates: React component showing top 5 contractors
  ├─ Calls: /api/recommendations from Epic 2.4
  ├─ Displays: Score, rating, distance, travel time, availability
  └─ Duration: 2 days

3.3 One-Click Job Assignment Workflow (Feature)
  ├─ Creates: AssignJobCommand endpoint
  ├─ Updates: Job status to "Assigned"
  ├─ Triggers: Notifications (handled in Epic 5-6)
  ├─ Depends: Contractor data (Epic 2), Auth (Epic 1.3)
  └─ Duration: 2 days

3.4 Job Reassignment & Contractor Swap (Feature)
  ├─ Creates: ReassignJobCommand endpoint
  ├─ Triggers: Contractor notifications of changes
  ├─ Depends: Job assignment workflow (3.3)
  └─ Duration: 2 days

3.5 Contractor List Management UI (Frontend)
  ├─ Creates: UI for adding/removing favorite contractors
  ├─ Calls: Dispatcher contractor list endpoints (Epic 2.5)
  └─ Duration: 1-2 days

3.6 Contractor History & Performance View (Frontend)
  ├─ Creates: React component showing contractor past jobs
  ├─ Displays: Completion rate, average rating, job history
  └─ Duration: 2 days
```

**End of Epic 3:** Dispatcher can view jobs and intelligently assign contractors.

---

### **Epic 4: Job & Customer Portal** 👥

**Duration:** 1.5 weeks | **Stories:** 5 | **Depends on:** Epic 1, 2, 3 | **Enables:** Epic 6 (notifications)

#### Why Epic 4?

- **Customer UI:** React frontend for job submission and tracking
- **Depends on:** Assignment workflow (Epic 3) to show real-time updates
- **Enables Notifications:** Email/SignalR notifications for customer (Epic 6)

#### Epic 4 Stories

```
4.1 Customer Job Submission Form (Frontend)
  ├─ Creates: React form for new job creation
  ├─ Submits: CreateJobCommand to backend
  ├─ Depends: Epic 1 (Auth, RBAC), Epic 3 (assignment workflow)
  └─ Duration: 2 days

4.2 Customer Job Tracking & Real-Time Status Updates (Frontend + Real-Time)
  ├─ Creates: React component showing job status
  ├─ Updates: Real-time via SignalR (implemented in Epic 5.5)
  ├─ Shows: Job status, assigned contractor, ETA
  ├─ Depends: Epic 3 (assignment), Epic 5.5 (SignalR)
  └─ Duration: 2 days

4.3 Contractor Profile & Credibility View (Frontend)
  ├─ Creates: React component showing assigned contractor profile
  ├─ Displays: Name, rating, reviews, past jobs
  ├─ Depends: Contractor data (Epic 2)
  └─ Duration: 1-2 days

4.4 Customer Rating & Feedback Form (Frontend)
  ├─ Creates: React form for post-job rating
  ├─ Submits: RateContractorCommand
  ├─ Triggers: Rating aggregation (Epic 2.6), Notifications (Epic 6)
  └─ Duration: 1-2 days

4.5 Email Notifications to Customer (Backend + Integration)
  ├─ Sends: Email when job assigned, reminder to leave feedback
  ├─ Depends: AWS SES (Epic 1.5), Events (Epic 6.4)
  ├─ Implemented: In Epic 6.5 (Email Event Handler)
  └─ Duration: Deferred to Epic 6
```

**End of Epic 4:** Customer can submit jobs and track assignments.

---

### **Epic 5: Contractor Portal & Real-Time Notifications** 📱

**Duration:** 2 weeks | **Stories:** 6 | **Depends on:** Epic 1, 2, 3, 4 | **Enables:** Epic 6 (email notifications)

#### Why Epic 5?

- **Contractor UI:** React frontend for job acceptance/completion
- **Real-Time Coordination:** SignalR hub for sub-100ms updates
- **Enables Notifications:** Infrastructure for both in-app + email (Epic 6)

#### Epic 5 Stories

```
5.1 Contractor Job List & Notification Center (Frontend)
  ├─ Creates: React component showing available/assigned jobs
  ├─ Updates: Real-time via SignalR (5.5)
  ├─ Depends: Assignment workflow (Epic 3)
  └─ Duration: 2 days

5.2 Job Details Modal & Accept/Decline Workflow (Frontend)
  ├─ Creates: React modal showing job details, customer info
  ├─ Submits: AcceptJobCommand or DeclineJobCommand
  ├─ Triggers: Notifications to dispatcher, customer
  ├─ Depends: Epic 3 (assignment), Epic 1.3 (Auth)
  └─ Duration: 2 days

5.3 Job Status Management (In-Progress & Completion) (Feature)
  ├─ Creates: MarkInProgressCommand, MarkCompleteCommand
  ├─ Updates: Job status, triggers notifications
  ├─ Depends: Job assignment workflow (Epic 3)
  └─ Duration: 1-2 days

5.4 Contractor Rating & Earnings History (Frontend)
  ├─ Creates: React component showing past jobs, ratings received
  ├─ Displays: Customer reviews, earnings, completion rate
  ├─ Depends: Rating data (Epic 4.4), Job history
  └─ Duration: 1-2 days

5.5 Real-Time Job Notifications (SignalR) ⭐ CRITICAL
  ├─ Creates: SignalR Hub for job updates
  ├─ Enables: <100ms latency for all three portals
  ├─ Broadcasts: Job assigned, accepted, completed, reassigned
  ├─ Depends: Core job commands (Epic 3, 5.2-5.3)
  ├─ Note: Complex, requires careful testing
  └─ Duration: 3 days

5.6 Contractor Email Notifications (Backend Integration)
  ├─ Sends: Email when job assigned, when reassigned
  ├─ Depends: AWS SES (Epic 1.5), Events (Epic 6.4)
  ├─ Implemented: In Epic 6.5 (Email Event Handler)
  └─ Duration: Deferred to Epic 6
```

**End of Epic 5:** Contractor portal complete with real-time updates via SignalR.

---

### **Epic 6: Reviews, Email & System Coordination** 📧

**Duration:** 1 week | **Stories:** 6 | **Depends on:** All Epics 1-5 | **Final MVP**

#### Why Epic 6?

- **Event Coordination:** Ties all portals together via domain events
- **Email Service:** AWS SES notifications for critical events
- **System Maturity:** Adds reliability, observability, coordination

#### Epic 6 Stories

```
6.1 Customer Rating & Review System (Feature)
  ├─ Creates: Review entity and commands
  ├─ Stores: 1-5 star rating, optional text review
  ├─ Depends: Customer portal (Epic 4)
  └─ Duration: 1-2 days

6.2 Contractor Rating Aggregation & Display (Feature)
  ├─ Creates: Average rating calculation (Epic 2.6 detail)
  ├─ Updates: Contractor record with latest average
  ├─ Triggers: Scoring algorithm to re-rank
  ├─ Depends: Rating system (6.1)
  └─ Duration: 1 day

6.3 Email Service Setup & Configuration (Infrastructure)
  ├─ Configures: AWS SES sender identity, templates
  ├─ Tests: Email sending works
  ├─ Depends: Epic 1.5 (AWS infrastructure)
  └─ Duration: 1 day

6.4 Event Publishing & Domain Events (Architecture)
  ├─ Creates: JobAssigned, JobAccepted, JobCompleted, JobRated events
  ├─ Enables: Event-driven architecture for notifications
  ├─ Pub-Sub: In-memory event bus (publish → subscribers handle)
  └─ Duration: 2 days

6.5 Email Event Handler & Notifications (Integration)
  ├─ Subscribes: To domain events
  ├─ Sends: Email on JobAssigned → customer, contractor
  ├─ Sends: Email reminder → customer after job completion
  ├─ Depends: Event publishing (6.4), SES setup (6.3)
  └─ Duration: 2 days

6.6 SignalR Real-Time Coordination (Feature)
  ├─ Publishes: Events via SignalR Hub
  ├─ Updates: All three portals in <100ms
  ├─ Depends: Event system (6.4), SignalR setup (5.5)
  └─ Duration: 1-2 days
```

**End of Epic 6:** MVP complete! All three portals coordinated, real-time + email notifications working.

---

### **Epic 7: Testing, Performance & Production Deployment** 🚀

**Duration:** 1-2 weeks | **Stories:** 7 | **Depends on:** All Epics 1-6 | **Launch Ready**

#### Why Epic 7 Last?

- **Integration Tests:** Test complete workflows (all epics must exist)
- **Performance Optimization:** Baseline set in Epic 2; optimization after all features
- **Production Readiness:** CI/CD, monitoring, documentation complete

#### Epic 7 Stories

```
7.1 Integration Test Suite - Full Workflows (QA)
  ├─ Tests: Customer submit → dispatcher assign → contractor accept → complete → feedback
  ├─ Coverage: All three roles, all critical paths
  ├─ Tools: xUnit backend, Playwright E2E frontend
  ├─ Depends: All features (Epics 1-6)
  └─ Duration: 3-4 days

7.2 Performance Testing & Optimization (DevOps)
  ├─ Benchmarks: Recommendation queries <500ms, SignalR <100ms
  ├─ Optimizes: Database indexes, Redis caching, query optimization
  ├─ Depends: Baseline from Epic 2.4 integration test
  └─ Duration: 2-3 days

7.3 Error Handling & User Feedback (UX)
  ├─ Polishes: Error messages, loading states, confirmation dialogs
  ├─ Adds: Proper error boundary components
  ├─ Depends: All features
  └─ Duration: 1-2 days

7.4 Security Review & Hardening (Security)
  ├─ Reviews: JWT implementation, RBAC enforcement, SQL injection risks
  ├─ Hardens: Input validation, CORS configuration, rate limiting
  ├─ Depends: All backend features (Epics 1-3, 5-6)
  └─ Duration: 2-3 days

7.5 CI/CD Pipeline & Automated Deployment (DevOps)
  ├─ Configures: GitHub Actions workflow
  ├─ Deploys: Backend → App Runner, Frontend → S3 + CloudFront
  ├─ Depends: Epic 1.6 initial setup
  └─ Duration: 1-2 days

7.6 Documentation & Portfolio Preparation (Docs)
  ├─ Writes: API documentation (Swagger complete)
  ├─ Writes: Architecture decision records
  ├─ Prepares: README, setup guides, deployment docs
  ├─ For: Portfolio demonstration, code review
  └─ Duration: 2 days

7.7 Polish & Edge Case Handling (QA)
  ├─ Tests: Mobile responsiveness, browser compatibility
  ├─ Fixes: Edge cases, off-by-one errors, race conditions
  ├─ Cleans: Code cleanup, removed logging, final review
  └─ Duration: 1-2 days
```

**End of Epic 7:** MVP complete, production-ready, deployed to AWS.

---

## Cross-Epic Data Flows

### Job Assignment Flow (E2E)

```
Customer (Epic 4.1) → Job Created
    ↓
Dispatcher (Epic 3.2) → Requests Recommendations
    ↓
Backend (Epic 2.4) → Calls Scoring Algorithm
    ├─ Checks: Availability (Epic 2.2)
    ├─ Gets: Distance (Epic 2.3)
    ├─ Ranks: Available contractors (Epic 2.4)
    ↓
Dispatcher (Epic 3.3) → Clicks "Assign" button
    ↓
Backend → Publishes JobAssigned event (Epic 6.4)
    ├─ Handler 1 → Send Email (Epic 6.5)
    ├─ Handler 2 → Publish SignalR (Epic 6.6)
    ↓
Contractor (Epic 5.1) → Receives real-time notification (Epic 5.5)
    ↓
Contractor (Epic 5.2) → Accepts/Declines job
    ↓
Backend → Publishes JobAccepted event (Epic 6.4)
    ├─ Handler 1 → Send Email
    ├─ Handler 2 → Publish SignalR
    ↓
Customer (Epic 4.2) → Sees real-time status update
    ├─ Via SignalR (Epic 4.2 + 6.6)
    ├─ Or email notification (Epic 6.5)
    ↓
Contractor (Epic 5.3) → Marks job complete
    ↓
Backend → Publishes JobCompleted event
    ↓
Customer (Epic 4.4) → Leaves rating/review
    ↓
Backend → Publishes RatingPosted event
    ├─ Updates: Contractor average rating (Epic 6.2)
    ├─ Affects: Future recommendation rankings (Epic 2.4)
    ↓
System coordination complete ✅
```

---

## Why This Sequence?

| Epic | Dependencies | Why First?                                                         |
| ---- | ------------ | ------------------------------------------------------------------ |
| 1    | None         | Foundation for everything else                                     |
| 2    | Epic 1       | Scoring engine is business core; needed by dispatchers & customers |
| 3    | 1, 2         | Dispatcher UI consumes contractor recommendations                  |
| 4    | 1, 2, 3      | Customer UI consumes assignment workflow from Epic 3               |
| 5    | 1, 2, 3, 4   | Contractor UI + real-time coordination; needs all prior workflows  |
| 6    | 1-5          | Event coordination layer; needs all features to integrate          |
| 7    | 1-6          | Testing & optimization; all features must exist                    |

---

## Parallel Development Opportunities

**After Epic 2 completes**, Epics 3, 4, 5 can start in parallel:

```
Epic 2 completes (Day 14)
    ├─ Dev 1 → Epic 3 (Dispatcher portal)
    ├─ Dev 2 → Epic 4 (Customer portal)
    └─ Dev 3 → Epic 5 (Contractor portal + SignalR)

All merge back → Epic 6 (Event coordination)
```

**However,** if solo developer: Follow strict sequence 1→2→3→4→5→6→7.

---

## Risk Points & Mitigation

| Epic    | Risk                                   | Mitigation                                                       |
| ------- | -------------------------------------- | ---------------------------------------------------------------- |
| 2.4     | Scoring algorithm slow (<500ms target) | Perf test in Epic 2.4; cache layer; optimize queries in Epic 7.2 |
| 5.5     | SignalR real-time coordination complex | Start early (Epic 5); thorough testing; baseline from Epic 2.4   |
| 6.4-6.6 | Event coordination bugs                | Integration tests in Epic 7.1; trace event flow end-to-end       |
| 7.1     | Integration tests flaky                | Use Playwright fixtures; mock external APIs (Google Maps)        |

---

## Validation Checklist

Use this to validate dependencies are correct:

- [ ] Epic 1 Database schema covers all entities from Epics 2-6
- [ ] Epic 2 endpoints return all data needed by Epics 3, 4, 5
- [ ] Epic 3 assignment workflow triggers events for Epic 6
- [ ] Epic 4 job submission creates Job entity from Epic 1 schema
- [ ] Epic 5 SignalR hub publishes events from Epic 6
- [ ] Epic 6 event handlers subscribe to all domain events
- [ ] Epic 7 integration tests cover all Epic 1-6 workflows
- [ ] No circular dependencies exist

---

**Document Version:** 1.0  
**Last Updated:** November 7, 2025  
**Author:** Product Owner (Sarah)
