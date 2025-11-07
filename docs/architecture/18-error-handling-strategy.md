# 18. Error Handling Strategy

## 18.1 Standard Error Format

```json
{
  "error": {
    "code": "CONTRACTOR_UNAVAILABLE",
    "message": "Contractor is already assigned during this time slot",
    "timestamp": "2025-11-07T14:30:00Z",
    "requestId": "req_abc123xyz"
  }
}
```

## 18.2 Frontend Error Handling

- Axios response interceptor catches all API errors
- Display user-friendly toast notifications
- Component-level try/catch for specific error handling
- Graceful fallbacks for network errors

## 18.3 Backend Error Handling

- Global exception handler middleware
- Custom exception types: `ValidationException`, `BusinessRuleException`, `NotFoundException`
- Structured error responses
- Full error logging to CloudWatch

---
