# User Interface Design Goals

## Overall UX Vision

Three distinct, purpose-built interfaces optimized for three different user workflows and mental models:

- **Dispatcher Dashboard**: Fast, task-focused, minimal friction. Goal: Get from "I have a job" to "job assigned" in <2 minutes. Emphasis on clarity, ranked recommendations, one-click actions.
- **Customer Portal**: Transparent, reassuring, trust-building. Goal: Customer sees job status in real-time and knows exactly who is assigned and when they'll arrive. Emphasis on status clarity, contractor credibility (ratings/reviews), communication.
- **Contractor Portal**: Simple, notification-forward, mobile-first feel (even on desktop). Goal: Contractor sees jobs, understands details, accepts/declines quickly. Emphasis on push notifications, clear job info, quick actions (accept/decline/complete).

**Unifying principle**: Each interface is optimized for _how that user thinks about their job_, not forcing all three into a single design.

## Key Interaction Paradigms

**Dispatcher Dashboard:**

- **One-click workflows**: Job list → Request recommendations → See ranked contractors → Confirm assignment → Done. No modal dialogs or multi-step forms.
- **Inline editing**: Contractor list (add/remove) happens inline with immediate visual feedback, not in separate pages.
- **Real-time status**: Job board auto-updates when contractor accepts/declines or job status changes (users see it happen live).
- **Progressive disclosure**: Show top 5 contractor recommendations immediately; expand details on hover/click (avoid cognitive overload).

**Customer Portal:**

- **Status-centric**: Entire page revolves around job status progression (visual timeline or large status badge).
- **Contractor as credential**: Assigned contractor card is prominent with rating, reviews, photo, ETA. Customer doesn't need to dig to find this info.
- **Email-link driven**: Customer can rate job directly from email link (not requiring app login).

**Contractor Portal:**

- **Push notification flow**: Contractor gets alert → Clicks through to job details → Accepts/declines in modal → Returns to job list. Minimal friction.
- **Job details modal**: Before contractor accepts, they see full job info (location, customer, time, pay). No surprises.
- **Job history**: Simple chronological list of past jobs with completion status and customer rating received.

## Core Screens and Views

| Screen                               | Role(s)    | Purpose                                                                                                                                           |
| ------------------------------------ | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Authentication/Login**             | All        | Email/password or SSO (Google). Role-based redirect after login.                                                                                  |
| **Dispatcher Dashboard**             | Dispatcher | Main workspace: Open jobs, request recommendations, assign jobs, manage contractor list.                                                          |
| **Contractor Recommendations Modal** | Dispatcher | Ranked list of top 5 contractors with scores, availability, travel time, ratings. One-click confirm.                                              |
| **Contractor List Management**       | Dispatcher | View all available contractors; add/remove from personal "Contractor List"; filter recommendations by list membership.                            |
| **Job History**                      | Dispatcher | View past jobs, assignments, completion status, customer ratings.                                                                                 |
| **Customer Job Submission**          | Customer   | Form to create new job (type, location, date/time, description). Submit button, confirmation message.                                             |
| **Customer Job Tracking**            | Customer   | Real-time view of job status (submitted → assigned → in-progress → completed). Shows assigned contractor info.                                    |
| **Contractor Profile Card**          | Customer   | Contractor name, rating, reviews, photo, ETA. Accessible from job tracking view.                                                                  |
| **Customer Rating Form**             | Customer   | 1-5 star rating + optional text review. Post-job (triggered by email or in-app reminder).                                                         |
| **Contractor Job List**              | Contractor | All assigned jobs (current, upcoming, completed). Shows job summary (location, customer, time, pay).                                              |
| **Job Details Modal**                | Contractor | Full job info before accept/decline decision (customer profile, location, time window, estimated pay, customer rating).                           |
| **Contractor Job History**           | Contractor | Past jobs with completion status, customer ratings received, earnings summary.                                                                    |
| **Settings/Profile**                 | All        | Update email, notification preferences, password. Role-specific settings (dispatcher contractor list preferences, contractor availability hours). |

## Accessibility

**Target**: **WCAG AA** (Web Content Accessibility Guidelines Level AA)

- Keyboard navigation fully supported (Tab, Enter, Esc)
- Color contrast ratios meet AA standards (4.5:1 for body text, 3:1 for graphics)
- Semantic HTML (proper use of `<button>`, `<input>`, `<label>`, ARIA attributes)
- Focus indicators visible and clear
- Screen reader compatible (alt text for images, descriptive button labels)
- Avoid color-only information (use icons + text, not just color)

## Branding

**Visual Style**: Modern, clean, professional (not consumer-grade).

- **Color Palette**:
  - Primary: Indigo-600 (professional, trustworthy)
  - Secondary: Teal-500 (accent, energy)
  - Neutral: Gray-100 to Gray-900 (clean backgrounds and text)
- **Typography**: Clean sans-serif (e.g., Inter, Segoe UI)
- **Spacing**: Generous whitespace to reduce cognitive load; 8px base grid
- **Icons**: Simple, line-based (not filled/colorful emojis)
- **Photography**: None initially (can add contractor/customer avatars in Phase 2)

**Tone**: Professional but approachable. Clear, direct language. No marketing fluff.

## Target Device and Platforms

**Primary**:

- **Desktop/Tablet** (1024px+) - Dispatcher dashboard optimized for desktop (large monitors enable at-a-glance job board)
- **Mobile-responsive** (375px+) - Customer and Contractor portals fully responsive; Contractor portal optimized for mobile browsers (Phase 2: native apps)

**Browser Support**: Chrome, Firefox, Safari, Edge (latest 2 versions)

**Responsive Breakpoints**:

- **Desktop**: 1920px (primary dispatcher dashboard target)
- **Tablet**: 768px (iPad, etc.)
- **Mobile**: 375px (iPhone SE and up)

---
