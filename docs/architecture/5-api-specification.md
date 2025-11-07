# 5. API Specification

SmartScheduler uses a **REST API** with role-based endpoint organization. All endpoints return JSON and require JWT authentication (except auth endpoints).

**Base URL:** `https://api.smartscheduler.com` (production) | `http://localhost:5000` (development)

**API Version:** `v1` (path-prefixed: `/api/v1/*`)

**Authentication:** Bearer token in `Authorization` header: `Authorization: Bearer {jwt_token}`

**Content Type:** `application/json` (all requests/responses)

**Error Format:** Standard error response structure:

```json
{
  "error": {
    "code": "JOB_ALREADY_ASSIGNED",
    "message": "This job is already assigned to a contractor",
    "statusCode": 409,
    "timestamp": "2025-11-07T10:30:00Z",
    "requestId": "req_abc123xyz"
  }
}
```

## 5.1 Authentication Endpoints

**POST /api/v1/auth/register**

Create new user account with role assignment.

```yaml
Request:
  body:
    email: string (required, valid email)
    password: string (required, min 8 chars, must include uppercase/lowercase/number)
    role: enum ('Dispatcher' | 'Customer' | 'Contractor')
    profile: object (role-specific fields)

Response: 201 Created
  body:
    user: User
    accessToken: string (JWT, expires in 1 hour)
    refreshToken: string (expires in 7 days)
    expiresIn: number (3600 seconds)
```

**POST /api/v1/auth/login**

Authenticate existing user.

**POST /api/v1/auth/refresh**

Exchange refresh token for new access token.

**POST /api/v1/auth/logout**

Invalidate refresh token.

## 5.2 Dispatcher Endpoints

- `GET /api/v1/dispatcher/jobs` - List all jobs
- `POST /api/v1/dispatcher/recommendations` - Get ranked contractor recommendations
- `POST /api/v1/dispatcher/jobs/{jobId}/assign` - Assign job to contractor
- `PUT /api/v1/dispatcher/jobs/{jobId}/reassign` - Reassign job to different contractor
- `GET /api/v1/dispatcher/contractors` - List all contractors
- `GET /api/v1/dispatcher/contractors/{contractorId}/history` - View contractor history
- `POST /api/v1/dispatcher/contractor-list/{contractorId}` - Add contractor to personal list
- `DELETE /api/v1/dispatcher/contractor-list/{contractorId}` - Remove contractor from personal list
- `GET /api/v1/dispatcher/contractor-list` - Get personal contractor list

## 5.3 Customer Endpoints

- `POST /api/v1/customer/jobs` - Submit new job request
- `GET /api/v1/customer/jobs` - List customer's own jobs
- `GET /api/v1/customer/jobs/{jobId}` - Get single job detail
- `GET /api/v1/customer/contractors/{contractorId}/profile` - View contractor profile
- `POST /api/v1/customer/jobs/{jobId}/review` - Submit rating and review

## 5.4 Contractor Endpoints

- `GET /api/v1/contractor/assignments` - List contractor's assigned jobs
- `GET /api/v1/contractor/assignments/{assignmentId}` - Get detailed assignment information
- `PUT /api/v1/contractor/assignments/{assignmentId}/accept` - Accept job assignment
- `PUT /api/v1/contractor/assignments/{assignmentId}/decline` - Decline job assignment
- `PUT /api/v1/contractor/assignments/{assignmentId}/start` - Mark job as in-progress
- `PUT /api/v1/contractor/assignments/{assignmentId}/complete` - Mark job as completed
- `GET /api/v1/contractor/profile` - Get contractor's own profile and statistics
- `GET /api/v1/contractor/reviews` - List all reviews received

## 5.5 Common Endpoints

- `GET /api/v1/health` - Health check (no auth required)
- `GET /api/v1/users/me` - Get current user profile
- `PUT /api/v1/users/me` - Update current user profile

_Full endpoint specifications with request/response schemas available in API documentation at `/swagger`._

---
