# Next Steps

## UX Expert Prompt (If Handing Off to UX Designer)

**To:** UX/Design Team  
**From:** Product  
**Subject:** Design SmartScheduler User Interfaces

**Context:** We've defined three distinct user portals (Dispatcher, Customer, Contractor) for an intelligent field service marketplace. See Section 3 (UI Design Goals) and Epics 3-5 (Portal Stories) for full requirements.

**Your Mission:** Design high-fidelity wireframes and component library for all three portals.

**Key Principles:**

- **Dispatcher Dashboard:** Speed + clarity. One-click workflows. Minimize clicks to assignment.
- **Customer Portal:** Transparency + trust. Job status visible. Contractor credibility prominent.
- **Contractor Portal:** Simplicity + mobile-first. Fast job details → accept/decline. Real-time notifications.

**Deliverables:**

1. Wireframes for 12 core screens (see Story list for screens)
2. Component library: buttons, forms, modals, cards, notifications (leverage shadcn/ui)
3. Responsive layouts: desktop (1920px), tablet (768px), mobile (375px)
4. Accessibility checklist: WCAG AA compliance verification
5. Design system: colors (indigo-600 primary, teal-500 secondary), typography, spacing grid
6. Prototype or Figma: Interactive prototype showing key workflows (recommend Figma)

**Success Criteria:**

- Dispatcher can conceptually assign a job in 3 clicks (wireframes show this clearly)
- Customer can see job status and contractor details intuitively
- Contractor can accept/decline job in <1 minute (flow optimized for field use)
- All screens follow design system consistently

**Timeline:** 1-2 weeks (iterate with dev team if needed)

---

## Architect Prompt (For Technical Implementation)

**To:** Technical Architect / Development Team  
**From:** Product  
**Subject:** Architecture & Implementation Plan for SmartScheduler MVP

**Context:** SmartScheduler is a three-role field service marketplace with intelligent contractor matching. This PRD defines the MVP scope, user workflows, and success criteria. Your job: translate this into technical architecture and implementation plan.

**Core System Characteristics:**

- **Multi-role system** with role-based access control (RBAC)
- **Real-time coordination** via SignalR (all three portals sync in <100ms)
- **Intelligent matching** powered by weighted scoring algorithm
- **Event-driven architecture** for loose coupling and testability
- **Production-ready** with CI/CD, monitoring, security hardening

**Your Deliverables (In Order):**

**Phase 1: Technical Design (Week 1-2)**

1. **Domain Model Diagram** (DDD): Entities (Contractor, Customer, Job, Assignment, Review, etc.), aggregates, relationships
2. **API Contract** (OpenAPI/Swagger): All endpoints (Dispatcher, Customer, Contractor APIs) with request/response schemas
3. **Data Schema** (ER Diagram): PostgreSQL tables, indexes, relationships
4. **Real-Time Architecture**: SignalR hub structure, group management, event broadcasting
5. **Email Service**: AWS SES integration, template system, retry logic
6. **Deployment Architecture**: AWS resources (App Runner, RDS, S3, CloudFront, ECR)

**Phase 2: Implementation (Weeks 3-7)**

1. Start with Epic 1 (Foundation): .NET project setup, JWT auth, CI/CD pipeline
2. Follow epic sequence (Epic 2 → Scoring Algorithm is critical early work)
3. Implement each story with acceptance criteria as test cases (TDD approach)
4. Code review within team; ensure clean architecture (no cross-layer dependencies)

**Phase 3: Testing & Deployment (Week 8)**

1. Integration test suite (Epic 7.1)
2. Performance testing & optimization (Epic 7.2)
3. Security review & hardening (Epic 7.4)
4. Documentation (Epic 7.6)
5. Deploy to production (Epic 7.5)

**Key Technical Constraints (Non-Negotiable):**

- ✅ Backend: C# with .NET 8
- ✅ Cloud: AWS (App Runner, RDS, S3, CloudFront, SES)
- ✅ Real-Time: SignalR
- ✅ Database: PostgreSQL
- ✅ Testing: xUnit + integration tests (no E2E unless bonus)

**Key Technical Choices (For Your Decision):**

- Frontend library: React 18+ with TypeScript (recommended)
- UI components: shadcn/ui or similar (recommended for speed)
- State management: React Context API + useReducer (sufficient; no Redux)
- Database ORM: Entity Framework Core (recommended for .NET ecosystem)
- Email: AWS SES (configured)

**Success Criteria for Architect:**

1. Domain model reflects business logic accurately (no anemic domain)
2. API contracts match requirements (all 51 requirements traceable to endpoints)
3. Real-time coordination works reliably (<100ms latency verified)
4. Scoring algorithm performs <500ms with 10,000+ contractors
5. Security review complete (OWASP Top 10 addressed)
6. CI/CD pipeline fully automated (no manual deployments)
7. Code quality: clean architecture, >80% test coverage on business logic

**Reference Documents:**

- Full PRD: See Sections 1-5 (Goals, Requirements, UI Design, Tech Assumptions, Epics)
- Epic Details: See Section 5 (43 stories with acceptance criteria)
- Requirements Traceability: All 51 requirements mapped to stories

**Timeline:** 4-8 weeks for MVP (Epic 1-7 complete)

**Communication Plan:**

- Weekly sync: Review completed stories, blockers, upcoming work
- PR reviews: Code review before merge to main
- Demo: Bi-weekly stakeholder demo (show working features)

---

**Document Version:** 1.0  
**Status:** Ready for Implementation  
**Last Updated:** November 2025
