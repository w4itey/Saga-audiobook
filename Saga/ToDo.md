# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Saga is a .NET MAUI cross-platform audiobook app that integrates with Audiobookshelf servers. Key features include multi-user authentication, SSO integration, library browsing, and planned Android Auto/Apple CarPlay support.

## Build and Development Commands

```bash
# Build the project
dotnet build

# Clean build artifacts
dotnet clean

# Restore packages
dotnet restore

# Build for specific platform
dotnet build -f net8.0-android
dotnet build -f net8.0-ios
```

## Architecture Overview

### Authentication Flow
The app uses a multi-stage authentication system:

1. **Server Discovery** (`ServerDiscoveryPage`) - Users enter their Audiobookshelf server URL
2. **Capability Detection** - App calls `/api/status` to discover available auth methods
3. **Login Method Selection** (`LoginPage`) - Dynamic UI based on server capabilities
   - Local authentication (username/password)
   - SSO via OpenID Connect through Audiobookshelf
4. **Token Management** - Secure storage via `AuthenticationService`

### Key Services

**`IAuthenticationService`** - Core authentication abstraction supporting:
- Multi-user sessions with secure token storage
- Local and SSO authentication flows
- User profile management and switching
- PKCE-based OAuth2 flows for mobile security

**`AudiobookshelfApiClient`** - HTTP client for Audiobookshelf API:
- Server capability discovery (`GetServerInfoAsync`)
- Authentication endpoints (local and OAuth2)
- Library and media item retrieval
- Token exchange for OAuth flows

### Multi-User Architecture
The app supports multiple concurrent users via:
- `MultiUserSession` - Tracks current and available users
- `UserProfile` - Individual user data with server associations
- Secure per-user token storage using `SecureStorage`
- Event-based user switching notifications

### Navigation Flow
```
App.xaml.cs -> ServerDiscoveryPage -> LoginPage -> MainPage
      |                |                  |           |
   Auth Check    Server Discovery   Method Selection  Libraries
```

### OAuth2/SSO Implementation
- Uses PKCE (Proof Key for Code Exchange) for mobile security
- Integrates with Audiobookshelf's OpenID Connect endpoints
- Custom URI scheme `saga://auth/callback` for callback handling
- Dynamic SSO button generation based on server configuration

### Data Models Structure
- **Authentication**: `AuthenticationResult`, `UserProfile`, `MultiUserSession`
- **Server**: `ServerInfo`, `AuthMethod` (for SSO provider discovery)
- **Media**: `Library`, `LibraryItem` (with metadata support)
- **API**: Request/Response models for Audiobookshelf integration

### Platform-Specific Configuration
**Android**: `AndroidManifest.xml` configured with OAuth2 callback intent filters
**iOS**: Standard MAUI configuration for WebAuthenticator support

## Current Development Status
Refer to `DEVELOPMENT_PLAN.md` for detailed roadmap. Phase 1 (authentication and multi-user) is largely complete. Future phases include social features, vehicle integration, and e-book support.

## Key Dependencies
- **Microsoft.Maui.Controls** - UI framework
- **Microsoft.Identity.Client** - OAuth2/MSAL support  
- **System.IdentityModel.Tokens.Jwt** - JWT token handling
- **Microsoft.Maui.Essentials** - Platform APIs (SecureStorage, WebAuthenticator)

## Important Notes
- All authentication tokens stored via `SecureStorage` for security
- Server URLs must include protocol (auto-prefixed with https:// if missing)
- Dynamic UI generation based on server-reported authentication capabilities
- Multi-user support requires careful session state management between users

## Commiting to Git
- Dont take claude accrediitation