# Epic Details

## Epic 1: Foundation & Infrastructure

**Expanded Goal:** Establish the foundation for the entire system. Set up .NET 8 backend project with clean architecture, PostgreSQL database with core schemas, JWT authentication, basic API structure, AWS infrastructure (RDS, App Runner, S3), and CI/CD pipeline. This epic enables all subsequent work.

### Story 1.1: Project Setup & Clean Architecture

**As a** developer,  
**I want** a well-organized .NET 8 project with clean architecture layers,  
**so that** I can build features following established patterns and maintain code quality throughout the project.

**Acceptance Criteria:**

1. .NET 8 project created with folder structure: `/API`, `/Application`, `/Domain`, `/Infrastructure`
2. Each layer has clear responsibility: Domain (entities, domain logic), Application (services, DTOs), Infrastructure (DB, external services), API (controllers, middleware)
3. Dependency injection configured in .NET DI container (Program.cs)
4. SOLID principles demonstrated (e.g., interfaces for services, no cross-layer dependencies)
5. Global exception handler middleware in place (catches errors and returns standard error responses)
6. Logging configured (Serilog) and integrated into startup
7. API returns structured responses with consistent error format (code, message, statusCode)
8. GitHub repo initialized with .gitignore configured for .NET projects
9. Local development can run with `dotnet run` without configuration

### Story 1.2: Database Schema & Entity Framework Core

**As a** developer,  
**I want** a PostgreSQL database with all core entities and relationships defined via EF Core Code First,  
**so that** I have a solid data foundation for contractor, customer, job, and rating data.

**Acceptance Criteria:**

1. PostgreSQL connection string configured in appsettings.json (local dev, AWS RDS prod)
2. Entity Framework Core DbContext created with DbSets for: Contractors, Customers, Jobs, Assignments, Reviews, DispatcherContractorList, Users
3. All relationships configured (e.g., Job → Customer, Job → Contractor, Review → Job)
4. Initial migration created and applied (`Add-Migration Initial`, `Update-Database`)
5. Migration script can be run fresh to recreate full schema
6. Seeding script available to populate test data (5 contractors, 3 customers, sample jobs)
7. Database diagram generated or documented (entity relationships visible)
8. All migrations tracked in code (reproducible, versionable)

### Story 1.3: Authentication & JWT

**As a** user,  
**I want** to authenticate via email/password and receive a JWT token,  
**so that** I can access protected API endpoints and maintain a secure session.

**Acceptance Criteria:**

1. User entity created with Id, Email, PasswordHash, Role (Dispatcher, Customer, Contractor), IsActive
2. Authentication endpoint: `POST /api/v1/auth/login` accepts email + password, returns JWT token + refresh token
3. JWT payload includes: UserId, Email, Role (used for RBAC in protected endpoints)
4. JWT token expires in 1 hour; refresh token valid for 7 days
5. Protected endpoints require valid JWT (Authorization: Bearer token header)
6. Invalid/expired token returns 401 Unauthorized
7. Password hashing uses BCrypt (not plaintext, not MD5)
8. Refresh endpoint: `POST /api/v1/auth/refresh` returns new JWT token given valid refresh token
9. Logout endpoint: `POST /api/v1/auth/logout` invalidates refresh token

### Story 1.4: Role-Based Access Control (RBAC)

**As a** API developer,  
**I want** to enforce role-based permissions on endpoints,  
**so that** customers can't access dispatcher features, and contractors can't see competitor job details.

**Acceptance Criteria:**

1. [Authorize(Roles = "Dispatcher")] attribute on dispatcher-only endpoints
2. [Authorize(Roles = "Customer")] attribute on customer-only endpoints
3. [Authorize(Roles = "Contractor")] attribute on contractor-only endpoints
4. Unauthenticated requests to protected endpoints return 401
5. Authenticated requests without required role return 403 Forbidden
6. Data queries filtered by role (e.g., customer sees only their own jobs)
7. No accidental data leaks (e.g., contractor can't query all contractors' ratings in GET request)
8. Test cases verify RBAC enforcement (e.g., contractor token on dispatcher endpoint returns 403)

### Story 1.5: AWS Infrastructure & Deployment Foundation

**As a** developer,  
**I want** AWS infrastructure set up (RDS PostgreSQL, App Runner, S3, CloudFront) and ready for deployment,  
**so that** I can deploy the backend and frontend to production.

**Acceptance Criteria:**

1. AWS account configured with appropriate IAM roles and policies
2. RDS PostgreSQL instance created (dev and prod environments) with backups enabled
3. App Runner service created pointing to ECR (backend Docker image)
4. S3 bucket created for frontend static assets
5. CloudFront distribution configured to serve S3 frontend
6. Database connection string available as environment variable (Secrets Manager)
7. Docker setup: Dockerfile for .NET backend (multi-stage build, minimal image)
8. Docker image builds locally and runs without errors
9. Basic infrastructure documentation (how to deploy, what AWS services used)

### Story 1.6: CI/CD Pipeline (GitHub Actions)

**As a** developer,  
**I want** automated testing and deployment on every commit,  
**so that** I catch regressions early and can deploy with confidence.

**Acceptance Criteria:**

1. GitHub Actions workflow file created (.github/workflows/ci-cd.yml)
2. On PR: Run tests (backend + frontend), lint code, build Docker image (don't push)
3. On merge to main: Run tests, build Docker image, push to ECR, deploy to App Runner
4. Frontend build on merge: Runs `npm run build`, uploads to S3, invalidates CloudFront cache
5. Deployment notifications (Slack or email) on success/failure
6. Secrets (AWS credentials, DB password) stored in GitHub Secrets (not hardcoded)
7. CI/CD workflow shows passing status on main branch

---

## Epic 2: Contractor Management & Scoring Engine

**Expanded Goal:** Implement core contractor management features and the intelligent scoring algorithm that ranks contractors for job assignments. This includes contractor CRUD, availability calculation, distance calculation via mapping API, and the weighted scoring formula. The scoring engine is the system's differentiator and must be performant and accurate.

### Story 2.1: Contractor CRUD & Profile Management

**As a** dispatcher,  
**I want** to manage contractor profiles (create, read, update, deactivate),  
**so that** I can maintain an accurate list of available contractors and their specializations.

**Acceptance Criteria:**

1. `POST /api/v1/contractors` creates new contractor (name, location, tradeType, workingHours, phone)
2. `GET /api/v1/contractors` returns list of all active contractors with pagination (limit 50)
3. `GET /api/v1/contractors/{id}` returns single contractor with full profile (name, rating, reviews count, location, tradeType)
4. `PUT /api/v1/contractors/{id}` updates contractor (name, location, workingHours, etc.)
5. `PATCH /api/v1/contractors/{id}/deactivate` marks contractor as inactive (soft delete)
6. Deactivated contractors don't appear in recommendation list
7. Contractor average rating is calculated from all reviews (e.g., 3 reviews: 5, 4, 4 → avg 4.33)
8. Location field accepts address string; coordinates stored for distance calculations
9. All endpoints require JWT authentication; only dispatchers/admins can create/modify contractors

### Story 2.2: Availability Engine

**As a** dispatcher,  
**I want** the system to automatically calculate contractor free time slots,  
**so that** the scoring algorithm knows when contractors are available for new jobs.

**Acceptance Criteria:**

1. Contractor has working hours (e.g., 9 AM - 5 PM Monday-Friday)
2. When job is assigned to contractor, occupies time slot (e.g., 2 PM - 4 PM on Nov 15)
3. `CalculateAvailability(contractorId, desiredDateTime)` returns true/false (is contractor free?)
4. Availability considers job duration + travel time (e.g., if job is 2 hours + 30 min travel = 2.5 hour block)
5. No double-bookings: overlapping time slots prevented
6. Contractor can have multiple jobs on same day (e.g., 9-11 AM job, 2-4 PM job, both allowed)
7. Availability engine handles edge cases: job ends at noon, next job starts at noon (no gap) → should fail (no buffer time)
8. Unit tests verify: no overlaps, buffer time enforced, edge cases handled

### Story 2.3: Mapping API Integration (Distance & Travel Time)

**As a** the scoring algorithm,  
**I want** real-time distance and travel time data between job site and contractor location,  
**so that** I can rank contractors by proximity accurately.

**Acceptance Criteria:**

1. Google Maps API credentials configured in AWS Secrets Manager
2. `GetDistance(jobLocation, contractorLocation)` returns distance in miles (or km)
3. `GetTravelTime(jobLocation, contractorLocation)` returns travel time in minutes
4. API calls cached (Redis) for 24 hours (same location pairs don't re-query repeatedly)
5. Graceful fallback if API is down: return default distance (50 miles) + log error
6. Batch API calls where possible (multiple distance queries in one request)
7. Error handling: invalid addresses return error response (not crash)
8. Unit tests mock Google Maps API (don't make real API calls in tests)

### Story 2.4: Intelligent Scoring & Ranking Algorithm

**As a** dispatcher,  
**I want** contractors ranked intelligently by availability, proximity, and quality,  
**so that** I can confidently assign jobs to the best-fit contractor with one click.

**Acceptance Criteria:**

1. Scoring formula: `score = (0.4 × availabilityScore) + (0.3 × ratingScore) + (0.3 × distanceScore)`
   - availabilityScore: 1.0 if available, 0.0 if not
   - ratingScore: normalized 0-1 (e.g., 4.5 stars / 5.0 = 0.9)
   - distanceScore: normalized 0-1 (closer = higher; e.g., 5 miles / 50 miles = 0.1)
2. `GET /api/v1/recommendations` accepts: jobType, jobLocation, desiredDateTime, optional contractor_list_only filter
3. Returns top 5 available contractors ranked by score (highest first)
4. Response includes: contractorId, name, score, rating, distance, travelTime, availableTimeSlots
5. Response time <500ms (even with 10,000 contractors in database)
6. Contractor List Filter: if contractor_list_only=true, only rank contractors in dispatcher's personal list
7. If no contractors available, return empty list with message "No available contractors"
8. Unit tests verify scoring accuracy (known test cases produce expected rankings)
9. Integration test: 100 contractor records, 10 queries → response time <500ms (performance baseline)

### Story 2.5: Dispatcher Contractor List Management

**As a** dispatcher,  
**I want** to curate a personal list of preferred contractors,  
**so that** I can filter recommendations to only trusted, reliable contractors.

**Acceptance Criteria:**

1. `POST /api/v1/dispatcher/contractor-list/{contractorId}` adds contractor to dispatcher's list
2. `DELETE /api/v1/dispatcher/contractor-list/{contractorId}` removes contractor from dispatcher's list
3. `GET /api/v1/dispatcher/contractor-list` returns dispatcher's curated list
4. Added contractor is immediately available in filtered recommendations
5. Removed contractor is immediately hidden from filtered recommendations (if filter is active)
6. No error if contractor already in list (idempotent)
7. No error if removing contractor not in list (idempotent)
8. Data isolation: dispatchers only see their own contractor list
9. Contractor list persists across sessions

### Story 2.6: Rating Aggregation & Average Score Calculation

**As a** the scoring algorithm,  
**I want** contractor average ratings computed accurately and updated when new reviews posted,  
**so that** contractor quality is reflected in rankings.

**Acceptance Criteria:**

1. When customer posts rating (1-5 stars), calculate new average for contractor
2. Average rating stored on Contractor entity (avgRating decimal field)
3. `UPDATE contractor.avgRating = (SUM(ratings) / COUNT(reviews))`
4. Partial ratings (e.g., 4.5 stars) supported (decimal precision)
5. Single review: contractor rating = that review score
6. 3 reviews (5, 4, 5 stars): average = 4.67
7. Rating updates trigger recommendation cache invalidation (next query includes new rating)
8. Historical ratings not changeable (once posted, rating is permanent)
9. Contractor with no reviews has avgRating = null (handled gracefully in scoring)

---

## Epic 3: Dispatcher Portal & Job Assignment

**Expanded Goal:** Build the dispatcher dashboard UI with job list, contractor recommendations display, one-click assignment workflow, and contractor list management. This is where dispatchers get their productivity boost (reducing 30-60 min assignment time to <5 min).

### Story 3.1: Dispatcher Dashboard UI & Job List View

**As a** dispatcher,  
**I want** to see all open jobs in one dashboard view,  
**so that** I can quickly identify jobs needing assignment and prioritize my work.

**Acceptance Criteria:**

1. Dashboard loads after dispatcher logs in (role-based redirect)
2. Job list displays: Job ID, Customer Name, Location, Desired DateTime, Current Status (pending/assigned/in-progress/completed)
3. Jobs sorted by desired DateTime (earliest first)
4. Pending jobs highlighted (different background color or badge)
5. Assigned jobs show assigned contractor name and status
6. Job list paginated or scrollable (>10 jobs possible)
7. Responsive on desktop (1920px) and tablet (768px); mobile optional
8. Real-time updates: new jobs appear instantly when submitted by customers
9. Loading states visible (skeleton loaders while fetching data)
10. Empty state message if no jobs: "No jobs at this time"

### Story 3.2: Contractor Recommendations & Ranking Display

**As a** dispatcher,  
**I want** to request contractor recommendations for a job with one click,  
**so that** I can see the best contractors ranked and make a decision fast.

**Acceptance Criteria:**

1. Job row has "Get Recommendations" button
2. Click button → API call to `/api/v1/recommendations` (job details, optional contractor_list_only filter)
3. Modal/drawer opens showing top 5 recommended contractors
4. Each contractor card displays: Rank (1-5), Name, Rating (stars + count), Distance (miles), Travel Time (mins), Availability Slot
5. Contractor cards sortable by any field (rating, distance, etc.)
6. Loading spinner while API fetches recommendations (<500ms expected)
7. Error handling: "No available contractors" message if API returns empty list
8. Contractor card is clickable to view full profile (expanded details if desired)
9. Modal closes on background click or explicit close button
10. Recommendation request doesn't assign yet (just displays options)

### Story 3.3: One-Click Job Assignment Workflow

**As a** dispatcher,  
**I want** to assign a contractor with one click,  
**so that** I can complete the assignment in seconds.

**Acceptance Criteria:**

1. Recommendation modal has "Assign" button on each contractor card
2. Click "Assign" → Confirmation dialog: "Assign [Contractor Name] to [Job Details]?"
3. Confirm → API call `POST /api/v1/jobs/{jobId}/assign` with contractorId
4. Backend updates Job status = "assigned" and creates Assignment record
5. Success toast: "Job assigned to [Contractor Name]"
6. Job list refreshes: job now shows assigned contractor, status changed to "assigned"
7. Contractor is notified in real-time (SignalR) + receives email
8. Customer is notified in real-time + receives email
9. Modal closes automatically on successful assignment
10. If assignment fails (contractor no longer available), error message: "Contractor no longer available; please try again"

### Story 3.4: Job Reassignment & Contractor Swap

**As a** dispatcher,  
**I want** to reassign an already-assigned job to a different contractor,  
**so that** I can handle last-minute changes (contractor cancellation, etc.) quickly.

**Acceptance Criteria:**

1. Assigned job row shows "Reassign" button (in addition to job details)
2. Click "Reassign" → Same recommendations flow (get top 5 contractors)
3. Reassignment creates new Assignment record, marks previous as cancelled
4. Old contractor notified: "Your assignment for [Job] has been reassigned"
5. New contractor notified: "You've been assigned [Job]"
6. Customer notified: "Your job has been reassigned to [New Contractor Name]"
7. All parties notified in real-time + email
8. Job list updates to show new contractor
9. Reassignment completes in <2 minutes (dispatcher clicks reassign → confirms new contractor)
10. Reassignment history visible if desired (optional: show reassignment trail)

### Story 3.5: Contractor List Management UI

**As a** dispatcher,  
**I want** to manage my personal contractor list directly in the UI,  
**so that** I can curate trusted contractors and filter recommendations.

**Acceptance Criteria:**

1. "Contractor List" section in dispatcher dashboard (separate tab or panel)
2. Displays current contractor list: Name, Rating, Location, Actions (Remove)
3. Shows all available contractors as searchable/filterable list
4. "Add" button next to each contractor (or checkboxes for multi-select add)
5. Click "Add" → Contractor moves to "My Contractor List"
6. Click "Remove" → Contractor removed from list (with confirmation)
7. Toggle button/checkbox: "Filter recommendations by my list only" (contractor_list_only=true)
8. When filter enabled, recommendation API only returns contractors from dispatcher's list
9. Adding contractor is instant; removing contractor instant
10. List persists across sessions (backend stores in DB)

### Story 3.6: Contractor History & Performance View

**As a** dispatcher,  
**I want** to see contractor history (past jobs, ratings, patterns),  
**so that** I can make informed decisions about reliability and specialization fit.

**Acceptance Criteria:**

1. Click on contractor card (in recommendations or contractor list) → Profile view opens
2. Profile shows: Name, Location, TradeType, Current Rating, Phone
3. Job history table: Date, Job Type, Customer Name, Completion Status, Customer Rating
4. Last 10 jobs shown (paginated if more)
5. Stats: Total jobs assigned, Completed jobs count, Acceptance rate (%), Avg rating
6. If contractor has low rating or high cancellation rate, visible warning (optional: flag for attention)
7. Close profile → returns to recommendations or contractor list
8. Mobile-friendly profile view

---

## Epic 4: Job & Customer Portal

**Expanded Goal:** Build customer-facing portal where customers submit jobs, track status in real-time, see contractor details, and provide feedback. Emphasis on transparency and trust-building.

### Story 4.1: Customer Job Submission Form

**As a** customer,  
**I want** to submit a job through a simple form,  
**so that** I don't need to call and can request work at my convenience.

**Acceptance Criteria:**

1. Job submission page loads with form: Job Type, Location, Desired Date/Time, Description
2. Job Type dropdown: Flooring, HVAC, Plumbing, Electrical, Other
3. Location: Address input field (or address autocomplete via Google Maps)
4. Desired Date/Time: Date + time picker (or simple datetime input)
5. Description: Text area for additional details (e.g., "3-room install, hardwood")
6. Submit button: "Submit Job"
7. Validation: All required fields present before submit
8. Loading state: spinner while submitting
9. Success: "Job submitted! We're finding contractors now."
10. Customer redirected to job tracking view (see their job in real-time)
11. Job appears in dispatcher dashboard immediately

### Story 4.2: Customer Job Tracking & Real-Time Status Updates

**As a** customer,  
**I want** to see my job status update in real-time,  
**so that** I know where things stand without calling.

**Acceptance Criteria:**

1. Customer sees job status as visual timeline or large status badge: "Submitted" → "Contractor Assigned" → "In Progress" → "Completed"
2. Job details shown: Job Type, Location, Desired Date/Time
3. Assigned contractor info displayed prominently (when available):
   - Contractor name, photo (optional), rating
   - Estimated arrival time (ETA)
   - Phone number (so customer can call if needed)
4. Status updates in real-time via SignalR (<5 second latency):
   - When dispatcher assigns → "Contractor Assigned" status appears
   - When contractor marks in-progress → status changes to "In Progress"
   - When contractor marks complete → status changes to "Completed"
5. Customer can see if job is in progress (not speculative)
6. If job is reassigned, customer sees new contractor details instantly
7. Refresh on page load shows current status
8. Mobile-responsive view

### Story 4.3: Contractor Profile & Credibility View

**As a** customer,  
**I want** to see contractor details and ratings before work starts,  
**so that** I know I can trust the person coming to my home.

**Acceptance Criteria:**

1. When contractor is assigned, contractor card is prominent on job tracking page
2. Click contractor card → Profile modal/drawer opens
3. Profile shows: Name, Photo (if available), Rating (stars + count), Reviews (text reviews from other customers)
4. Sample reviews displayed: "John was professional and quick" - Jane K., Nov 2025, ⭐⭐⭐⭐⭐
5. Average rating highlighted (e.g., 4.8/5 stars, "Excellent")
6. If contractor has low rating: neutral tone (not scary, just factual)
7. Modal closeable; returns to job tracking
8. Customer can contact contractor via phone or (optional) in-app message

### Story 4.4: Customer Rating & Feedback Form

**As a** customer,  
**I want** to rate the contractor and job after completion,  
**so that** my feedback helps improve the system and informs other customers.

**Acceptance Criteria:**

1. After job marked "Completed", job tracking page shows: "Rate this job" section
2. Rating form displays: 1-5 star rating selector, optional text review field
3. Stars are clickable (click star to select rating)
4. Text field: "Tell us about your experience" (e.g., "Great work, finished early!")
5. Submit button: "Submit Rating"
6. Validation: Rating required; text review optional
7. Success message: "Thank you for your feedback!"
8. Rating submitted → contractor receives notification + email
9. Rating visible to future customers (appears in contractor profile reviews)
10. Customer can view their own submitted ratings (history of jobs rated)

### Story 4.5: Email Notifications to Customer

**As a** customer,  
**I want** to receive email notifications for critical job events,  
**so that** I'm informed even if I don't check the app frequently.

**Acceptance Criteria:**

1. Email sent when job assigned: "Your job has been assigned!" with contractor name, rating, phone, ETA
2. Email includes direct link to job tracking page (one-click view)
3. Email sent when job in-progress: "[Contractor Name] is on their way!"
4. Email sent when job completed: "Your job is complete! Please rate [Contractor Name]"
5. Rating reminder email: "We'd love your feedback on your recent job" with one-click rating link
6. All emails include job details (date, location, contractor info)
7. Email template is professional and mobile-friendly
8. Customer can unsubscribe from emails (optional for MVP)

---

## Epic 5: Contractor Portal & Real-Time Notifications

**Expanded Goal:** Build contractor-facing portal with job list, job details before accepting, accept/decline workflow, job completion tracking, and real-time SignalR notifications. Emphasis on simplicity and adoption.

### Story 5.1: Contractor Job List & Notification Center

**As a** contractor,  
**I want** to see assigned jobs in one place and get notified when new assignments arrive,  
**so that** I don't miss opportunities and can manage my schedule.

**Acceptance Criteria:**

1. Contractor dashboard shows job list: Current/Upcoming jobs, Active jobs (in progress), Completed jobs
2. Each job shows: Job Type, Location, Scheduled Time, Customer Name, Status (accepted/pending/in-progress/completed)
3. Assigned but unaccepted jobs highlighted (pending contractor action)
4. Real-time notification badge/alert when new job assigned: "New job! [Location] at [Time]"
5. Notification sound or browser alert (optional)
6. Clicking notification → opens job details modal (see Story 5.2)
7. Completed jobs archived (history tab available)
8. Mobile-responsive; can be used on phone (field contractors check this frequently)

### Story 5.2: Job Details Modal & Accept/Decline Workflow

**As a** contractor,  
**I want** to see full job details before accepting,  
**so that** I can make an informed decision about whether it fits my schedule/expertise.

**Acceptance Criteria:**

1. New job assigned → Modal/drawer opens automatically or contractor clicks to expand
2. Job details shown: Job Type, Location, Scheduled Date/Time, Duration estimate, Customer name, Customer rating (so contractor can assess customer reliability)
3. Pay information: Estimated pay / rate (if available from dispatcher entry)
4. Customer profile: Name, rating, past jobs with this contractor (if any)
5. "Accept" and "Decline" buttons clearly visible
6. Contractor clicks "Accept" → status updates to "Accepted", contractors confirms
7. Contractor clicks "Decline" → dispatcher + customer notified; job reopens for other contractors
8. Accept/Decline completes instantly (<1 second)
9. Modal closes after decision; returns to job list
10. Acceptance visible to dispatcher + customer in real-time (SignalR)

### Story 5.3: Job Status Management (In-Progress & Completion)

**As a** contractor,  
**I want** to mark jobs as in-progress and completed,  
**so that** customers see real-time status and I can track completed work.

**Acceptance Criteria:**

1. Accepted job shows "Mark In Progress" button on job card
2. Click "Mark In Progress" → Status changes to "In Progress" (customer sees instantly via SignalR)
3. In-progress job shows "Mark Complete" button
4. Click "Mark Complete" → Status changes to "Completed", job moves to history
5. Completion triggers email to customer: "Job complete! Please rate [Contractor]"
6. Contractor sees job in history (completed jobs tab)
7. Contractor can view all jobs (current, past) in chronological order
8. Job history persistent (doesn't disappear after session)

### Story 5.4: Contractor Rating & Earnings History

**As a** contractor,  
**I want** to see my rating and earnings history,  
**so that** I can track my performance and see how I'm doing (for motivation).

**Acceptance Criteria:**

1. "Profile" tab in contractor dashboard shows: Name, Rating (e.g., 4.7/5 stars), Total jobs assigned, Completed jobs count
2. "Job History" shows all past jobs: Date, Location, Customer, Job Type, Customer Rating (if rated)
3. Stats panel: "Jobs Assigned: 42, Accepted: 38, Completed: 38, Acceptance Rate: 90%"
4. Earnings summary (optional for MVP): "Total Earnings: $3,400" (if payment tracking available)
5. Recent ratings from customers visible: "5 stars - Maria K: 'Very professional, finished early!'"
6. If rating drops (e.g., new low review), notification to contractor (not punitive tone; informational)
7. Contractor can view detailed job history (filter by date range if desired)

### Story 5.5: Real-Time Job Notifications (SignalR)

**As a** contractor,  
**I want** to receive real-time notifications of job assignments and changes,  
**so that** I don't miss opportunities and I'm always in sync.

**Acceptance Criteria:**

1. When dispatcher assigns job → Contractor receives real-time notification (<10 seconds) via SignalR
2. Notification shows: Job Type, Location, Scheduled Time (popup or badge)
3. Contractor can click notification → opens job details modal (see Story 5.2)
4. If job reassigned to different contractor, original contractor notified: "Your assignment has been reassigned"
5. If job cancelled, contractor notified: "Job cancelled: [Reason, if provided]"
6. If job schedule updated, contractor notified: "Your schedule updated: New time [Time]"
7. Notifications persist even if contractor leaves page (history in notification center)
8. Sound/browser alert optional (contractor can disable in settings)
9. Notification latency <10 seconds (real-time feel)

### Story 5.6: Contractor Email Notifications

**As a** contractor,  
**I want** to receive email notifications of important job events,  
**so that** I don't miss assignments even if app isn't open.

**Acceptance Criteria:**

1. Email sent when job assigned: "You have a new job assignment!" with job details, location, time, direct accept/decline link
2. Email sent when job cancelled: "[Job] has been cancelled"
3. Email sent when job schedule changes: "[Job] schedule updated to [New Time]"
4. Customer rating received email: "[Customer Name] rated you 5 stars"
5. Each email includes direct link to contractor portal (or direct action link)
6. Email template professional and mobile-friendly
7. Contractor can disable email notifications in settings (optional for MVP)

---

## Epic 6: Reviews, Email & System Coordination

**Expanded Goal:** Implement the customer rating system, email notification infrastructure, and event-driven coordination that enables all three portals to work in sync. This epic makes the system feel "alive" with real-time updates and keeps all parties informed.

### Story 6.1: Customer Rating & Review System

**As a** customer,  
**I want** to rate contractors and leave reviews after job completion,  
**so that** my feedback shapes contractor quality and helps future customers make decisions.

**Acceptance Criteria:**

1. After job completion, "Rate This Job" section appears on customer job tracking page
2. Rating selector: 1-5 star clickable interface (visual star icons)
3. Text review: Optional text area "Tell us about your experience"
4. Submit button: "Submit Rating"
5. Validation: Rating required; text optional
6. Success: "Thank you for your feedback!" + toast notification
7. Rating submitted → Review record created in database with: jobId, contractorId, rating, comment, timestamp
8. Customer can edit their own review (within 24 hours, optional)
9. Review visible to future customers on contractor profile
10. Duplicate submission prevented (one review per job, per customer)

### Story 6.2: Contractor Rating Aggregation & Display

**As a** the system,  
**I want** contractor average rating calculated accurately and updated in real-time,  
**so that** scoring algorithm and UI displays current contractor quality.

**Acceptance Criteria:**

1. When new review created, calculate new average: `avgRating = SUM(all ratings) / COUNT(all reviews)`
2. Store result on Contractor entity (`avgRating` field)
3. Rating updates visible immediately on contractor profile (next page load)
4. Contractor with no reviews: avgRating = null (handled in UI as "No ratings yet")
5. Example: 3 reviews (5, 4, 5) → avgRating = 4.67 (decimal precision)
6. Contractor profile shows: "[Rating] based on [N] reviews"
7. Rating used in scoring algorithm (weightage 30% of score)
8. Recommendation cache invalidated on new rating (next query includes updated rating)
9. Unit tests verify aggregation accuracy

### Story 6.3: Email Service Setup & Configuration

**As a** system,  
**I want** to send transactional emails via AWS SES,  
**so that** all users stay informed of important events even when app closed.

**Acceptance Criteria:**

1. AWS SES account configured and verified (domain/email addresses)
2. Email credentials stored in AWS Secrets Manager (never hardcoded)
3. Email service abstraction layer created (interface `IEmailService`)
4. `SendEmail(to, subject, template, data)` method sends email via SES
5. Email templates created for each event type:
   - JobAssigned (to contractor + customer)
   - ContractorDeclined (to dispatcher + customer)
   - JobCompleted (to customer)
   - RatingPosted (to contractor)
   - FeedbackReminder (to customer, 2 hours after job completion)
6. Templates use Handlebars syntax (or similar) for variable substitution: {{contractorName}}, {{jobLocation}}, {{rating}}
7. Email HTML formatting: Professional, branded, mobile-responsive
8. Fallback to plaintext if HTML fails
9. Retry logic: If send fails, retry 3 times with exponential backoff

### Story 6.4: Event Publishing & Domain Events

**As a** system,  
**I want** to publish domain events when important things happen,  
**so that** different parts of the system react to events without tight coupling.

**Acceptance Criteria:**

1. Domain events defined: `JobAssigned`, `JobCancelled`, `ContractorAccepted`, `ContractorDeclined`, `JobCompleted`, `RatingPosted`
2. When job assigned: `JobAssignedEvent` published with jobId, contractorId, customerId, assignmentTime
3. When contractor accepts: `ContractorAcceptedEvent` published with jobId, contractorId
4. When job completed: `JobCompletedEvent` published with jobId, contractorId, customerId
5. When rating posted: `RatingPostedEvent` published with jobId, contractorId, rating
6. Events stored in `Events` table for audit trail: eventId, eventType, aggregateId, timestamp, data (JSON)
7. Event handlers subscribe to events (email service listens for JobAssignedEvent, sends email)
8. Event processing guaranteed (no lost events)
9. Events published **after** domain operation succeeds (no orphaned events if transaction fails)

### Story 6.5: Email Event Handler & Notifications

**As a** system,  
**I want** to send emails automatically when important events occur,  
**so that** users are notified without manual intervention.

**Acceptance Criteria:**

1. Event handler: `JobAssignedEventHandler` listens for `JobAssignedEvent`
   - Sends email to contractor: "You've been assigned [Job]"
   - Sends email to customer: "[Contractor] assigned to your job"
2. Event handler: `ContractorDeclinedEventHandler` listens for `ContractorDeclinedEvent`
   - Sends email to dispatcher: "[Contractor] declined [Job]"
   - Sends email to customer: "Contractor declined; finding new match"
3. Event handler: `JobCompletedEventHandler` listens for `JobCompletedEvent`
   - Sends email to customer: "Job complete! Please rate [Contractor]"
4. Event handler: `RatingPostedEventHandler` listens for `RatingPostedEvent`
   - Sends email to contractor: "[Customer] rated you [Rating] stars"
5. Email includes relevant context: job details, contractor info, direct links to app
6. All emails sent within 2 seconds of event
7. Failed email sends logged but don't crash system (graceful failure)
8. Email audit log: timestamp, recipient, event, success/failure status

### Story 6.6: SignalR Real-Time Coordination

**As a** user,  
**I want** to see real-time updates across the system,  
**so that** I never see stale data and the system feels responsive.

**Acceptance Criteria:**

1. SignalR hub created: `NotificationHub`
2. User groups by role: `dispatcher-{dispatcherId}`, `customer-{customerId}`, `contractor-{contractorId}`
3. When job assigned:
   - `ContractorGroup` notified: "New job assigned: [Job]"
   - `CustomerGroup` notified: "Contractor assigned: [Name]"
   - `DispatcherGroup` notified: "Job status updated"
4. When contractor accepts:
   - `CustomerGroup` notified: "[Contractor] accepted your job"
   - `DispatcherGroup` notified: "[Contractor] accepted [Job]"
5. When job status changes (in-progress, completed):
   - `CustomerGroup` notified: "Status: [NewStatus]"
   - `DispatcherGroup` notified: "Job status: [NewStatus]"
6. Real-time latency <100ms (all connected clients see update within 100ms)
7. Connection monitoring: Detect disconnects, auto-reconnect within 30 seconds
8. Graceful fallback if connection drops (polling fallback, optional)
9. Unit tests verify SignalR message routing

---

## Epic 7: Testing, Performance & Production Deployment

**Expanded Goal:** Write comprehensive integration tests, optimize performance (caching, indexing), polish error handling, set up production deployment pipeline, and prepare documentation. This epic ensures the system is production-ready and portfolio-polished.

### Story 7.1: Integration Test Suite - Full Workflows

**As a** developer,  
**I want** comprehensive integration tests covering complete user workflows,  
**so that** I can deploy with confidence and showcase code quality.

**Acceptance Criteria:**

1. **Test Suite 1: Dispatcher Workflow**
   - Setup: Create contractor, customer, job
   - Action: Dispatcher requests recommendations, assigns job
   - Verify: Job status = "assigned", contractor notified, customer notified
   - Assertion: All three parties see updated state
2. **Test Suite 2: Contractor Workflow**
   - Setup: Job assigned to contractor
   - Action: Contractor accepts job, marks in-progress, marks complete
   - Verify: Job progresses through all states
   - Assertion: Customer sees real-time status updates
3. **Test Suite 3: Customer Rating Workflow**
   - Setup: Job completed
   - Action: Customer submits rating
   - Verify: Rating persisted, contractor avg rating updated
   - Assertion: Rating visible on contractor profile
4. **Test Suite 4: Contractor List Filtering**
   - Setup: Dispatcher adds contractors to list, filters recommendations
   - Action: Request recommendations with `contractor_list_only=true`
   - Verify: Only contractors in dispatcher's list returned
   - Assertion: Filtering works correctly
5. **Test Suite 5: Email Notifications**
   - Setup: Trigger job assignment event
   - Action: Email handler sends email
   - Verify: Email sent to correct recipient with correct content
   - Assertion: Email audit log records success
6. **Test Suite 6: Real-Time SignalR**
   - Setup: Two contractors connected to SignalR hub
   - Action: Publish job assignment event
   - Verify: Both receive notification within 100ms
   - Assertion: Real-time latency verified
7. Test database auto-setup before each test (migrations run)
8. Test data seeded before each test (contractors, customers, etc.)
9. All tests use xUnit + FluentAssertions
10. Test coverage: All critical workflows pass 100%

### Story 7.2: Performance Testing & Optimization

**As a** system,  
**I want** to verify performance targets are met,  
**so that** users experience fast, responsive interactions.

**Acceptance Criteria:**

1. **Performance Target Validation:**
   - Recommendation API <500ms with 10,000 contractors: ✅ Verified
   - Job assignment <2 seconds: ✅ Verified
   - SignalR latency <100ms: ✅ Verified
   - Page load <2 seconds: ✅ Verified
2. **Database Optimization:**
   - Indexes added: `Contractors.Location`, `Jobs.Status`, `Assignments.ContractorId`
   - LINQ queries optimized (avoid N+1 queries, use `.Include()` for related data)
   - Query execution plans reviewed
3. **Caching Strategy:**
   - Redis cache for contractor list (5-minute TTL, invalidate on add/remove)
   - Distance calculations cached (24-hour TTL)
   - Recommendation results cached (1-minute TTL, short-lived)
4. **Load Testing:**
   - 100 concurrent users querying recommendations: System sustains <500ms response time
   - 1000+ concurrent SignalR connections: Hub handles gracefully
   - 1000+ emails/hour: Email queue handles without backlog
5. **Frontend Performance:**
   - React components optimized (React.memo on expensive components)
   - Code splitting by route (lazy loading)
   - Bundle size <200KB (gzipped)
   - Lighthouse score >85 (performance metric)

### Story 7.3: Error Handling & User Feedback

**As a** user,  
**I want** clear, actionable error messages,  
**so that** I understand what went wrong and how to fix it.

**Acceptance Criteria:**

1. **Backend Error Responses:**
   - All errors return structured JSON: `{ error: { code, message, statusCode } }`
   - Example: `{ error: { code: "JOB_ALREADY_ASSIGNED", message: "This job is already assigned", statusCode: 409 } }`
   - HTTP status codes correct: 400 (bad request), 401 (unauthorized), 403 (forbidden), 404 (not found), 409 (conflict), 500 (server error)
2. **Frontend Error Handling:**
   - API errors caught and displayed as toast notifications
   - User-friendly messages: "Failed to assign job. Please try again." (not technical stack traces)
   - Retry logic: Important actions (assignment, payment) can be retried
   - Fallback UI: If API down, display "Service temporarily unavailable"
3. **Logging & Monitoring:**
   - All errors logged to CloudWatch with request ID
   - Stack traces logged (not exposed to user)
   - Error monitoring: Dashboard shows error rates, frequent errors
   - Alerts: Slack notification if error rate spikes
4. **Edge Cases Handled:**
   - Contractor no longer available when assignment clicked: "Contractor no longer available"
   - Duplicate submission (double-click): Prevented (idempotent operations)
   - Network timeout: Graceful retry or user prompt

### Story 7.4: Security Review & Hardening

**As a** system,  
**I want** to implement security best practices,  
**so that** user data is protected and system is hardened against common attacks.

**Acceptance Criteria:**

1. **Authentication & Authorization:**
   - JWT tokens include expiration; refresh tokens for long-lived sessions
   - Passwords hashed with BCrypt (not plaintext, not MD5)
   - Role-based access control enforced at API layer
   - Unauthenticated requests to protected endpoints return 401
   - Unauthorized requests (wrong role) return 403
2. **Data Protection:**
   - Sensitive data (passwords, email) never logged
   - API responses filtered by role (contractor can't see competitor details)
   - SQL injection prevented (parameterized queries via EF Core)
   - HTTPS enforced everywhere (no HTTP)
3. **Secrets Management:**
   - Database password, API keys stored in AWS Secrets Manager (not hardcoded)
   - Secrets rotated periodically (optional for MVP)
   - No secrets in Git repo (.gitignore configured)
4. **CORS & API Security:**
   - CORS configured for frontend domain only (not wildcard)
   - API rate limiting (optional, but good practice)
   - Input validation on all endpoints (no arbitrary SQL queries)
5. **Security Testing:**
   - OWASP Top 10 review (SQL injection, XSS, CSRF, etc.)
   - Penetration testing (optional, but appreciated for portfolio)

### Story 7.5: CI/CD Pipeline & Automated Deployment

**As a** developer,  
**I want** automated testing and deployment,  
**so that** I can ship changes confidently without manual steps.

**Acceptance Criteria:**

1. **GitHub Actions Workflow:**
   - On PR: Run tests, lint code, build Docker image (don't push)
   - On merge to main: Run tests, build Docker image, push to ECR, deploy to App Runner
   - On release tag: Deploy to production environment
2. **Backend Deployment:**
   - Docker image built (multi-stage, minimal size)
   - Image pushed to Amazon ECR
   - App Runner service updated with new image
   - Zero-downtime deployment (blue-green or rolling update)
3. **Frontend Deployment:**
   - React build: `npm run build`
   - Output uploaded to S3 bucket
   - CloudFront cache invalidated (users see new version immediately)
4. **Database Migrations:**
   - Migrations run automatically on App Runner startup
   - Rollback capability (previous migration available)
5. **Deployment Verification:**
   - Health check endpoint: `GET /health` returns 200 OK
   - Smoke tests run post-deployment (quick sanity checks)
   - Notifications: Slack message on success/failure
6. **Secrets & Configuration:**
   - Database credentials via AWS Secrets Manager
   - Environment variables set via App Runner configuration
   - Secrets never exposed in logs

### Story 7.6: Documentation & Portfolio Preparation

**As a** hiring manager,  
**I want** clear documentation and a professional portfolio,  
**so that** I can understand the system and assess technical capability.

**Acceptance Criteria:**

1. **Architecture Documentation:**
   - C4 Model diagrams (Context, Container, Component levels)
   - Domain model ER diagram (entities and relationships)
   - Data flow diagram (how data moves between portals)
   - API architecture overview
2. **API Documentation:**
   - Swagger/OpenAPI live documentation (auto-generated)
   - All endpoints documented: request/response examples, error codes
   - Authentication: JWT token example
   - Example curl commands for key endpoints
3. **README.md:**
   - Project overview (2-3 paragraphs)
   - Tech stack (link to Epics showing choices)
   - Setup instructions: "Clone → `dotnet restore` → `npm install` → `dotnet run`"
   - Deployment: "How to deploy to AWS"
   - Testing: "How to run tests"
   - Contact/LinkedIn for questions
4. **Code Quality:**
   - Clean code with meaningful variable/method names
   - Comments only where necessary (why, not what)
   - No debug logs or commented-out code
   - Consistent formatting (automatic via EditorConfig)
5. **Git History:**
   - Meaningful commit messages (not "fix bug" or "wip")
   - Example: "feat: implement contractor scoring algorithm" or "fix: prevent double-booking in availability engine"
   - Clean commit history (no "oops, forgot something" commits)
6. **Demo Video (Optional):**
   - 3-5 minute video showing complete end-to-end workflow
   - Script: "I'll show how a customer submits a job, dispatcher assigns it, and contractor completes it"
   - Shows real-time updates, all three portals in action
   - Demonstrates system sophistication

### Story 7.7: Polish & Edge Case Handling

**As a** user,  
**I want** the system to handle edge cases gracefully,  
**so that** the experience feels complete and professional.

**Acceptance Criteria:**

1. **Empty States:**
   - No jobs: "No jobs at this time. We'll notify you when one arrives."
   - No contractors: "No available contractors. Please try again later."
   - No recommendations: "No contractors available for this time slot."
2. **Loading States:**
   - API calls show spinner or skeleton loader (not blank page)
   - Progress indication for long operations (e.g., "Saving... 50%")
3. **Confirmation Dialogs:**
   - Destructive actions prompt user: "Are you sure? This cannot be undone."
   - Examples: Reassign job (old contractor loses assignment), remove contractor from list
4. **Form Validation:**
   - Real-time validation feedback (red border on invalid field)
   - Helpful error messages: "Email must be valid" (not "Invalid input")
   - Required fields marked clearly
5. **Responsive Design:**
   - All pages work on desktop (1920px), tablet (768px), mobile (375px)
   - No horizontal scrolling
   - Touch-friendly buttons on mobile (minimum 44px tap target)
6. **Accessibility (WCAG AA):**
   - Keyboard navigation supported (Tab, Enter, Esc)
   - Color contrast meets AA standards
   - Screen reader compatible (alt text, semantic HTML)
   - Focus indicators visible
7. **Performance Polish:**
   - No janky animations (60 FPS)
   - Transitions smooth (<300ms)
   - Lazy loading for images (if any)

---
