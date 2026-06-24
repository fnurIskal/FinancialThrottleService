# Cloud.md - Financial Throttle Frontend Development Guide

## PROJECT OVERVIEW

**Stack:**
- Framework: React 18+ with TypeScript (strict mode)
- Build Tool: Vite (fastest development setup)
- State Management: React Context + Hooks
- HTTP Client: axios
- UI Components: Custom components + TailwindCSS
- Real-time: WebSocket for logs
- Package Manager: npm

**Setup Command:**
```bash
npm create vite@latest financial-throttle-frontend -- --template react-ts
cd financial-throttle-frontend
npm install
npm install axios zustand react-hot-toast lucide-react
npm run dev
```

---

## PART 1: API RESPONSE CONTRACTS

### 1.1 Queue Endpoint - GET /api/queue

**Response Structure:**
```typescript
interface QueueResponse {
  groups: WaitingGroup[];
}

interface WaitingGroup {
  databaseName: string;           // "RAS_101"
  securityId: number;              // 68
  templateId: number;              // 21
  securityCode: string;            // "SEC_68"
  priorityScore: number;           // 0-1 (0.95)
  itemCount: number;               // 10
  items: WaitingItem[];
}

interface WaitingItem {
  quarter: number;                 // 202503, 202512, 202603
  isOriginal: boolean;             // false (Q202503), true (Q202603*)
  username: string;                // "Sentris", "boss"
  disclosureId: number;            // 1605794
}
```

**Real Example:**
```json
{
  "groups": [
    {
      "databaseName": "RAS_101",
      "securityId": 68,
      "templateId": 21,
      "securityCode": "SEC_68",
      "priorityScore": 0,
      "itemCount": 10,
      "items": [
        {
          "quarter": 202503,
          "isOriginal": false,
          "username": "Sentris",
          "disclosureId": 1605794
        },
        {
          "quarter": 202603,
          "isOriginal": true,
          "username": "Sentris",
          "disclosureId": 1605794
        }
      ]
    }
  ]
}
```

---

### 1.2 Status Endpoint - GET /api/status

**Response Structure:**
```typescript
interface StatusResponse {
  status: "Running" | "Idle" | "Error";
  queuedCount: number;
  suspendedCount: number;
  lastHeartbeat: string;  // ISO 8601 timestamp
  timestamp: string;
}
```

**Real Example:**
```json
{
  "status": "Running",
  "queuedCount": 42,
  "suspendedCount": 3,
  "lastHeartbeat": "2026-06-23T10:12:15.437Z",
  "timestamp": "2026-06-23T10:15:30.000Z"
}
```

---

### 1.3 Logs Endpoint - GET /api/logs/{date}?category={category}&level={level}

**Response Structure:**
```typescript
interface LogEntry {
  _id: string;
  timestamp: string;           // ISO 8601
  level: "Information" | "Warning" | "Error" | "Debug";
  category: "system" | "financial" | "api" | "consistency";
  message: string;
  groupKey: string;            // "RAS_101|753|2"
  securityCode: string;
  databaseName: string;
  securityId: number;
  templateId: number;
  quarter?: number;
  exception: string | null;
  metadata: Record<string, any> | null;
}

interface LogsResponse {
  logs: LogEntry[];
  totalCount: number;
  date: string;
}
```

**Real Example:**
```json
{
  "logs": [
    {
      "_id": "6a390a7fc718233f42d26921",
      "timestamp": "2026-06-22T10:12:15.437Z",
      "level": "Information",
      "category": "financial",
      "message": "GÖNDER: RAS_101|753|2 Quarter=202503",
      "groupKey": "RAS_101|753|2",
      "securityCode": "boss",
      "databaseName": "RAS_101",
      "securityId": 753,
      "templateId": 2,
      "quarter": 202503,
      "exception": null,
      "metadata": null
    }
  ],
  "totalCount": 150,
  "date": "2026-06-22"
}
```

---

### 1.4 Suspended Endpoint - GET /api/suspended

**Response Structure:**
```typescript
interface SuspendedGroup {
  databaseName: string;
  securityId: number;
  securityCode: string;
  templateId: number;
  failureCount: number;
  firstFailedUtc: string;      // ISO 8601
  nextRetryUtc: string;         // ISO 8601
  failureReason: string;        // "Database connection timeout"
  retryInProgress: boolean;
}

interface SuspendedResponse {
  groups: SuspendedGroup[];
  totalCount: number;
}
```

**Real Example:**
```json
{
  "groups": [
    {
      "databaseName": "DB_PAYROLL",
      "securityId": 456,
      "securityCode": "SEC_456",
      "templateId": 2302,
      "failureCount": 3,
      "firstFailedUtc": "2026-06-22T08:30:00Z",
      "nextRetryUtc": "2026-06-23T14:30:00Z",
      "failureReason": "Connection timeout - max retries exceeded",
      "retryInProgress": false
    }
  ],
  "totalCount": 3
}
```

---

### 1.5 Process Group - POST /api/queue/{databaseName}/{securityId}/{templateId}/process

**Response:**
```typescript
interface ProcessResponse {
  success: boolean;
  message: string;
  groupKey: string;
  timestamp: string;
}
```

---

### 1.6 Retry Suspended - POST /api/suspended/{databaseName}/{securityId}/{templateId}/retry

**Response:**
```typescript
interface RetryResponse {
  success: boolean;
  message: string;
  retryScheduled: boolean;
  nextRetryTime: string;
}
```

---

## PART 2: UI COMPONENTS SPECIFICATIONS

### 2.1 Color & Design Tokens

**Colors:**
- Primary: #667eea (purple)
- Secondary: #764ba2 (dark purple)
- Success: #10b981 (green)
- Warning: #f59e0b (orange)
- Error: #ef4444 (red)
- Info: #3b82f6 (blue)
- Background: #f5f5f5
- Dark Background: #1f2937
- White: #ffffff

**Typography:**
- Title: 28px, 700 weight
- Heading: 20px, 700 weight
- Subtitle: 14px, 500 weight
- Body: 12px, 400 weight
- Caption: 11px, 400 weight

**Spacing:** 4px, 8px, 12px, 16px, 20px, 24px, 32px

**Shadows:**
- Light: 0 2px 8px rgba(0,0,0,0.05)
- Medium: 0 8px 24px rgba(0,0,0,0.12)
- High: 0 16px 48px rgba(0,0,0,0.16)

---

### 2.2 Sidebar Component

**Structure:**
- Logo: "💰 Financial Throttle"
- Menu Items:
  - Dashboard (active highlight - purple bg)
  - Queue
  - Suspended
  - Logs
  - Settings
- Premium Box (purple gradient)

**States:**
- Active: #ede9ff background, purple text
- Hover: #f3f4f6 background
- Inactive: gray text

---

### 2.3 Status Cards (Dashboard)

**4 Cards:**
1. Awaiting Processing (42) - orange icon
2. Active right now (7) - green icon
3. Resource attention (3) - red icon
4. Avg of last 14 days (1m 24s) - purple icon

**Card Style:** White bg, subtle shadow, hover lift effect

---

### 2.4 Queue Table Columns

```
| Database Name | Security Code | Template ID | Quarter | Item Count | Score (%) | Status | Actions |
```

**Colors by Priority:**
- Red (#ef4444): Low/Critical
- Orange (#f59e0b): Medium
- Green (#10b981): High/Normal
- Blue (#3b82f6): Info

**Score Bar:** Horizontal bar chart 0-100%

---

### 2.5 Logs Viewer

**Features:**
- Search box
- Date picker (Jun 16, 2026)
- Level filter: All, System, Financial, API, Consistency, Error
- Category filter toggle buttons
- Export button

**Log Display:**
- Timestamp | Level | Category | Message (in dark themed code block)
- Monospace font
- Color-coded levels (INFO=blue, WARNING=yellow, ERROR=red)

---

### 2.6 Suspended Table Columns

```
| Database Name | Security Code | Template ID | First Error Time | Attempts | Failure Reason | Next Retry | Actions |
```

**Failure Reason Colors:**
- Orange (#f59e0b): "Connection timeout"
- Red (#ef4444): "Auth failed", "Audit request"
- Yellow (#eab308): "API schema mismatch"

**Actions:** Manual Retry button (purple)

---

## PART 3: PAGE SPECIFICATIONS

### PAGE 1: DASHBOARD

**Components:**
1. Header with greeting + search + notifications
2. 4 Status Cards (grid layout)
3. "Last 5 Processed Groups" table
   - Show: Database, Security Code, Template ID, Quarter, Items, Score, Status, Timestamp
   - Status colors: green (Success), warning (Processing), info (Pending)
   - Each row clickable → Detail Modal

**Data Binding:**
- Status cards from GET /api/status
- Queue table from GET /api/queue (limit to 5)
- Real-time updates every 30 seconds

**Interactions:**
- Click row → Open group detail modal
- Click refresh icon → Manual fetch
- Status cards show live data

---

### PAGE 2: QUEUE

**Components:**
1. Header + search
2. Filter Section:
   - "All Databases" dropdown (RAS_101, RAS_STAJ107, RAS_STAJ32501)
   - "All Status" dropdown (Processing, Pending, etc)
   - "Priority" dropdown (High, Medium, Low)
   - "Refresh" button (purple)
3. Queue Table (full list with pagination)
   - Columns: Database Name, Security Code, Template ID, Quarter, Item Count, Score %, Status, Actions
   - Pagination: 1-5 pages shown, "Showing 1-15 of 163 groups"
   - Each row has "View" and "Process" buttons

**Data Binding:**
- Table from GET /api/queue
- Apply client-side filters (database, status, priority)
- Paginate 15 items per page

**Interactions:**
- Filter change → Table update
- View button → Detail modal
- Process button → Confirmation modal → POST /api/queue/.../process
- Search → Filter by database name or security code

---

### PAGE 3: SUSPENDED

**Components:**
1. Header
2. Alert Box: "3 groups are suspended. Review failure reasons and retry if necessary."
3. Suspended Table
   - Columns: Database Name, Security Code, Template ID, First Error Time, Attempts, Failure Reason, Next Retry, Actions
   - Attempts shown as red badges (3)
   - Each row has "Manual Retry" button

**Data Binding:**
- Table from GET /api/suspended
- Show failureReason, firstFailedUtc, failureCount, nextRetryUtc

**Interactions:**
- Manual Retry button → POST /api/suspended/.../retry
- Success → Toast notification + table refresh
- Show next retry time countdown (if > 0 seconds)

---

### PAGE 4: LOGS

**Components:**
1. Header + search
2. Controls:
   - Search box: "Search logs..."
   - Date picker: Default today (Jun 16, 2026)
   - Level filter: All | System | Financial | API | Consistency | Error
   - Export button
3. Log Viewer (dark themed code block)
   - Display logs with syntax highlighting
   - Timestamp | Level | Category | Message
   - Scrollable, monospace font
   - Max height 600px, overflow scroll

**Data Binding:**
- GET /api/logs/{date}?level={level}&category={category}
- Format: [HH:MM:SS] [LEVEL] [CATEGORY] Message
- Color by level: INFO=blue, WARNING=yellow, ERROR=red

**Interactions:**
- Date change → Fetch new logs
- Level/Category filter → Client-side filter or refetch
- Search box → Filter message text (client-side)
- Export → Download as JSON

---

### PAGE 5: SETTINGS

**Components:**
1. Worker Configuration Section
   - Max Parallel Groups: Slider 1-10 (default 5)
   - Worker Interval (seconds): Slider 10-120 (default 30)
   - Use Dummy Data: Toggle ON/OFF
2. API Configuration Section
   - FinancialTransaction API URL: Input field
   - Priority Client API URL: Input field
   - Timeout (seconds): Slider 30-300
   - Max Retries: Slider 1-10
3. Database Configuration Section
   - Test buttons for each connection (RAS_STAJ, RAS_STAJ107, RAS_STAJ32501, MongoDB)
   - Status indicators (green ✅ / red ❌)
4. Save & Reset buttons

**Data Binding:**
- Load from localStorage or config API
- Save changes to localStorage

**Interactions:**
- Toggle switches
- Slider changes
- Test connection buttons → Show loading spinner → Status update
- Save → Toast notification

---

## PART 4: DETAIL MODAL / GROUP VIEW

**Trigger:** Click on any group row

**Content:**
1. Header: "GROUP DETAILS: {databaseName} | {securityId} | {securityCode} | Template: {templateId}"
2. General Info Section:
   - Database, Security ID, Security Code, Template ID, Priority Score, Created Time, Last Updated
3. Items Section (expandable list):
   - All items from group.items array
   - Show: Quarter (with * if original), Disclosure ID, Username, Status
   - Each item has + (add) and × (remove) buttons
4. Processing History Section (table):
   - Timestamp | Action | Result | Details
5. Action Buttons:
   - [View Queue] [Process Now] [Pause] [Suspend]
   - [Delete Group] [Modify] [Duplicate]

**Data Binding:**
- Get full group object from queue response
- Display all nested data

**Interactions:**
- Process Now → Confirmation → POST /api/queue/.../process
- Suspend → Confirmation → Mark as suspended
- Close modal → Back to list

---

## PART 5: DATA FLOW & STATE MANAGEMENT

**Global State (Zustand or Context):**
```typescript
interface AppState {
  // Queue
  groups: WaitingGroup[];
  selectedGroup: WaitingGroup | null;
  loading: boolean;
  error: string | null;
  
  // Filters
  filterDatabase: string;
  filterStatus: string;
  filterPriority: string;
  
  // Logs
  logs: LogEntry[];
  selectedDate: Date;
  
  // Actions
  fetchGroups: () => Promise<void>;
  processGroup: (group: WaitingGroup) => Promise<void>;
  suspendGroup: (group: WaitingGroup) => Promise<void>;
}
```

**API Calls:**
```typescript
// Fetch all groups
GET /api/queue → Update groups in state

// Fetch status
GET /api/status → Update status cards

// Fetch logs
GET /api/logs/{date}?level={level}&category={category} → Update logs

// Fetch suspended
GET /api/suspended → Update suspended groups

// Process group
POST /api/queue/{db}/{sid}/{tid}/process → Refetch groups

// Retry suspended
POST /api/suspended/{db}/{sid}/{tid}/retry → Refetch suspended
```

---

## PART 6: REAL-TIME FEATURES

**WebSocket for Logs (Future):**
```typescript
const ws = new WebSocket('ws://api/logs/stream');
ws.onmessage = (event) => {
  const logEntry: LogEntry = JSON.parse(event.data);
  addLogEntry(logEntry);
};
```

**Polling (Current):**
- Status: Fetch every 30 seconds
- Logs: Fetch on date change or manual refresh
- Queue: Fetch on process action or page load

---

## PART 7: ERROR HANDLING

**Toast Notifications:**
```typescript
// Success
toast.success('Group processed successfully');

// Error
toast.error('Failed to process group: Connection timeout');

// Loading
toast.loading('Processing group...');
```

**Fallback UI:**
- No groups: "📭 No groups in queue. Create your first group."
- No logs: "📝 No logs found for this date."
- Error: "❌ Failed to load data. Please try again."

---

## PART 8: DEVELOPMENT ROADMAP

**Phase 1: Setup & Layout**
1. Create Vite project
2. Install dependencies
3. Create sidebar + layout
4. Create reusable components (Card, Table, Button, Modal)

**Phase 2: Dashboard Page**
1. Create status cards
2. Create table component for last 5 groups
3. Integrate GET /api/queue
4. Integrate GET /api/status
5. Add refresh functionality

**Phase 3: Queue Page**
1. Create full queue table
2. Add filter controls
3. Add pagination
4. Add search functionality
5. Add Process action

**Phase 4: Suspended Page**
1. Create suspended table
2. Integrate GET /api/suspended
3. Add Manual Retry action
4. Add failure reason color coding

**Phase 5: Logs Page**
1. Create log viewer (dark theme)
2. Add date picker
3. Add level/category filters
4. Add search
5. Add export functionality

**Phase 6: Settings & Polish**
1. Create settings page
2. Add localStorage integration
3. Add theme toggle (light/dark)
4. Error handling & edge cases
5. Performance optimization

---

## PART 9: TYPESCRIPT INTERFACES (ALL)

```typescript
// Queue
interface WaitingItem {
  quarter: number;
  isOriginal: boolean;
  username: string;
  disclosureId: number;
}

interface WaitingGroup {
  databaseName: string;
  securityId: number;
  templateId: number;
  securityCode: string;
  priorityScore: number;
  itemCount: number;
  items: WaitingItem[];
}

interface QueueResponse {
  groups: WaitingGroup[];
}

// Status
interface StatusResponse {
  status: "Running" | "Idle" | "Error";
  queuedCount: number;
  suspendedCount: number;
  lastHeartbeat: string;
  timestamp: string;
}

// Logs
interface LogEntry {
  _id: string;
  timestamp: string;
  level: "Information" | "Warning" | "Error" | "Debug";
  category: "system" | "financial" | "api" | "consistency";
  message: string;
  groupKey: string;
  securityCode: string;
  databaseName: string;
  securityId: number;
  templateId: number;
  quarter?: number;
  exception: string | null;
  metadata: Record<string, any> | null;
}

interface LogsResponse {
  logs: LogEntry[];
  totalCount: number;
  date: string;
}

// Suspended
interface SuspendedGroup {
  databaseName: string;
  securityId: number;
  securityCode: string;
  templateId: number;
  failureCount: number;
  firstFailedUtc: string;
  nextRetryUtc: string;
  failureReason: string;
  retryInProgress: boolean;
}

interface SuspendedResponse {
  groups: SuspendedGroup[];
  totalCount: number;
}

// API Responses
interface ProcessResponse {
  success: boolean;
  message: string;
  groupKey: string;
  timestamp: string;
}

interface RetryResponse {
  success: boolean;
  message: string;
  retryScheduled: boolean;
  nextRetryTime: string;
}
```

---

## PART 10: COMPONENT CHECKLIST

**Layout Components:**
- [ ] Sidebar (with menu items & premium box)
- [ ] Header (with search & notifications)
- [ ] MainLayout (sidebar + content wrapper)

**Reusable Components:**
- [ ] Card (generic white card with shadow)
- [ ] Button (primary, secondary, danger variants)
- [ ] Table (generic table with headers, rows, pagination)
- [ ] Modal (with header, body, footer, close button)
- [ ] Input (text, number, search)
- [ ] Select/Dropdown (for filters)
- [ ] Toggle/Switch
- [ ] Slider
- [ ] DatePicker
- [ ] Toast (notifications)
- [ ] Badge (status, priority colors)
- [ ] Spinner (loading state)

**Page Components:**
- [ ] Dashboard Page
  - [ ] StatusCards
  - [ ] LastProcessedTable
  - [ ] StatusPanel (greeting + controls)

- [ ] Queue Page
  - [ ] QueueFilters
  - [ ] QueueTable
  - [ ] Pagination

- [ ] Suspended Page
  - [ ] SuspendedAlert
  - [ ] SuspendedTable

- [ ] Logs Page
  - [ ] LogsControls
  - [ ] LogViewer

- [ ] Settings Page
  - [ ] WorkerConfig
  - [ ] APIConfig
  - [ ] DatabaseConfig

**Modal Components:**
- [ ] GroupDetailModal
- [ ] ProcessConfirmModal
- [ ] RetryConfirmModal
- [ ] SuspendConfirmModal

---

## PART 11: STYLING APPROACH

**TailwindCSS Usage:**
- Install: `npm install -D tailwindcss postcss autoprefixer`
- Configure tailwind.config.js with custom colors
- Use utility classes for most styling
- Create custom classes in globals.css for repeated patterns

**CSS Variables:**
```css
:root {
  --color-primary: #667eea;
  --color-secondary: #764ba2;
  --color-success: #10b981;
  --color-warning: #f59e0b;
  --color-error: #ef4444;
  
  --shadow-light: 0 2px 8px rgba(0,0,0,0.05);
  --shadow-medium: 0 8px 24px rgba(0,0,0,0.12);
  
  --border-radius-sm: 6px;
  --border-radius-md: 8px;
  --border-radius-lg: 12px;
}
```

---

## PART 12: TESTING CHECKLIST

- [ ] Dashboard loads status correctly
- [ ] Queue table displays all groups
- [ ] Filters work (database, status, priority)
- [ ] Process action triggers confirmation
- [ ] Suspended page shows correct failure reasons
- [ ] Logs display with correct timestamps
- [ ] Settings save to localStorage
- [ ] Real-time updates work (30s polling)
- [ ] Error toasts show on failures
- [ ] Modals open/close correctly
- [ ] Responsive on mobile/tablet

---

## NOTES FOR CLAUDE CODE

**Use frontend-design skill** to ensure:
- Component consistency
- Design token adherence
- CSS var usage
- Responsive breakpoints
- Accessibility (ARIA labels, semantic HTML)
- Performance optimization

**Build incrementally:**
1. Create each component in isolation
2. Test with mock data first
3. Integrate API calls once component structure is solid
4. Add error states after happy path works

**TypeScript Strictness:**
```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true
  }
}
```

**Performance Tips:**
- Use React.memo for table rows
- Lazy load pages with React.lazy()
- Debounce search input (300ms)
- Optimize re-renders with useCallback

---

## API BASE URL

Development: `http://localhost:5059/api`
Production: Update in .env files

---

**START WITH DASHBOARD PAGE - COMPONENT BY COMPONENT**