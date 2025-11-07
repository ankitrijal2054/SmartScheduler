# 15. Security and Performance

## 15.1 Security Requirements

**Frontend Security:**

- CSP headers configured
- XSS prevention via React JSX auto-escaping
- JWT tokens in localStorage (acceptable for portfolio)
- Sensitive data never stored client-side

**Backend Security:**

- FluentValidation on all inputs
- Parameterized queries (SQL injection prevention)
- Rate limiting: 100 req/min unauthenticated, 500 req/min authenticated
- CORS configured for frontend domain only
- Password hashing: BCrypt (cost factor 12)
- JWT tokens: 1-hour expiry, refresh tokens 7-day expiry

## 15.2 Performance Optimization

**Frontend Performance:**

- Bundle size target: <200KB gzipped
- Code splitting by route
- Lazy loading: `React.lazy()` + `<Suspense>`
- Image lazy loading

**Backend Performance:**

- Response time target: <500ms P95 for recommendations
- Database indexes on all foreign keys
- Redis caching (distance: 24h, contractor list: 5min)
- Connection pooling: Min 10, Max 100

**Caching Strategy:**

- Distance calculations: 24-hour TTL
- Contractor list: 5-minute TTL, invalidated on update
- Recommendation results: 1-minute TTL

---
