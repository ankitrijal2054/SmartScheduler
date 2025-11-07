# 17. Coding Standards

## 17.1 Critical Fullstack Rules

- **Type Sharing:** All models defined in shared types, no duplication
- **API Calls:** Always use service layer, never direct fetch/axios in components
- **Environment Variables:** Access only through config objects
- **Error Handling:** All API routes use global exception handler
- **State Updates:** Never mutate state directly, use setState/dispatch
- **Database Queries:** Always use repository pattern, controllers never access DbContext directly
- **Async/Await:** All async operations use async/await, no .then() chains

## 17.2 Naming Conventions

| Element         | Frontend             | Backend    | Example                           |
| --------------- | -------------------- | ---------- | --------------------------------- |
| Components      | PascalCase           | -          | `UserProfile.tsx`                 |
| Hooks           | camelCase with 'use' | -          | `useAuth.ts`                      |
| API Routes      | kebab-case           | -          | `/api/dispatcher/contractor-list` |
| Database Tables | PascalCase           | PascalCase | `Contractors`, `Jobs`             |
| Constants       | SCREAMING_SNAKE_CASE | PascalCase | `MAX_RECOMMENDATIONS = 5`         |

---
