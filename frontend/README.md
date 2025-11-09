# SmartScheduler Frontend

Dispatcher dashboard UI for job assignment management.

## Tech Stack

- **Framework:** React 18.3.1 + TypeScript 5.9.2
- **Build Tool:** Vite 7.2.2 (HMR <100ms)
- **CSS:** Tailwind CSS 3.4 + shadcn/ui components
- **State:** React Context API + Custom Hooks
- **API:** Axios with JWT authentication
- **Testing:** Vitest + React Testing Library
- **Routing:** React Router 7.9

## Project Structure

```
src/
├── components/          # Shared UI components
│   ├── shared/         # LoadingSpinner, JobStatusBadge, etc.
│   ├── common/         # EmptyState, ProtectedRoute
├── contexts/           # Auth context provider
├── features/           # Feature-specific components
│   └── dispatcher/    # Dashboard, JobList, JobCard
├── hooks/             # Custom React hooks (useJobs)
├── services/          # API clients (dispatcherService)
├── types/             # TypeScript interfaces
├── utils/             # Config, helpers
├── App.tsx            # Main routing
└── main.tsx           # Entry point
```

## Quick Start

### Installation

```bash
npm install
```

### Development

```bash
npm run dev
```

Starts Vite dev server on `http://localhost:3000` with hot module replacement.

### Build

```bash
npm run build
```

Optimized production build to `dist/` directory (294 KB gzipped).

### Testing

```bash
npm test                 # Run tests in watch mode
npm test -- --run       # Run tests once
npm run test:coverage   # Generate coverage report
```

**Test Status:** ✅ 15/15 tests passing (100%)

- 9 JobList component tests
- 6 useJobs hook tests

## Core Features Implemented

### 1. Job List Display

- Real-time job list with pagination
- Color-coded status badges (Pending: yellow, Assigned: blue, etc.)
- Responsive grid layout (desktop: 5 columns, tablet: stacks)
- Empty state messaging

### 2. Dispatcher Dashboard

- Role-based access control via ProtectedRoute
- Header with dispatcher name
- Job refresh every 30 seconds (polling)
- Placeholder for recommendations (Story 3.2)

### 3. Data Management

- `useJobs` hook for centralized state
- Pagination controls (prev/next buttons)
- Sorting support (sortBy, sortOrder parameters)
- Error handling with user-friendly messages

### 4. API Integration

- JWT token injection in request headers
- Request cancellation on component unmount
- Automatic token refresh on 401 errors
- Transparent error handling

## Environment Variables

Create `.env` file:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_JWT_STORAGE_KEY=auth_token
```

## Performance

- **Initial Load:** ~300ms (Vite optimized bundling)
- **Bundle Size:** 294.30 KB (95.80 KB gzipped)
- **Re-renders:** Minimized via React.memo and useCallback
- **Polling Interval:** 30 seconds (configurable in `src/utils/config.ts`)

## Accessibility

- ARIA labels on interactive elements
- Semantic HTML (buttons, sections, articles)
- Keyboard navigation support
- Color-coded badges with text labels (not color-only)

## Next Steps (Future Stories)

- **Story 3.2:** Add contractor recommendations panel
- **Story 3.3:** Job detail view with assignment workflow
- **Story 6.6:** Replace polling with SignalR WebSocket for real-time updates
- **Story X.Y:** Implement AuthContext with actual login flow

## Contributing

Follow coding standards from `docs/architecture/17-coding-standards.md`:

- Strict TypeScript (no `any`)
- Component naming: PascalCase
- Hook naming: camelCase with `use` prefix
- Service methods: descriptive names with error handling
- All async operations: async/await (no .then() chains)

## Troubleshooting

### Port 3000 already in use

```bash
# Change port in vite.config.ts or:
lsof -ti:3000 | xargs kill -9
```

### Tests timing out

- Tests use real timers (no fake timers for polling)
- Increase timeout: `{ timeout: 10000 }` in waitFor()

### API connection errors

- Ensure backend is running on `http://localhost:5000`
- Check `VITE_API_BASE_URL` environment variable
- JWT token must be in `localStorage[auth_token]`

---

**Status:** ✅ Ready for Review (Story 3.1 Complete)
