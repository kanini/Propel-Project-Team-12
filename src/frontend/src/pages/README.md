# Pages

This directory contains page components for different routes in the application.

## Structure

Each page represents a route in the application and should:
- Be in its own directory
- Include necessary child components
- Handle page-level data fetching
- Connect to Redux store as needed

## Example Structure

```
pages/
├── Home/
│   ├── index.tsx
│   └── Home.tsx
├── Login/
│   ├── index.tsx
│   └── Login.tsx
└── Dashboard/
    ├── index.tsx
    └── Dashboard.tsx
```
