# 16. Testing Strategy

## 16.1 Testing Pyramid

- Unit Tests (60%): Business logic, services, domain entities
- Integration Tests (30%): API endpoints, database operations
- E2E Tests (10%): Critical user flows

## 16.2 Backend Tests

- xUnit + FluentAssertions
- In-memory database for integration tests
- Moq for mocking dependencies

## 16.3 Frontend Tests

- Vitest + React Testing Library
- Component unit tests
- Hook tests
- Service tests

## 16.4 E2E Tests

- Playwright (cross-browser: Chrome, Firefox, WebKit)
- Complete workflow tests:
  - Dispatcher: Job assignment flow
  - Customer: Job submission → tracking → review
  - Contractor: Accept assignment → complete → view review

---
