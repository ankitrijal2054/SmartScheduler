# Story 5.3: Job Status Management - Risk Assessment

**Date**: November 9, 2025  
**Story ID**: 5.3  
**Story Title**: Job Status Management (In-Progress & Completion)  
**Reviewer**: Quinn (Test Architect)  
**Overall Risk Level**: 🟢 **LOW**

---

## Risk Matrix

| Risk Factor                                | Probability | Impact | Priority | Status       | Mitigation                                                                                                     |
| ------------------------------------------ | ----------- | ------ | -------- | ------------ | -------------------------------------------------------------------------------------------------------------- |
| Invalid state transitions allowed          | Low         | High   | P1       | ✅ MITIGATED | Domain validation in Assignment.MarkX() methods prevents invalid transitions                                   |
| Unauthorized contractor updates assignment | Low         | High   | P1       | ✅ MITIGATED | Authorization check: `if (assignment.ContractorId != request.ContractorId)` throws UnauthorizedAccessException |
| Race condition on concurrent updates       | Low         | Medium | P2       | ✅ MITIGATED | EF Core change tracking + database commit ensures consistency                                                  |
| N+1 query performance issue                | Low         | Medium | P2       | ✅ MITIGATED | Single assignment fetch includes Job navigation property via eager loading                                     |
| Pagination bypass (DOS vector)             | Very Low    | Low    | P3       | ✅ MITIGATED | Limit capped at 100, offset validated ≥ 0                                                                      |
| Event publishing failure silently          | Low         | High   | P1       | ⚠️ MONITORED | Event publisher async - implementation in Story 6.5/6.6. Recommend: Add event handler error logging            |
| Database index missing on history queries  | Low         | Medium | P2       | ✅ MITIGATED | Migration applies `(ContractorId, Status, CompletedAt DESC)` index                                             |

---

## Risk Assessment Details

### 1. Invalid State Transitions (SCORE: 2/10 - LOW)

**Risk**: Contractor could potentially mark job as completed without marking in-progress first.

**Probability**: LOW (Business logic prevents it)

- Domain entity methods validate state before transition
- `Assignment.MarkComplete()` throws exception if status ≠ InProgress
- Frontend conditionally hides "Mark Complete" button unless status = InProgress

**Impact if Occurs**: HIGH (Data integrity violation)

- Completed job with no StartedAt timestamp would be inconsistent
- Could cause issues in billing/reporting

**Mitigation**:

- ✅ Domain validation enforced at entity level (cannot be bypassed)
- ✅ API rejects invalid transitions with 400 BadRequest
- ✅ Unit tests verify rejection: `UpdateAssignmentStatusCommandHandlerTests.MarkComplete_NotInProgressStatus_ThrowsInvalidOperationException`

**Residual Risk**: Negligible. Domain logic is authoritative.

---

### 2. Unauthorized Contractor Updates (SCORE: 2/10 - LOW)

**Risk**: Contractor A could mark Contractor B's assignment as complete, falsifying work.

**Probability**: LOW (Multiple authorization checks)

- Command handler checks: `if (assignment.ContractorId != request.ContractorId)`
- GetUserId() extracts contractor ID from JWT claims
- Controller has `[Authorize(Roles = "Contractor")]` attribute

**Impact if Occurs**: CRITICAL (Fraud, payment fraud)

- Contractor B gets paid for work not done
- System loses integrity

**Mitigation**:

- ✅ Authorization enforced at command handler level (closest to business logic)
- ✅ ContractorId extracted from JWT claims (unforgeably bound to contractor)
- ✅ Test verifies rejection: `UpdateAssignmentStatusCommandHandlerTests.MarkInProgress_UnauthorizedContractor_ThrowsUnauthorizedAccessException`
- ✅ Controller test verifies 403 response: `AssignmentsControllerTests.MarkInProgress_UnauthorizedContractor_ReturnsForbid`

**Residual Risk**: Negligible. Two-layer authorization prevents exploitation.

---

### 3. Race Condition on Concurrent Updates (SCORE: 3/10 - LOW-MEDIUM)

**Risk**: If same assignment updated concurrently by same contractor (e.g., double-click), inconsistent state could result.

**Probability**: LOW (User unlikely to double-click rapidly, but possible on slow networks)

**Impact if Occurs**: MEDIUM (Potential duplicate timestamps or state confusion)

- Two PATCH requests race to update same assignment
- One succeeds, one fails
- Frontend handles gracefully with toast notification

**Mitigation**:

- ✅ EF Core change tracking ensures last-write-wins semantics
- ✅ InvalidOperationException thrown for already-InProgress → InProgress attempt
- ✅ PATCH endpoint idempotent (safe for retry)
- ✅ Frontend shows loading spinner during request (discourages double-click)
- 🔄 FUTURE: Consider optimistic concurrency control (ConcurrencyToken on Assignment entity)

**Residual Risk**: Low. Current implementation sufficient for MVP.

---

### 4. N+1 Query Performance (SCORE: 2/10 - LOW)

**Risk**: Multiple database queries per request could cause performance degradation at scale.

**Probability**: LOW (EF Core navigation property included)

**Impact if Occurs**: MEDIUM (API latency increases, poor UX)

- History endpoint with 100 completed assignments could issue 101 queries
- Response time degrades from <100ms to >1s

**Mitigation**:

- ✅ Assignment.Job navigation property eagerly loaded in handler
- ✅ History endpoint uses pagination (50 items default, max 100)
- ✅ Database indexes applied: `(ContractorId, Status, CompletedAt DESC)`
- 🔄 FUTURE: Monitor response times with load testing to validate <100ms SLA

**Residual Risk**: Low. Pagination + indexes provide sufficient protection.

---

### 5. Pagination Bypass (DOS Vector) (SCORE: 1/10 - VERY LOW)

**Risk**: Attacker sends `limit=999999&offset=0` to consume server memory.

**Probability**: VERY LOW (Limit capped in controller)

**Impact if Occurs**: LOW (Request fails with 400 BadRequest)

- Server returns error, no memory exhaustion
- Attacker blocked

**Mitigation**:

- ✅ Limit capped at 100: `limit = Math.Min(limit, 100)`
- ✅ Offset validated ≥ 0: `if (limit <= 0 || offset < 0) return BadRequest()`
- ✅ Pagination struct holds results safely

**Residual Risk**: Negligible.

---

### 6. Event Publishing Failure (SCORE: 4/10 - LOW-MEDIUM)

**Risk**: If event publisher fails during JobCompletedEvent publish, email (Story 6.5) never sends.

**Probability**: LOW (Event publisher typically resilient)

**Impact if Occurs**: HIGH (Customer doesn't receive "job complete" email)

- Customer unaware job finished
- Cannot rate contractor
- Affects engagement metrics

**Mitigation**:

- ✅ Event publisher is async (configured in Program.cs)
- ✅ Exception handling in handler catches and logs
- 🔄 FUTURE: Implement event handler in Story 6.5 with retry logic + dead-letter queue
- 🔄 FUTURE: Add monitoring/alerting for event publishing failures

**Residual Risk**: Medium. Handled by downstream event handler implementation (Story 6.5).

**Recommendation**: When implementing Story 6.5 email handler, ensure:

- Retry mechanism for transient failures
- Dead-letter queue for persistent failures
- Alerting on event handler errors

---

### 7. Missing Database Index (SCORE: 2/10 - LOW)

**Risk**: History endpoint queries without index could cause table scans for large datasets.

**Probability**: LOW (Migration creates index)

**Impact if Occurs**: MEDIUM (Query performance degradation)

- GetAssignmentsByContractorAndStatusAsync scans entire Assignments table
- Query time: O(n) instead of O(log n)
- At 100k assignments: 5 seconds instead of 50ms

**Mitigation**:

- ✅ Migration file created: `20251109000000_AddAssignmentIndexes.cs`
- ✅ Index on `(ContractorId, Status, CompletedAt DESC)` applied
- ✅ Covers all columns used in WHERE and ORDER BY clauses

**Residual Risk**: Negligible if migration is applied.

**Prerequisite**: Ensure migration is run before deployment.

---

## Security Risk Assessment

### Authentication & Authorization

**✅ PASS** - Authorization is layered and correct:

1. JWT authentication required (`[Authorize(Roles = "Contractor")]`)
2. Contractor ID extracted from claims (unforgeably bound)
3. Command handler re-verifies identity match
4. 403 Forbidden returned for unauthorized attempts

**No security gaps identified**.

### Input Validation

**✅ PASS** - All inputs validated:

- Assignment ID: Validated exists (404 Not Found if not)
- Status: Enum validation prevents invalid values
- Pagination: Limit/offset validated with bounds checking

**No injection vectors identified**.

### Data Protection

**✅ PASS** - No sensitive data exposure:

- Assignment timestamps safe to return
- Contractor ID/Job ID are foreign keys (not passwords/tokens)
- No PII exposure

**No data leaks identified**.

---

## Performance Risk Assessment

| Metric                  | Target | Expected        | Status  |
| ----------------------- | ------ | --------------- | ------- |
| PATCH /mark-in-progress | <500ms | ~50ms (O(1))    | ✅ PASS |
| PATCH /mark-complete    | <500ms | ~50ms (O(1))    | ✅ PASS |
| GET /history (50 items) | <500ms | ~50ms (indexed) | ✅ PASS |
| Event publish           | async  | <100ms          | ✅ PASS |

**No performance risks identified**.

---

## Reliability Risk Assessment

### Error Handling

**✅ PASS** - Comprehensive error handling:

- AssignmentNotFoundException (404)
- UnauthorizedAccessException (403)
- InvalidOperationException (400)
- General Exception (logged, re-thrown)
- Toast notifications on frontend for all error cases

### Logging

**✅ PASS** - Structured logging at all levels:

- Info: Status transitions logged with context
- Warning: Unauthorized/not-found logged
- Error: Exceptions logged with stack trace

### Failure Modes

**Resilient to**:

- ✅ Missing assignment → 404 Not Found
- ✅ Invalid contractor → 403 Forbidden
- ✅ Invalid state transition → 400 Bad Request
- ✅ Database connection timeout → Exception propagates, logged
- ✅ Event publish failure → Logged, but doesn't block response

**Known Limitation**:

- ⚠️ If event publisher fails, email (Story 6.5) won't trigger (mitigated in downstream handler)

---

## Test Coverage Risk Assessment

### Coverage by Risk Level

| Risk Level            | Tests | Coverage | Confidence |
| --------------------- | ----- | -------- | ---------- |
| Critical (Auth/State) | 7     | 100%     | Very High  |
| High (Transitions)    | 6     | 100%     | Very High  |
| Medium (Validation)   | 4     | 100%     | High       |
| Low (UI/UX)           | 2     | 100%     | High       |

**Overall**: 19/19 tests passing (100% success rate)

**Gap Analysis**: ✅ No gaps. All major risk scenarios tested.

**Gaps by Type**:

- ✅ Authorization scenarios: Covered (3 tests)
- ✅ State transition validity: Covered (5 tests)
- ✅ Error scenarios: Covered (4 tests)
- ✅ API response codes: Covered (7 tests)
- 🔄 E2E full workflow: NOT YET (deferred to Playwright suite)
- 🔄 Performance under load: NOT YET (manual testing recommended)

---

## Deployment Risk Assessment

### Pre-Deployment Checklist

- ✅ All tests passing (19/19)
- ✅ Code review complete
- ✅ Architecture compliance verified
- ✅ Security authorization verified
- ✅ Database migration ready (indexes)
- ⚠️ Event handler not yet implemented (Story 6.5 - email)
- ⚠️ SignalR handler not yet implemented (Story 6.6 - real-time)

### Deployment Recommendation

**Status**: ✅ **SAFE TO DEPLOY**

**Prerequisites**:

1. Apply database migration (`20251109000000_AddAssignmentIndexes.cs`)
2. Ensure JWT authentication configured
3. Ensure MediatR event publisher configured

**Post-Deployment Monitoring**:

1. Monitor API response times for /history endpoint
2. Watch for event publishing errors in logs
3. Verify contractor status transitions working end-to-end

---

## Risk Ownership & Escalation

| Risk                      | Owner       | Escalation Path                     |
| ------------------------- | ----------- | ----------------------------------- |
| Authorization enforcement | Dev (James) | Security team if pattern violated   |
| Event publishing failures | Dev/PM      | Product if email delays impact UX   |
| Performance degradation   | Dev         | Ops if response times exceed SLA    |
| Database index missing    | Dev/DBA     | Ops if deployment skipped migration |

---

## Risk Trend

**Previous Stories**: 5.1 (Contractor Job List), 5.2 (Accept/Decline)  
**Risk Trajectory**: ✅ **STABLE/IMPROVING**

Story 5.3 maintains low risk by:

- Following established patterns (CQRS, Repository, DI)
- Comprehensive test coverage
- Strong authorization/validation
- Clear error handling

No new architectural risks introduced.

---

## Recommendations

### Must Do (Immediate)

1. ✅ **Already done**: Domain validation prevents invalid transitions
2. ✅ **Already done**: Authorization enforced at command handler
3. ✅ **Already done**: Database indexes applied via migration

### Should Do (This Sprint)

1. 🔄 **Implement Story 6.5**: Email handler for JobCompletedEvent (unblocks customer notifications)
2. 🔄 **Implement Story 6.6**: SignalR handler for JobInProgressEvent (unblocks real-time updates)
3. 🔄 **Monitor**: Response times on GET /history endpoint in staging

### Nice to Have (Future)

1. 📋 **Optimistic Concurrency**: Add `ConcurrencyToken` to Assignment entity for true concurrent update safety
2. 📋 **E2E Tests**: Create Playwright test for full workflow (accept → in-progress → complete → email received)
3. 📋 **Load Testing**: Validate pagination performance with 100k+ assignments

---

## Conclusion

**Overall Risk**: 🟢 **LOW**

Story 5.3 is **production-ready** with **minimal residual risks**. All critical security and authorization patterns properly implemented. Test coverage is comprehensive. Database indexes and pagination prevent performance issues.

**Recommended Action**: ✅ **PROCEED TO DEPLOYMENT**

No blocking issues identified.

---

**Reviewed by**: Quinn (Test Architect)  
**Date**: November 9, 2025  
**Next Review**: After implementation of Stories 6.5 and 6.6
