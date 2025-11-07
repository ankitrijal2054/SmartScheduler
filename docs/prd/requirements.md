# Requirements

## Functional Requirements (FRs)

### Backend & Data Management

- **FR1**: System maintains contractor profiles with name, location, trade type, availability hours, and average customer rating
- **FR2**: System tracks contractor active/inactive status and supports adding/removing contractors from dispatcher's personal "Contractor List"
- **FR3**: System implements Availability Engine that calculates free time slots based on working hours + existing assigned jobs, preventing double-bookings
- **FR4**: System integrates with mapping API (Google Maps or OpenRouteService) to calculate travel distances and real-time travel times between job sites and contractor locations
- **FR5**: System implements Intelligent Scoring & Ranking Engine using weighted formula: `score = (availabilityWeight × availabilityScore) + (ratingWeight × ratingScore) + (distanceWeight × distanceScore)`
- **FR6**: System ranks available contractors and returns top 5 matches with scores, suggested time slots, travel time, and average ratings
- **FR7**: System supports contractor list filtering in recommendations (optional "contractor_list_only" parameter to show only dispatcher's curated contractors)
- **FR8**: System maintains job records with job type, location, time window, assigned contractor, and status (pending → assigned → in-progress → completed)
- **FR9**: System supports job rescheduling and contractor reassignment without data integrity issues
- **FR10**: System records customer ratings (1-5 stars) and optional text reviews after job completion
- **FR11**: System aggregates customer ratings to compute average contractor ratings used in scoring algorithm
- **FR12**: System publishes domain events (JobAssigned, JobCancelled, ScheduleUpdated, ContractorRated, JobCompleted) to message bus for system-wide coordination
- **FR13**: System provides `/api/recommendations` endpoint accepting job details and optional contractor list filter, returning ranked contractor recommendations in <500ms

### Dispatcher Interface

- **FR14**: Dispatcher can view all open jobs with status and assigned contractor information
- **FR15**: Dispatcher can request contractor recommendations for a job with one click
- **FR16**: Dispatcher sees ranked contractor recommendations with scores, availability windows, location/distance, travel time, and average ratings
- **FR17**: Dispatcher can confirm assignment with one click (immediately updates job status and triggers notifications)
- **FR18**: Dispatcher can reassign an assigned job to a different contractor in <2 minutes
- **FR19**: Dispatcher can view full contractor list with ratings, specializations, and locations
- **FR20**: Dispatcher can add contractors to personal "Contractor List" and remove contractors from it
- **FR21**: Dispatcher can toggle filter to show "all contractors" vs. "only contractors in my Contractor List"
- **FR22**: Dispatcher can view contractor history (past jobs, ratings, performance patterns)
- **FR23**: Dispatcher receives real-time in-app notifications for contractor declined jobs, jobs completed, customer ratings posted
- **FR24**: Dispatcher receives email notifications for critical events (contractor declined, job completed, customer rating posted)

### Customer Interface

- **FR25**: Customer can submit new jobs via form (job type, location, desired date/time, description)
- **FR26**: Customer sees real-time job status updates (submitted → assigned → in-progress → completed) with <5 second propagation latency
- **FR27**: Customer sees assigned contractor details (name, rating, reviews, estimated arrival time)
- **FR28**: Customer can view contractor profile and ratings before job starts
- **FR29**: Customer can rate contractor (1-5 stars) after job completion
- **FR30**: Customer can provide optional text review of job quality and contractor professionalism
- **FR31**: Customer receives email notification when job is assigned (includes contractor details, rating, reviews, ETA, direct link to app)
- **FR32**: Customer receives email notification with reminder to leave post-job feedback (auto-sent after job completion, includes direct rating link)

### Contractor Interface

- **FR33**: Contractor receives real-time in-app notification when job is assigned (<10 second latency)
- **FR34**: Contractor sees assigned job details (location, customer, time window, job type, estimated pay/rate)
- **FR35**: Contractor can view customer ratings before accepting/declining assignment
- **FR36**: Contractor can accept or decline job assignment in <1 minute
- **FR37**: Contractor can mark job as in-progress and as completed
- **FR38**: Contractor sees real-time updates if dispatcher reschedules or reassigns job
- **FR39**: Contractor can view job history (past jobs, completion status, customer ratings received)
- **FR40**: Contractor receives email notification when job is assigned (includes job details, customer profile, location, time window, direct accept/decline link)
- **FR41**: Contractor receives email notification of schedule changes or reassignment

### Platform & Messaging

- **FR42**: System publishes events to message bus with event type, timestamp, affected entities, and audit trail
- **FR43**: SignalR hub broadcasts job status changes to all connected clients in real-time
- **FR44**: SignalR hub broadcasts contractor acceptance/decline updates to relevant users (dispatcher, customer, contractor)
- **FR45**: System sends transactional emails via AWS SES or SendGrid with custom templates for each event type
- **FR46**: System maintains email audit log (sent, failed, retries) for troubleshooting and compliance

### Authentication & Authorization

- **FR47**: System supports three user roles: Dispatcher, Customer, Contractor
- **FR48**: System uses JWT-based authentication with role-based access control (RBAC)
- **FR49**: Dispatchers see only jobs and contractors relevant to them (can only assign jobs)
- **FR50**: Customers see only their own jobs and assigned contractor information (cannot see other customers' jobs)
- **FR51**: Contractors see only assigned jobs and their own job history (cannot see competitors' job details or other contractors' information)

## Non-Functional Requirements (NFRs)

### Performance

- **NFR1**: Recommendation API response time <500ms (end-to-end from request to ranked list)
- **NFR2**: Job assignment confirmation latency <2 seconds (dispatcher clicks "confirm" → job status updates)
- **NFR3**: Real-time event propagation latency <100ms via SignalR (job assigned → all affected clients notified)
- **NFR4**: Job status update propagation <5 seconds (customer sees update after status change)
- **NFR5**: Contractor notification latency <10 seconds (job assigned → contractor receives in-app + email notification)
- **NFR6**: Page load time <2 seconds for all portals (dispatcher dashboard, customer portal, contractor portal)
- **NFR7**: Email notification delivery <2 seconds after triggering event
- **NFR8**: Database query response time <100ms for all operational queries (contractor list, job status, etc.)

### Reliability & Availability

- **NFR9**: System uptime 99.5% (real-time coordination requires high reliability)
- **NFR10**: SignalR connection monitoring and automatic reconnection within 30 seconds if connection drops
- **NFR11**: Email notification retry logic (exponential backoff, max 3 retries over 24 hours if delivery fails)
- **NFR12**: Zero scheduling conflicts in 100+ test scenarios (availability engine prevents double-bookings)
- **NFR13**: Transactional consistency: job assignment immediately visible to all three roles or fails atomically (no partial updates)

### Scalability

- **NFR14**: System supports 1000+ concurrent users (dispatchers, customers, contractors) without degradation
- **NFR15**: System supports 10,000+ contractors in database with <500ms recommendation queries
- **NFR16**: Email service handles 1000+ emails/hour without failures
- **NFR17**: SignalR hub supports 1000+ concurrent WebSocket connections with graceful degradation

### Security & Data Privacy

- **NFR18**: All API communication via HTTPS with TLS 1.3
- **NFR19**: Secrets managed via AWS Secrets Manager (never hardcoded)
- **NFR20**: Role-based access control (RBAC) enforced at API layer (customers cannot access other customers' jobs)
- **NFR21**: Contractor data isolated: contractors cannot view competitors' job history or rates
- **NFR22**: Email addresses encrypted at rest in database
- **NFR23**: Location data (addresses, coordinates) handled securely with access controls

### Maintainability & Architecture

- **NFR24**: Backend follows Domain-Driven Design (DDD) with clear domain entities (Contractor, Customer, Job, Schedule, Review)
- **NFR25**: Backend implements CQRS (Command Query Responsibility Segregation) for clean separation of write and read models
- **NFR26**: Event-driven architecture with published domain events enabling future extensibility (Phase 2 features)
- **NFR27**: Code follows SOLID principles (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion)
- **NFR28**: Unit test coverage >80% for critical business logic (scoring algorithm, availability engine, job assignment workflow)
- **NFR29**: Integration test suite covers end-to-end workflows (customer submit → dispatcher assign → contractor notification → complete → feedback)
- **NFR30**: API documentation complete with endpoint contracts, request/response schemas, error codes
- **NFR31**: Code repository with clean commit history and descriptive commit messages (demonstrates development process)

### User Experience & Accessibility

- **NFR32**: All portals responsive across desktop (1920px+), tablet (768px), and mobile (375px) with appropriate layouts
- **NFR33**: WCAG AA accessibility compliance (keyboard navigation, color contrast, semantic HTML)
- **NFR34**: Page interactions provide immediate visual feedback (loading states, success/error messages, confirmation dialogs)
- **NFR35**: All critical actions have confirmation dialogs or undo capability (prevent accidental job reassignments)
- **NFR36**: Error messages are clear and actionable (not generic "Error 500" but "Job already assigned to another contractor")

### Browser Support & Compatibility

- **NFR37**: Support Chrome, Firefox, Safari, Edge (latest 2 versions)
- **NFR38**: React frontend uses modern hooks/functional components (no class components, demonstrating current best practices)
- **NFR39**: TypeScript for frontend with strict mode enabled (type safety demonstration)

### Deployment & DevOps

- **NFR40**: Infrastructure as Code (AWS resources defined in CloudFormation or Terraform)
- **NFR41**: CI/CD pipeline (GitHub Actions or similar) with automated testing on every commit
- **NFR42**: Blue-green deployment strategy (zero-downtime deployments)
- **NFR43**: Application monitoring and logging (CloudWatch or similar for production visibility)
- **NFR44**: Database migrations versioned and tracked (Liquibase or FluentMigrator for .NET)

---
