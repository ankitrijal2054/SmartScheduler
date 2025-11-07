# 7. External APIs

## 7.1 Google Maps Distance Matrix API

**Purpose:** Calculate real-time travel distance and travel time between job site and contractor location for scoring algorithm.

**Documentation:** https://developers.google.com/maps/documentation/distance-matrix/overview

**Base URL:** `https://maps.googleapis.com/maps/api/distancematrix/json`

**Authentication:** API key (query parameter: `key={API_KEY}`)

**Rate Limits:**

- Standard: 100 requests per second
- Daily limit: $200 free credit/month (~40,000 requests)
- Cost after free tier: $5 per 1000 requests

**Caching Strategy:** Distance between two locations cached for 24 hours in Redis (cache key: `distance:{originLat},{originLng}:{destLat},{destLng}`). Reduces API costs by ~90%.

**Fallback Logic:** If API returns error, use Haversine formula to calculate "as-the-crow-flies" distance, multiply by 1.3x to approximate road distance.

---

## 7.2 AWS SES (Simple Email Service)

**Purpose:** Send transactional emails to contractors, customers, and dispatchers for critical events.

**Documentation:** https://docs.aws.amazon.com/ses/

**Authentication:** AWS IAM credentials or IAM role (when running on App Runner)

**Rate Limits:**

- Production: 50,000 emails/day (free tier), then $0.10 per 1000 emails
- Sending rate: 14 emails/second (default)

**Email Templates:**

- JobAssignedToContractor - Notifies contractor of new assignment
- JobAssignedToCustomer - Notifies customer of contractor assignment
- ContractorDeclined - Notifies dispatcher that contractor declined
- JobCompleted - Notifies customer, prompts for feedback
- RatingReminder - Sent 2 hours after completion if no review
- RatingReceivedContractor - Notifies contractor of new review

**Integration Notes:**

- Emails sent via AWS SDK for .NET
- Retry logic: 3 attempts with exponential backoff
- Email audit log stored in database

---
