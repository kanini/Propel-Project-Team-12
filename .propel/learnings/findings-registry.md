# Findings Registry

This file tracks recurring patterns, anti-patterns, and architectural decisions discovered during bug fixes and development.

## Frontend Architecture Patterns

### API Client Pattern (Updated: 2026-03-23)

**Pattern**: Project uses plain Redux Toolkit slices + fetch API, NOT RTK Query

**Files**:
- `src/frontend/src/api/*.ts` - API client functions
- `src/frontend/src/store/*.ts` - Redux store and slices  
- `src/frontend/src/store/slices/*.ts` - Feature slices

**Standard Pattern**:
```typescript
// ✅ CORRECT - Use plain async functions with fetch
export async function fetchResource(): Promise<ResourceDto> {
  const url = `${API_BASE_URL}/api/resource`;
  const token = localStorage.getItem('token');
  
  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` }),
    },
  });
  
  if (!response.ok) {
    throw new Error(`Failed to fetch: ${response.statusText}`);
  }
  
  return await response.json();
}
```

**Anti-Pattern**:
```typescript
// ❌ WRONG - Do NOT use RTK Query (no baseApi configured)
import { api } from './baseApi'; // This file doesn't exist!
export const resourceApi = api.injectEndpoints({ ... });
```

**Rationale**:
- No RTK Query middleware configured in store
- No `baseApi` file exists
- Existing code uses plain fetch pattern consistently
- Simpler for this project's complexity level

**Related Bugs**: bug_typescript_dashboard_errors

---

## Common Pitfalls

### TypeScript Implicit Any Types

**Issue**: Map callbacks with implicit any types fail compilation

**Solution**: Always add explicit type annotations
```typescript
// ❌ WRONG
items.map((item) => ...)

// ✅ CORRECT  
items.map((item: ItemDto) => ...)
```

**Rule**: Enable `noImplicitAny` in tsconfig.json - already enabled in this project

---

## Redux State Management

### State Update Pattern

**Standard**: Use Redux Toolkit slices with async thunks for API calls

**Example**:
```typescript
// providerSlice.ts pattern
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

export const fetchProviders = createAsyncThunk(
  'providers/fetch',
  async (params, { rejectWithValue }) => {
    try {
      return await providerApi.fetchProviders(params);
    } catch (error) {
      return rejectWithValue(error.message);
    }
  }
);

const providerSlice = createSlice({
  name: 'providers',
  initialState: { data: [], loading: false, error: null },
  extraReducers: (builder) => {
    builder
      .addCase(fetchProviders.pending, (state) => { state.loading = true; })
      .addCase(fetchProviders.fulfilled, (state, action) => { 
        state.data = action.payload;
        state.loading = false;
      })
      .addCase(fetchProviders.rejected, (state, action) => {
        state.error = action.payload;
        state.loading = false;
      });
  },
});
```

**Files to Reference**: `src/frontend/src/store/slices/providerSlice.ts`

---

## Error Handling Standards

### API Error Handling

**Pattern**: Consistent error messages and status code handling

```typescript
if (!response.ok) {
  if (response.status === 401) {
    throw new Error('Unauthorized. Please log in again.');
  }
  if (response.status === 404) {
    throw new Error('Resource not found');
  }
  if (response.status === 500) {
    throw new Error('Server error. Please try again later.');
  }
  throw new Error(`Failed: ${response.statusText}`);
}
```

**Requirement**: User-friendly error messages, no internal details exposed

---

## Component Patterns

### Data Fetching in Components

**Standard**: Use `useEffect` with cleanup for data fetching

```typescript
useEffect(() => {
  let isMounted = true;
  
  const loadData = async () => {
    try {
      const data = await fetchData();
      if (isMounted) {
        setData(data);
      }
    } catch (error) {
      if (isMounted) {
        setError(error.message);
      }
    }
  };
  
  loadData();
  
  return () => { isMounted = false; };
}, [dependencies]);
```

**Rationale**: Prevents state updates on unmounted components

---

## Next Steps

When adding new patterns or discovering bugs:

1. Document the pattern/anti-pattern here
2. Reference the bug ID or PR that discovered it
3. Provide code examples (correct and incorrect)
4. Update related documentation in task folders

---

**Last Updated**: March 23, 2026  
**Maintained By**: Development Team
