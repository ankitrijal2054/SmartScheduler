# 8. Core Workflows

## 8.1 Job Assignment Workflow

```mermaid
sequenceDiagram
    actor Dispatcher
    participant DispatcherUI as Dispatcher UI
    participant API as Backend API
    participant ScoringEngine as Scoring Engine
    participant DB as PostgreSQL
    participant SignalRHub as SignalR Hub
    participant EmailService as AWS SES
    participant ContractorUI as Contractor UI
    participant CustomerUI as Customer UI

    Dispatcher->>DispatcherUI: Click "Get Recommendations"
    DispatcherUI->>API: POST /api/v1/dispatcher/recommendations
    API->>ScoringEngine: ScoreContractors(jobDetails)
    ScoringEngine->>DB: GetActiveContractors(filter by list if needed)
    DB-->>ScoringEngine: List<Contractor>
    ScoringEngine->>ScoringEngine: Calculate weighted scores
    ScoringEngine-->>API: Top 5 ranked contractors
    API-->>DispatcherUI: ContractorRecommendation[]

    Dispatcher->>DispatcherUI: Click "Assign" on Contractor #1
    DispatcherUI->>API: POST /api/v1/dispatcher/jobs/{jobId}/assign
    API->>DB: Create Assignment(jobId, contractorId)
    API->>SignalRHub: NotifyContractor(contractorId)
    API->>SignalRHub: NotifyCustomer(customerId)
    API->>EmailService: SendJobAssignedEmail(contractor)
    API->>EmailService: SendJobAssignedEmail(customer)

    SignalRHub-->>ContractorUI: Real-time notification
    SignalRHub-->>CustomerUI: Real-time notification
```

## 8.2 Contractor Accept/Decline Workflow

_See Section 8 in full document for complete sequence diagrams._

## 8.3 Job Completion & Review Workflow

_See Section 8 in full document for complete sequence diagrams._

---
