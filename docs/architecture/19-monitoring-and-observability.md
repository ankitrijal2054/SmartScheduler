# 19. Monitoring and Observability

## 19.1 Monitoring Stack

- Frontend: Sentry (error tracking), Web Vitals API
- Backend: AWS CloudWatch (logs, metrics), Serilog (structured logging)
- Error Tracking: CloudWatch Logs with structured context

## 19.2 Key Metrics

**Frontend:**

- Core Web Vitals (LCP <2.5s, FID <100ms, CLS <0.1)
- JavaScript error rate (<0.1%)
- API response times (P95 <500ms)

**Backend:**

- Request rate (monitored per endpoint)
- Error rate (4xx <5%, 5xx <0.5%)
- Recommendation API P95 <500ms
- SignalR concurrent connections

## 19.3 CloudWatch Alarms

- High error rate: 5xx >1% for 5 minutes → SNS alert
- Slow API: Recommendation P95 >500ms → Alert
- SignalR connection drop >50% → Alert
- Database connection pool >90% → Alert

---
