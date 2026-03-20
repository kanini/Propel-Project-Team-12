# Component Inventory — Patient Access & Clinical Intelligence Platform

## 1. Layout Components

| Component | Variant | Screens Used | Count |
|-----------|---------|--------------|-------|
| Shell (Header + Sidebar + Main) | Patient | SCR-003, SCR-006–SCR-016 | 13 |
| Shell (Header + Sidebar + Main) | Staff | SCR-004, SCR-017–SCR-020, SCR-023–SCR-024 | 7 |
| Shell (Header + Sidebar + Main) | Admin | SCR-005, SCR-021–SCR-022, SCR-025–SCR-026 | 5 |
| Centered Auth Card | Public | SCR-001, SCR-002 | 2 |
| Centered Success Card | Confirmation | SCR-008 | 1 |
| Split Panel (Main + Side) | Staff clinical | SCR-017, SCR-023 | 2 |
| Side-by-Side Comparison | Conflict | SCR-024 | 1 |

## 2. Navigation Components

| Component | Variant | Screens Used | Properties |
|-----------|---------|--------------|------------|
| Header | Standard | All authenticated (24 screens) | Logo, breadcrumb, user avatar |
| Sidebar | Patient (6 items) | SCR-003, SCR-006–SCR-016 | 240px, collapsible |
| Sidebar | Staff (6 items) | SCR-004, SCR-017–SCR-020, SCR-023–SCR-024 | 240px, collapsible |
| Sidebar | Admin (4 items) | SCR-005, SCR-021–SCR-022, SCR-025–SCR-026 | 240px, collapsible |
| Nav Item | Default, Hover, Active | All sidebar screens | Icon + label, active border-left |
| Breadcrumb | Standard | All authenticated screens | Home / Context path |
| Tabs | Horizontal | SCR-010 | Upcoming / Past / Waitlist |
| Pagination | Standard | SCR-021, SCR-025 | Page numbers, prev/next |

## 3. Content Components

| Component | Variant | Screens Used | Properties |
|-----------|---------|--------------|------------|
| Card | Standard | 20+ screens | Border, shadow, padding 20px |
| StatCard | Dashboard | SCR-003, SCR-004, SCR-005, SCR-019, SCR-020 | Value + label + trend note |
| Accordion | Expandable | SCR-016, SCR-017 | Header + body, chevron toggle |
| Table | Standard | SCR-003–SCR-005, SCR-010, SCR-015, SCR-017, SCR-019–SCR-021, SCR-023, SCR-025 | Header row, data rows, responsive overflow |
| Badge | Success | SCR-016, SCR-017, SCR-023 + status tables | Green: Verified, Active |
| Badge | Warning | SCR-016, SCR-017, SCR-023, SCR-024 | Amber: AI-suggested, Pending |
| Badge | Error | SCR-020, SCR-023, SCR-024, SCR-025 | Red: No Show, Conflict, Failed |
| Badge | Info | SCR-016, SCR-017, SCR-021, SCR-023, SCR-025 | Blue: Count, Action type |
| Badge | Neutral | SCR-019, SCR-020, SCR-021 | Gray: Checked In, Inactive |
| Chart Placeholder | Line Chart | SCR-016, SCR-017 | Vitals trend area |
| Document Preview | Thumbnail | SCR-017, SCR-023 | PDF icon, name, metadata |
| Chat Bubble | AI / User | SCR-012 | Directional, colored |
| Typing Indicator | Animated | SCR-012 | Three-dot animation |
| Progress Bar | Upload | SCR-014 | Fill percentage |
| Calendar | Date Grid | SCR-007, SCR-011, SCR-018 | Month view with day states |
| Time Slot Grid | Selectable | SCR-007, SCR-011, SCR-018 | Available/selected/unavailable states |
| Confidence Bar | Percentage | SCR-023 | Fill + percentage label |
| Stepper | Multi-step | SCR-007, SCR-013 | 4-step horizontal progress |
| Conflict Card | Comparison | SCR-024 | Source A vs Source B layout |

## 4. Interactive Components

| Component | Variant | Screens Used | States |
|-----------|---------|--------------|--------|
| Button | Primary | All screens | Default, Hover, Focus, Disabled |
| Button | Outline / Secondary | All screens | Default, Hover, Focus |
| Button | Success / Danger | SCR-017, SCR-022, SCR-023, SCR-024 | Verify / Reject actions |
| Button | Small (btn-sm) | Table action cells | Compact inline actions |
| TextField | Standard | SCR-001, SCR-002, SCR-007, SCR-009, SCR-013, SCR-018, SCR-022 | Default, Focus, Error, Disabled |
| Select | Standard | SCR-018, SCR-019, SCR-021, SCR-022, SCR-025, SCR-026 | Default, Focus |
| Search Input | With button | SCR-018, SCR-020, SCR-021, SCR-025 | Default, Focus, Active |
| Toggle Switch | Boolean | SCR-022, SCR-026 | Active, Inactive |
| Radio Group | Options | SCR-009, SCR-024 | Selected, Unselected |
| Modal | Confirmation | SCR-010 | Overlay + card, ESC dismiss |
| Link | Standard | All screens | Default, Hover, Focus |
| File Upload Zone | Drag & Drop | SCR-014 | Default, Hover, Uploading, Complete, Error |

## 5. Feedback Components

| Component | Variant | Screens Used | Properties |
|-----------|---------|--------------|------------|
| Alert | Info | SCR-009, SCR-018 | Blue background, icon, message |
| Alert | Warning | SCR-022 | Amber background, icon, message |
| Conflict Banner | Error | SCR-024 | Red background, severity badge |
| Tooltip Icon | Help | SCR-016, SCR-017 | "?" circle, hover text |
| Empty State | Placeholder | Referenced in spec (not wireframed in default state) | Icon + message + CTA |

## 6. States Matrix

| State | Components Affected | Implementation |
|-------|--------------------:|----------------|
| Default | All | Base CSS styles |
| Hover | Buttons, Nav Items, Cards, Time Slots | `:hover` transition |
| Focus | All interactive | `:focus-visible` outline 2px primary |
| Active/Selected | Nav Item, Time Slot, Calendar Day, Toggle, Radio | `.active` / `.selected` class |
| Disabled | Calendar Days, Time Slots, Buttons | `.disabled` / `[aria-disabled]` |
| Error | Form fields (referenced in spec) | Red border + error message below |
| Loading | Tables, Cards (referenced in spec) | Skeleton placeholder |

## 7. Reusability Analysis

| Component | Reuse Count | Cross-Role | Notes |
|-----------|-------------|------------|-------|
| Shell Layout | 25 screens | Yes (3 variants) | Most reused — header + sidebar pattern |
| Card | 20+ instances | Yes | Universal container |
| Button (Primary) | 26 screens | Yes | Always present |
| Badge | 15+ screens | Yes | Status indicators everywhere |
| Table | 13 screens | Yes | Data display workhorse |
| Search Bar | 4 screens | Yes | Consistent pattern |
| Form Group | 7 screens | Yes | Label + input + hint |
| Pagination | 2 screens | Admin only | User list + Audit log |

## 8. Framework Mapping

| Component | React Component | Tailwind Classes |
|-----------|----------------|------------------|
| Shell | `<AppShell>` | `grid grid-cols-[240px_1fr] grid-rows-[64px_1fr]` |
| Card | `<Card>` | `bg-white border rounded-md shadow-sm p-5` |
| Button | `<Button variant>` | `inline-flex items-center gap-1.5 px-4 py-2 rounded-md font-medium` |
| Badge | `<Badge variant>` | `inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium` |
| Table | `<DataTable>` | `w-full border-collapse` with `th` and `td` styling |
| TextField | `<Input>` | `w-full px-3.5 py-2.5 border rounded-md text-sm` |
| Toggle | `<Switch>` | `relative w-11 h-6 rounded-full` with pseudo-element |
| Accordion | `<Accordion>` | `border rounded-md overflow-hidden` |
| Sidebar | `<Sidebar>` | `bg-neutral-50 border-r p-4` |
