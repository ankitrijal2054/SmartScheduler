# 14. Deployment Architecture

## 14.1 Deployment Strategy

**Frontend Deployment:**

- Platform: AWS S3 + CloudFront
- Build: `npm run build` → `frontend/dist`
- CDN: CloudFront (1 hour cache for assets, 5 minutes for index.html)

**Backend Deployment:**

- Platform: AWS App Runner
- Build: Docker container → AWS ECR
- Deployment: Auto-deploy on ECR image push

## 14.2 CI/CD Pipeline (GitHub Actions)

**On Pull Request:**

- Run backend tests (xUnit)
- Run frontend tests (Vitest)
- Lint code

**On Merge to Main:**

- Build and push Docker image to ECR
- Deploy backend to App Runner
- Build frontend and upload to S3
- Invalidate CloudFront cache

## 14.3 Environments

| Environment | Frontend URL                       | Backend URL                            | Purpose                |
| ----------- | ---------------------------------- | -------------------------------------- | ---------------------- |
| Development | http://localhost:5173              | http://localhost:5000                  | Local development      |
| Staging     | https://staging.smartscheduler.com | https://api-staging.smartscheduler.com | Pre-production testing |
| Production  | https://app.smartscheduler.com     | https://api.smartscheduler.com         | Live environment       |

---
