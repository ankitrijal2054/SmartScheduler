# 4. Data Models

Based on PRD requirements, the following core business entities represent the domain concepts shared between frontend and backend.

## 4.1 User

**Purpose:** Central authentication entity supporting three distinct roles. All Dispatchers, Customers, and Contractors authenticate through this model.

**Key Attributes:**

- `id`: string (UUID) - Unique identifier
- `email`: string - Authentication credential (unique)
- `passwordHash`: string - BCrypt hashed password (never exposed to frontend)
- `role`: enum ('Dispatcher' | 'Customer' | 'Contractor') - Determines portal access and permissions
- `isActive`: boolean - Soft delete flag
- `createdAt`: DateTime - Account creation timestamp
- `lastLoginAt`: DateTime? - Track engagement

**TypeScript Interface:**

```typescript
export enum UserRole {
  Dispatcher = "Dispatcher",
  Customer = "Customer",
  Contractor = "Contractor",
}

export interface User {
  id: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string; // ISO 8601 format
  lastLoginAt: string | null;
}

// Auth response DTO (frontend receives this after login)
export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresIn: number; // seconds until token expiry
}
```

**Relationships:**

- One User → One Contractor (1:1, optional - only if role = Contractor)
- One User → One Customer (1:1, optional - only if role = Customer)
- One User → Many DispatcherContractorLists (1:N - if role = Dispatcher)

---

## 4.2 Contractor

**Purpose:** Represents service professionals available for job assignment. Central to the scoring algorithm.

**Key Attributes:**

- `id`: string (UUID) - Primary key
- `userId`: string - Links to User entity
- `name`: string - Full name
- `phoneNumber`: string - Contact number
- `location`: string - Address (human-readable)
- `latitude`: decimal - Geocoded coordinate
- `longitude`: decimal - Geocoded coordinate
- `tradeType`: enum - Specialization (Flooring, HVAC, Plumbing, Electrical, Other)
- `workingHoursStart`: TimeSpan - Daily availability start (e.g., 09:00)
- `workingHoursEnd`: TimeSpan - Daily availability end (e.g., 17:00)
- `averageRating`: decimal? - Computed from all reviews (null if no reviews)
- `reviewCount`: int - Total reviews received
- `totalJobsCompleted`: int - Performance metric
- `isActive`: boolean - Availability flag
- `createdAt`: DateTime

**TypeScript Interface:**

```typescript
export enum TradeType {
  Flooring = "Flooring",
  HVAC = "HVAC",
  Plumbing = "Plumbing",
  Electrical = "Electrical",
  Other = "Other",
}

export interface Contractor {
  id: string;
  userId: string;
  name: string;
  phoneNumber: string;
  location: string;
  latitude: number;
  longitude: number;
  tradeType: TradeType;
  workingHoursStart: string; // HH:mm format (e.g., "09:00")
  workingHoursEnd: string; // HH:mm format (e.g., "17:00")
  averageRating: number | null;
  reviewCount: number;
  totalJobsCompleted: number;
  isActive: boolean;
  createdAt: string;
}

// Recommendation response (includes scoring metadata)
export interface ContractorRecommendation {
  contractor: Contractor;
  score: number; // 0.0 - 1.0 (weighted score)
  availabilityScore: number; // 0.0 or 1.0
  ratingScore: number; // 0.0 - 1.0
  distanceScore: number; // 0.0 - 1.0
  distanceMiles: number;
  travelTimeMinutes: number;
  suggestedTimeSlot: string; // ISO 8601 datetime
}
```

**Relationships:**

- One Contractor → Many Jobs (1:N via Assignments)
- One Contractor → Many Reviews (1:N)
- One Contractor → Many DispatcherContractorLists (1:N - many dispatchers can add same contractor)

---

## 4.3 Customer

**Purpose:** Represents job requesters. Tracks customer information for job visibility and review attribution.

**Key Attributes:**

- `id`: string (UUID) - Primary key
- `userId`: string - Links to User entity
- `name`: string - Full name
- `phoneNumber`: string - Contact number
- `location`: string - Default address (optional, can override per job)
- `createdAt`: DateTime

**TypeScript Interface:**

```typescript
export interface Customer {
  id: string;
  userId: string;
  name: string;
  phoneNumber: string;
  location: string | null;
  createdAt: string;
}
```

**Relationships:**

- One Customer → Many Jobs (1:N)
- One Customer → Many Reviews (1:N - authored reviews)

---

## 4.4 Job

**Purpose:** Core workflow artifact. Represents work requests progressing through the system lifecycle.

**Key Attributes:**

- `id`: string (UUID) - Primary key
- `customerId`: string - Job requester
- `jobType`: enum - Work category (matches TradeType)
- `location`: string - Job site address
- `latitude`: decimal - Geocoded coordinate
- `longitude`: decimal - Geocoded coordinate
- `desiredDateTime`: DateTime - Preferred start time
- `estimatedDurationHours`: decimal - Expected job length (e.g., 2.5)
- `description`: string - Additional details
- `status`: enum - Workflow state (Pending, Assigned, InProgress, Completed, Cancelled)
- `assignedContractorId`: string? - Null until assigned
- `createdAt`: DateTime
- `updatedAt`: DateTime

**TypeScript Interface:**

```typescript
export enum JobStatus {
  Pending = "Pending",
  Assigned = "Assigned",
  InProgress = "InProgress",
  Completed = "Completed",
  Cancelled = "Cancelled",
}

export interface Job {
  id: string;
  customerId: string;
  jobType: TradeType;
  location: string;
  latitude: number;
  longitude: number;
  desiredDateTime: string; // ISO 8601
  estimatedDurationHours: number;
  description: string;
  status: JobStatus;
  assignedContractorId: string | null;
  createdAt: string;
  updatedAt: string;
}

// Enriched job with related entities (for UI display)
export interface JobDetail extends Job {
  customer: Customer;
  assignedContractor: Contractor | null;
  assignment: Assignment | null;
}
```

**Relationships:**

- One Job → One Customer (N:1)
- One Job → One Contractor (N:1 via Assignment, optional)
- One Job → One Assignment (1:1, optional - only when assigned)
- One Job → One Review (1:1, optional - only when completed and rated)

---

## 4.5 Assignment

**Purpose:** Links jobs to contractors with assignment metadata. Tracks acceptance status and completion timing.

**Key Attributes:**

- `id`: string (UUID) - Primary key
- `jobId`: string - Job being assigned
- `contractorId`: string - Assigned contractor
- `assignedAt`: DateTime - When dispatcher confirmed assignment
- `acceptedAt`: DateTime? - When contractor accepted (null if declined or pending)
- `declinedAt`: DateTime? - When contractor declined
- `startedAt`: DateTime? - When marked in-progress
- `completedAt`: DateTime? - When marked completed
- `status`: enum - Assignment state (Pending, Accepted, Declined, InProgress, Completed)

**TypeScript Interface:**

```typescript
export enum AssignmentStatus {
  Pending = "Pending", // Assigned but contractor hasn't responded
  Accepted = "Accepted", // Contractor confirmed
  Declined = "Declined", // Contractor rejected
  InProgress = "InProgress", // Contractor marked as started
  Completed = "Completed", // Contractor marked as done
}

export interface Assignment {
  id: string;
  jobId: string;
  contractorId: string;
  assignedAt: string;
  acceptedAt: string | null;
  declinedAt: string | null;
  startedAt: string | null;
  completedAt: string | null;
  status: AssignmentStatus;
}
```

**Relationships:**

- One Assignment → One Job (N:1)
- One Assignment → One Contractor (N:1)

---

## 4.6 Review

**Purpose:** Customer feedback on completed jobs. Drives contractor reputation and scoring algorithm.

**Key Attributes:**

- `id`: string (UUID) - Primary key
- `jobId`: string - Reviewed job
- `contractorId`: string - Contractor being rated
- `customerId`: string - Review author
- `rating`: int - Star rating (1-5)
- `comment`: string? - Optional text review
- `createdAt`: DateTime - Review submission timestamp

**TypeScript Interface:**

```typescript
export interface Review {
  id: string;
  jobId: string;
  contractorId: string;
  customerId: string;
  rating: number; // 1-5
  comment: string | null;
  createdAt: string;
}

// Enriched review with customer info (for contractor profile display)
export interface ReviewWithCustomer extends Review {
  customerName: string;
}
```

**Relationships:**

- One Review → One Job (N:1)
- One Review → One Contractor (N:1)
- One Review → One Customer (N:1 - author)

**Constraints:**

- One review per job (unique constraint on `jobId`)
- Review can only be created after job status = Completed
- Rating must be 1-5 (validation enforced)

---

## 4.7 DispatcherContractorList

**Purpose:** Tracks dispatcher's curated contractor preferences. Enables filtered recommendations ("only show my trusted contractors").

**Key Attributes:**

- `id`: string (UUID) - Primary key
- `dispatcherId`: string - Dispatcher who added contractor
- `contractorId`: string - Contractor in list
- `addedAt`: DateTime - When added to list

**TypeScript Interface:**

```typescript
export interface DispatcherContractorList {
  id: string;
  dispatcherId: string;
  contractorId: string;
  addedAt: string;
}
```

**Relationships:**

- One DispatcherContractorList → One User (N:1, where role = Dispatcher)
- One DispatcherContractorList → One Contractor (N:1)

---

## 4.8 Entity Relationship Diagram

```mermaid
erDiagram
    User ||--o| Contractor : "has (if role=Contractor)"
    User ||--o| Customer : "has (if role=Customer)"
    User ||--o{ DispatcherContractorList : "creates (if role=Dispatcher)"

    Customer ||--o{ Job : "creates"
    Job ||--o| Assignment : "has"
    Job ||--o| Review : "receives"

    Contractor ||--o{ Assignment : "receives"
    Contractor ||--o{ Review : "rated by"
    Contractor ||--o{ DispatcherContractorList : "appears in"

    Assignment }o--|| Job : "for"
    Assignment }o--|| Contractor : "assigned to"

    Review }o--|| Job : "about"
    Review }o--|| Contractor : "rates"
    Review }o--|| Customer : "written by"

    DispatcherContractorList }o--|| User : "owned by"
    DispatcherContractorList }o--|| Contractor : "includes"

    User {
        string id PK
        string email UK
        string passwordHash
        enum role
        boolean isActive
        datetime createdAt
    }

    Contractor {
        string id PK
        string userId FK
        string name
        string location
        decimal latitude
        decimal longitude
        enum tradeType
        time workingHoursStart
        time workingHoursEnd
        decimal averageRating
        int reviewCount
        boolean isActive
    }

    Customer {
        string id PK
        string userId FK
        string name
        string phoneNumber
    }

    Job {
        string id PK
        string customerId FK
        enum jobType
        string location
        decimal latitude
        decimal longitude
        datetime desiredDateTime
        enum status
        string assignedContractorId FK
    }

    Assignment {
        string id PK
        string jobId FK
        string contractorId FK
        datetime assignedAt
        datetime acceptedAt
        enum status
    }

    Review {
        string id PK
        string jobId FK UK
        string contractorId FK
        string customerId FK
        int rating
        string comment
    }

    DispatcherContractorList {
        string id PK
        string dispatcherId FK
        string contractorId FK
        datetime addedAt
    }
```

---
