# Saga Audiobook App - Complete Development Plan

## Project Overview
Saga is a comprehensive audiobook app that connects to Audiobookshelf servers with advanced features including Android Auto/Apple CarPlay integration, social features, multi-user support, and Kindle integration.

## Current Status (End of Day 1)
✅ **Completed:**
- Basic app structure and MAUI setup
- Fixed app startup and navigation issues  
- Login functionality with Audiobookshelf API
- Library display and book browsing
- Started OAuth2/SSO integration with Audiobookshelf's built-in SSO

🔄 **In Progress:**
- Authentication service enhancement
- OAuth2 PKCE flow implementation

## Phase 1: Enhanced Authentication & Multi-User Support (Priority: High)
**Status: IN PROGRESS**

### 1.1 SSO Integration with Audiobookshelf
- ✅ Research Audiobookshelf OAuth2 endpoints
- ✅ Implement PKCE flow for mobile security
- ✅ Add OAuth2 methods to AudiobookshelfApiClient
- ⏳ Create SSO login UI component
- ⏳ Handle OAuth callback and token exchange
- ⏳ Test with various SSO providers

### 1.2 Multi-User Support
- ✅ Create user profile management system (AuthenticationService implemented)
- ⏳ Implement user switching functionality
- ✅ Add secure token storage per user
- ⏳ Create user-specific libraries and progress tracking
- ⏳ Add family account and parental controls

### 1.3 Secure Token Management
- ✅ Implement foundation for token management (basic storage implemented)
- ⏳ Add token refresh mechanism when Audiobookshelf supports it
- ✅ Add encrypted storage for sensitive data (SecureStorage)
- ⏳ Handle token expiration gracefully
- ✅ Add session management across app lifecycle (MultiUserSession)

## Phase 2: Social Features & Friends System (Priority: High)

### 2.1 Friends System Architecture
- ⏳ Design friends database schema
- ⏳ Create friend request/accept/decline system
- ⏳ Add user search and discovery
- ⏳ Implement privacy controls

### 2.2 Reading Status Sync
- ⏳ Real-time progress sharing with WebSocket/SignalR
- ⏳ Activity feed for friends' reading progress
- ⏳ Reading challenges and achievements
- ⏳ Book recommendations based on friends' activity

### 2.3 Social Features UI
- ⏳ Friends list and management
- ⏳ Activity feed and notifications
- ⏳ Reading streaks and leaderboards
- ⏳ Book clubs and group reading

## Phase 3: Vehicle Integration (Priority: High)

### 3.1 Android Auto Integration
- ⏳ Add Android Auto dependencies and manifest config
- ⏳ Implement MediaBrowserService for car compatibility
- ⏳ Create hierarchical content structure (Libraries → Books → Chapters)
- ⏳ Add voice command support
- ⏳ Design car-safe UI with large touch targets
- ⏳ Test with Android Auto simulator and real vehicles

### 3.2 Apple CarPlay Integration  
- ⏳ Configure CarPlay entitlements and Info.plist
- ⏳ Implement MPPlayableContentManager
- ⏳ Add "Hey Siri" voice control integration
- ⏳ Create CarPlay-specific navigation patterns
- ⏳ Test with CarPlay simulator and real vehicles

### 3.3 Audio Playback Foundation
- ⏳ Integrate MediaManager or native audio APIs
- ⏳ Implement background audio service
- ⏳ Add MediaSession (Android) and MPNowPlayingInfoCenter (iOS)
- ⏳ Handle audio focus and interruptions
- ⏳ Add lock screen and notification controls

## Phase 4: E-book & Kindle Integration (Priority: Medium)

### 4.1 Kindle Integration Research & Implementation
- ⏳ Research Amazon Personal Document Service API
- ⏳ Implement email-to-Kindle functionality
- ⏳ Add EPUB to Kindle format conversion (Calibre integration)
- ⏳ Sync reading positions between audio and e-book versions

### 4.2 Enhanced E-book Features
- ⏳ Dual-mode reading (audio + visual text sync)
- ⏳ Highlight and note sharing between formats
- ⏳ Reading speed adjustment to match narration
- ⏳ Offline e-book downloading and management

## Phase 5: Advanced Playback & User Experience (Priority: Medium)

### 5.1 Enhanced Audio Features
- ⏳ Chapter navigation and intelligent bookmarking
- ⏳ Sleep timer, playback speed controls, equalizer
- ⏳ Audio quality optimization for different networks
- ⏳ Smart chapter summaries using AI

### 5.2 UI/UX Improvements
- ⏳ Remove debug alerts and add proper loading states
- ⏳ Add book covers and thumbnails
- ⏳ Implement search and filtering for large libraries
- ⏳ Add onboarding flow for new users
- ⏳ Create settings page for preferences

## Phase 6: Platform Optimization & Advanced Features (Priority: Low)

### 6.1 Performance & Reliability
- ⏳ Performance tuning for older devices and vehicles
- ⏳ Adaptive UI for different screen sizes
- ⏳ Network resilience and intelligent caching
- ⏳ Battery optimization for long listening sessions

### 6.2 Community & Social Features
- ⏳ Book clubs and group reading sessions
- ⏳ Discussion threads and reviews system
- ⏳ Integration with Goodreads and other platforms
- ⏳ Reading habit analytics and insights

## Technical Architecture Notes

### Authentication Flow
1. **Traditional Login**: Username/password → Audiobookshelf API
2. **SSO Login**: OAuth2 PKCE flow → Audiobookshelf SSO endpoints
3. **Multi-User**: Secure token storage per user with switching capability

### Key Technologies
- **.NET MAUI 8.0**: Cross-platform mobile framework
- **Audiobookshelf API**: Backend server and media management
- **OAuth2 with PKCE**: Secure mobile authentication
- **SecureStorage**: Encrypted token and sensitive data storage
- **MediaManager/Native Audio**: Cross-platform audio playback
- **SignalR/WebSockets**: Real-time social features
- **Android Auto MediaBrowserService**: Vehicle integration
- **Apple CarPlay MPPlayableContentManager**: Vehicle integration

### Current Files Structure
```
Saga/
├── Models/
│   ├── AuthenticationModels.cs (✅ Created - Full auth models)
│   ├── Library.cs (✅ Existing)
│   ├── LibraryItem.cs (✅ Created)
│   ├── LoginRequest.cs (✅ Existing)
│   ├── LoginResponse.cs (✅ Existing)
│   └── User.cs (✅ Existing)
├── Services/
│   ├── AudiobookshelfApiClient.cs (✅ Enhanced with OAuth2 PKCE)
│   ├── AuthenticationService.cs (✅ Created - Full multi-user support)
│   └── IAuthenticationService.cs (✅ Created)
├── Views/
│   ├── LoginPage.xaml/.cs (✅ Working)
│   └── MainPage.xaml/.cs (✅ Working)
├── MauiProgram.cs (✅ Updated with DI registration)
└── DEVELOPMENT_PLAN.md (✅ This file)
```

## Next Steps for Tomorrow
1. **Create SSO login UI** - Add buttons for different SSO providers and browser integration
2. **Test OAuth2 flow end-to-end** - Implement browser-based authentication flow
3. **Implement user switching UI** - Multi-user switching functionality in the app
4. **Begin audio playback foundation** - MediaManager integration for vehicle compatibility
5. **Start Android Auto integration** - MediaBrowserService implementation

## Key APIs and Endpoints
- **Audiobookshelf Login**: `POST /login`
- **Audiobookshelf Libraries**: `GET /api/libraries`  
- **Audiobookshelf Library Items**: `GET /api/libraries/{id}/items`
- **Audiobookshelf OAuth Start**: `GET /auth/openid`
- **Audiobookshelf OAuth Callback**: `POST /auth/openid/callback`
- **Audiobookshelf Mobile Redirect**: `/auth/openid/mobile-redirect`

## Notes
- Audiobookshelf has built-in SSO support, so we leverage their OAuth2 endpoints
- PKCE (Proof Key for Code Exchange) is required for mobile OAuth2 security
- Multi-user support is essential for family accounts
- Vehicle integration (Auto/CarPlay) requires specific audio service architecture
- Friends feature will need server-side component or integration with Audiobookshelf social features

---
*Plan created: [Date]*  
*Last updated: [Date]*