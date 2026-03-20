# Task - task_001_frontend_vite_react_scaffolding

## Requirement Reference
- User Story: us_001
- Story Location: .propel/context/tasks/EP-TECH/us_001/us_001.md
- Acceptance Criteria:
    - AC-1: Vite-powered React 18 project created under `src/frontend/` with TypeScript strict mode enabled
    - AC-2: `tsconfig.json` has `strict: true`, `tailwind.config.js` includes custom design tokens, `vite.config.ts` has proper build settings
    - AC-3: Development server starts successfully with hot module replacement active
    - AC-4: Redux Toolkit configured with root reducer, store, and proper TypeScript typing for `RootState` and `AppDispatch`
    - AC-5: Tailwind CSS styles apply correctly in development and production builds
- Edge Case:
    - Node.js version below 18 should fail with clear error message
    - Conflicting Tailwind configuration with custom tokens should prioritize custom tokens

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React | 18.x |
| Frontend | TypeScript | 5.x |
| Frontend | Redux Toolkit | 2.x |
| Frontend | Tailwind CSS | 3.x |
| Build Tool | Vite | 5.x |
| Library | React Router | 6.x |

**Note**: All code and libraries MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview
Initialize a production-ready React 18 frontend project using Vite as the build tool, with TypeScript strict mode, Redux Toolkit for state management, and Tailwind CSS for styling. The project will serve as the foundation for all frontend development, including authentication screens, appointment booking flows, and clinical intelligence dashboards. This task establishes the directory structure, configuration files, development server setup, and base component architecture following React best practices.

## Dependent Tasks
- None

## Impacted Components
- **NEW** src/frontend/package.json - Project dependencies and scripts
- **NEW** src/frontend/vite.config.ts - Vite build configuration
- **NEW** src/frontend/tsconfig.json - TypeScript compiler options
- **NEW** src/frontend/tailwind.config.js - Tailwind CSS configuration
- **NEW** src/frontend/src/store/index.ts - Redux store setup
- **NEW** src/frontend/src/App.tsx - Root React component
- **NEW** src/frontend/index.html - HTML entry point

## Implementation Plan
1. **Initialize Vite React-TypeScript Project**: Use `npm create vite@latest frontend -- --template react-ts` to scaffold base project with React 18 and TypeScript 5
2. **Configure TypeScript Strict Mode**: Update `tsconfig.json` to enable all strict type checking flags including `strict: true`, `noUncheckedIndexedAccess`, `noImplicitOverride`
3. **Install and Configure Tailwind CSS**: Add Tailwind CSS via PostCSS, create `tailwind.config.js` with custom design tokens for colors, typography, and spacing
4. **Install and Configure Redux Toolkit**: Install `@reduxjs/toolkit` and `react-redux`, create store with typed hooks (`useAppDispatch`, `useAppSelector`)
5. **Setup Project Structure**: Create folder structure (`components/`, `pages/`, `store/slices/`, `hooks/`, `utils/`, `types/`)
6. **Configure Vite Build Settings**: Update `vite.config.ts` with path aliases, environment variable handling, and production optimization settings
7. **Add Node Version Check**: Create `.nvmrc` file specifying Node.js 18 minimum and add version check script to `package.json`
8. **Verify Hot Module Replacement**: Test development server startup and HMR functionality with sample component changes

## Current Project State
```
Propel-Project-Team-12/
├── .propel/
├── .github/
├── README.md
└── (No frontend code exists yet)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/package.json | Project dependencies including React 18, Vite 5, TypeScript 5, Redux Toolkit 2, Tailwind CSS 3 |
| CREATE | src/frontend/vite.config.ts | Vite configuration with path aliases (@/), environment variables, and build optimization |
| CREATE | src/frontend/tsconfig.json | TypeScript configuration with strict mode, path mappings, and React JSX settings |
| CREATE | src/frontend/tailwind.config.js | Tailwind configuration with custom design tokens (colors, typography, spacing) |
| CREATE | src/frontend/postcss.config.js | PostCSS configuration for Tailwind CSS processing |
| CREATE | src/frontend/.nvmrc | Node.js version specification (18.x minimum) |
| CREATE | src/frontend/index.html | HTML entry point with root div and script tag |
| CREATE | src/frontend/src/main.tsx | Application entry point mounting React to DOM with Redux Provider |
| CREATE | src/frontend/src/App.tsx | Root React component with basic routing structure |
| CREATE | src/frontend/src/store/index.ts | Redux store configuration with typed hooks (useAppDispatch, useAppSelector) |
| CREATE | src/frontend/src/store/rootReducer.ts | Root reducer combining all slice reducers |
| CREATE | src/frontend/src/styles/index.css | Global styles with Tailwind directives (@tailwind base, components, utilities) |
| CREATE | src/frontend/src/vite-env.d.ts | Vite client types reference |
| CREATE | src/frontend/.gitignore | Frontend-specific gitignore (node_modules, dist, .env.local) |

## External References
- React 18 Documentation: https://react.dev/learn
- Vite Configuration Reference: https://vitejs.dev/config/
- TypeScript Strict Mode: https://www.typescriptlang.org/tsconfig#strict
- Redux Toolkit Quick Start: https://redux-toolkit.js.org/tutorials/quick-start
- Tailwind CSS Configuration: https://tailwindcss.com/docs/configuration
- React TypeScript Cheatsheet: https://react-typescript-cheatsheet.netlify.app/

## Build Commands
```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Type check
npm run typecheck
```

## Implementation Validation Strategy
- [x] Unit tests pass (N/A for scaffolding task)
- [x] Integration tests pass (N/A for scaffolding task)
- [x] TypeScript compiler runs without errors (`npm run typecheck`)
- [x] Development server starts successfully on localhost with HMR active
- [x] Production build completes without errors and outputs to `dist/`
- [x] Tailwind utility classes render correctly in browser
- [x] Redux DevTools Extension connects and shows store state
- [x] Node.js version check script fails gracefully on Node < 18

## Implementation Checklist
- [x] Scaffold Vite React-TypeScript project using `npm create vite@latest frontend -- --template react-ts`
- [x] Update `tsconfig.json` to enable strict mode and configure path aliases
- [x] Install Tailwind CSS dependencies (`tailwindcss`, `postcss`, `autoprefixer`)
- [x] Configure Tailwind CSS in `tailwind.config.js` with custom design tokens
- [x] Install Redux Toolkit and React-Redux (`@reduxjs/toolkit`, `react-redux`)
- [x] Create Redux store in `src/store/index.ts` with typed hooks
- [x] Update `vite.config.ts` with path aliases and environment variable handling
- [x] Create `.nvmrc` file and add version check script to `package.json`
