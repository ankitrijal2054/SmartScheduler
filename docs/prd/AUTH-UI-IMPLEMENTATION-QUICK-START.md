# Auth UI Implementation: Quick Start Guide

**Objective:** Build Sign-In/Sign-Up screens to unblock manual testing  
**Estimated Effort:** 2-3 days  
**Priority:** 🚨 CRITICAL BLOCKER

---

## Quick Facts

- **Status:** NOT STARTED
- **Dependency:** Story 1.3 (Auth Backend) ✅ COMPLETE
- **Blocks:** All manual testing of Epics 2-5
- **Epic:** 1 (Foundation & Infrastructure)
- **Story ID:** 1.3B
- **Timeline:** Start immediately

---

## What to Build (High Level)

```
/login → Sign-In Page
  ├─ Email input
  ├─ Password input
  ├─ Sign In button
  └─ → Call POST /api/v1/auth/login

/signup → Sign-Up Page
  ├─ Email input
  ├─ Password input
  ├─ Role selector (Dispatcher/Customer/Contractor)
  └─ → Call POST /api/v1/auth/signup

AuthContext (React Context)
  ├─ Store JWT token
  ├─ Store user info (email, role)
  ├─ Handle token refresh
  └─ Auto-logout on expiry

ProtectedRoute Component
  ├─ Check if JWT exists
  ├─ Redirect to /login if not
  └─ Enforce role-based access

Navigation/Header
  ├─ Show user role
  ├─ Settings button
  └─ Logout button
```

---

## Development Checklist (Quick Start)

### Phase 1: Setup (2-3 hours)

- [ ] Create `contexts/AuthContext.tsx` (state + hooks)
- [ ] Create `services/authService.ts` (API calls)
- [ ] Create `components/ProtectedRoute.tsx` (route guard)
- [ ] Create `components/Login.tsx` (sign-in form)
- [ ] Create `components/Signup.tsx` (sign-up form)

### Phase 2: Implementation (8-12 hours)

- [ ] Sign-In page: form validation + API integration
- [ ] Sign-Up page: form validation + role selector + API integration
- [ ] AuthContext: login/logout/refresh logic
- [ ] API interceptor: attach JWT to every request
- [ ] ProtectedRoute: guard /dispatcher, /customer, /contractor routes
- [ ] Role-based redirect: After login, send to correct dashboard

### Phase 3: Testing (6-8 hours)

- [ ] Manual sign-in with valid credentials
- [ ] Manual sign-in with invalid credentials
- [ ] Manual sign-up with all roles
- [ ] Token refresh works (wait 1+ hour in JWT exp, test)
- [ ] Logout clears tokens
- [ ] Protected routes work
- [ ] Responsive design (mobile/tablet/desktop)
- [ ] Accessibility (keyboard nav, contrast)

---

## Critical APIs to Call

**Backend expects these endpoints (verify they exist):**

```bash
# Sign-In
POST /api/v1/auth/login
Content-Type: application/json
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}

Response (200 OK):
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "user": {
    "id": "user-123",
    "email": "user@example.com",
    "role": "Dispatcher"
  }
}

# Sign-Up
POST /api/v1/auth/signup
{
  "email": "newuser@example.com",
  "password": "SecurePassword123",
  "role": "Customer"
}

Response (201 Created): Same as login response

# Refresh Token
POST /api/v1/auth/refresh
Content-Type: application/json
{ "refreshToken": "eyJhbGc..." }

Response (200 OK):
{ "accessToken": "eyJhbGc..." }

# Logout
POST /api/v1/auth/logout
Authorization: Bearer <JWT_TOKEN>

Response (200 OK):
{ "message": "Logged out successfully" }
```

**If `/api/v1/auth/signup` doesn't exist, create it on backend first.**

---

## Tech Stack

```typescript
// Frontend libraries (already installed?)
- react-router-dom (routing)
- axios (HTTP client)
- tailwind css (styling)

// Create your own:
- contexts/AuthContext.tsx (React Context)
- services/authService.ts (API wrapper)
- components/ProtectedRoute.tsx (route guard)
- components/Login.tsx (sign-in form)
- components/Signup.tsx (sign-up form)
```

---

## Token Storage Strategy

```javascript
// Login success
const { accessToken, refreshToken } = response.data;

// Store tokens
localStorage.setItem("accessToken", accessToken);
localStorage.setItem("refreshToken", refreshToken);

// On every API call
const token = localStorage.getItem("accessToken");
headers["Authorization"] = `Bearer ${token}`;

// On logout
localStorage.removeItem("accessToken");
localStorage.removeItem("refreshToken");
```

---

## Key Integration Points

### 1. After Sign-In Success

```javascript
const { role } = jwtPayload; // Extract from JWT

// Redirect based on role
if (role === "Dispatcher") navigate("/dispatcher/dashboard");
else if (role === "Customer") navigate("/customer/jobs");
else if (role === "Contractor") navigate("/contractor/assignments");
```

### 2. Protect All Authenticated Routes

```jsx
// In main App.tsx or routing file
<Route path="/dispatcher/*" element={<ProtectedRoute requiredRole="Dispatcher"><DispatcherLayout /></ProtectedRoute>} />
<Route path="/customer/*" element={<ProtectedRoute requiredRole="Customer"><CustomerLayout /></ProtectedRoute>} />
<Route path="/contractor/*" element={<ProtectedRoute requiredRole="Contractor"><ContractorLayout /></ProtectedRoute>} />
```

### 3. Token Refresh Before API Calls

```javascript
// In axios interceptor
const token = localStorage.getItem("accessToken");
const decoded = jwtDecode(token);

if (decoded.exp * 1000 < Date.now() + 5 * 60 * 1000) {
  // Expires in <5 minutes, refresh now
  const newToken = await refreshAccessToken();
  localStorage.setItem("accessToken", newToken);
}

// Then make API call with fresh token
```

---

## Test Scenarios (Manual QA)

| Scenario          | Steps                                                                              | Expected Result                                                    |
| ----------------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| Valid Sign-In     | 1. Go to `/login` 2. Enter valid email + password 3. Click Sign In                 | JWT stored, redirected to dispatcher/customer/contractor dashboard |
| Invalid Sign-In   | 1. Go to `/login` 2. Enter invalid email/password 3. Click Sign In                 | Error message shown, still on login page                           |
| Sign-Up           | 1. Go to `/signup` 2. Fill form with email, password, role 3. Click Create Account | JWT stored, redirected to dashboard, role-specific UI shown        |
| Logout            | 1. Login 2. Click Logout button                                                    | JWT cleared, redirected to `/login`                                |
| Protected Route   | 1. Don't login 2. Try to access `/dispatcher/dashboard`                            | Redirected to `/login`                                             |
| Role Mismatch     | 1. Login as Contractor 2. Try to access `/dispatcher/dashboard`                    | Redirected to `/login` (403 in backend)                            |
| Mobile Responsive | 1. Login on mobile device (375px)                                                  | UI readable, buttons clickable, no overflow                        |
| Token Expiry      | 1. Login 2. Wait 1+ hour 3. Make API call                                          | Auto-refresh, call succeeds without re-login                       |

---

## Expected Bugs (Don't Panic!)

Once you test, you might find:

- CORS issues calling backend → Fix backend CORS headers
- API endpoint not found (404) → Verify backend endpoints exist
- JWT malformed → Check backend token generation
- Role not in JWT → Verify backend includes role in payload
- Mobile layout broken → Use Tailwind responsive classes

**This is normal!** Document bugs, fix them, re-test.

---

## File Structure

```
frontend/src/
├── components/
│   ├── Login.tsx ← NEW
│   ├── Signup.tsx ← NEW
│   ├── ProtectedRoute.tsx ← NEW
│   └── Navigation.tsx (update for logout)
├── contexts/
│   ├── AuthContext.tsx ← NEW
│   └── ... (existing)
├── services/
│   ├── authService.ts ← NEW
│   ├── apiClient.ts ← UPDATE (add interceptor)
│   └── ... (existing)
├── App.tsx ← UPDATE (add routes, AuthProvider)
└── ...
```

---

## Acceptance Criteria (Copy-Paste for Dev)

✅ Must haves:

- [ ] Sign-In page at `/login`
- [ ] Sign-Up page at `/signup`
- [ ] JWT token stored in localStorage
- [ ] JWT token retrieved on API calls
- [ ] Role-based redirect after login
- [ ] ProtectedRoute prevents unauthenticated access
- [ ] Logout clears tokens and redirects to login
- [ ] Error messages display for invalid credentials
- [ ] Responsive on mobile/tablet/desktop

🔧 Quality:

- [ ] No console errors
- [ ] Keyboard navigation works
- [ ] Color contrast WCAG AA
- [ ] All tests pass

---

## Next Steps

1. **Review** full Story 1.3B: `docs/prd/story-1-3b-authentication-ui.md`
2. **Review** Course Correction: `docs/prd/COURSE-CORRECTION.md`
3. **Start** Phase 1 (Setup): Create files
4. **Start** Phase 2 (Implementation): Build forms + logic
5. **Test** Phase 3: Verify all scenarios work
6. **Deploy** to main branch + update PRD

---

**Estimated Timeline: 2-3 days**  
**Priority: CRITICAL BLOCKER**  
**Start: Immediately**
