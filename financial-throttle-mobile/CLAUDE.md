# FinancialThrottle Mobile

React Native mobile app (Expo) for the FinancialThrottle service.
Three screens: Dashboard, Suspended, Checker.

## Stack
- Expo (blank TypeScript template)
- React Navigation — bottom tab navigator
- Axios — API calls (same backend as web frontend)

## Color Palette
- Background: #F0F0F0
- Border: #D6D6D6
- Success / Queued: #DCFCE7
- Waiting: #FFEDD5
- Failure / Suspended: #FEE2E2
- Action (primary buttons, active tab): #1A1535
- Info: #DBEAFE
- Text primary: #111827
- Text secondary: #6B7280

## API
Base URL: http://<LOCAL_IP>:5059/api
Same REST endpoints as the web frontend.
No auth required.

Endpoints used:
- GET /status → Dashboard stats
- GET /queue → Dashboard last 5 groups
- GET /suspended → Suspended page list + retry/force send
- POST /suspended/{db}/{securityId}/{templateId}/retry → Manual retry
- POST /suspended/{db}/{securityId}/{templateId}/force-send → Force send
- GET /checker?databaseName=&securityId=&templateId=&quarter=&itemQuarterlyCode= → Checker results

## Project Structure
mobile/
  App.tsx              ← NavigationContainer + Tab.Navigator
  screens/
    DashboardScreen.tsx
    SuspendedScreen.tsx
    CheckerScreen.tsx
  constants/
    colors.ts          ← color palette constants
    api.ts             ← axios instance with baseURL
  types/
    index.ts           ← shared types (copy from web frontend types.ts)
  services/
    endpoints.ts       ← API call functions

## Screens

### Dashboard
- 4 stat cards in 2x2 grid: Running status, Pending Groups, Suspended count, Last Heartbeat
- Last 5 queue groups list with status badges (Queued / Waiting / Suspended)
- Pull to refresh

### Suspended
- List of suspended groups
- Each card shows: database, security code, template, attempts badge, first error time, next retry time, failure reason
- Two action buttons per card: Manual Retry (info color) and Force Send (failure color)
- Force Send shows a confirmation alert before calling API
- Pull to refresh

### Checker
- Form: Database Name, Security ID, Template ID, Quarter (required), Item Quarterly Code (optional)
- Run button (action color)
- Results: summary chips (total, processed, not found)
- Filter chips: All, Processed, Not Found
- Search input filters by item quarterly code or original definition
- Results list: item quarterly code, original definition, in quarterly (✓/✗), in production (✓/✗), status badge

## Navigation
Bottom tab bar with 3 tabs:
- Dashboard → home icon
- Suspended → alert icon  
- Checker → search icon

Active tab color: #1A1535
Inactive tab color: #9CA3AF
Tab bar background: #FFFFFF
Tab bar border top: #D6D6D6

## Notes
- Push notifications: not implemented yet
- No authentication required
- Pull to refresh on all screens
- Show loading spinner while fetching
- Show error state if API unreachable
