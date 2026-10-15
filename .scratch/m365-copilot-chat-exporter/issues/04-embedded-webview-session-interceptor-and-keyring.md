# 04: Embedded Webview Session Interceptor & Secure Keyring Slice

Status: ready-for-agent

**Blocked by:** 01: Foundation & First Tracer Bullet: Ingest-to-Markdown Slice

## What to build
An end-to-end authentication workflow allowing a user to click "Sign In", complete standard corporate Entra ID login (including MFA, SSO, and Conditional Access) within an embedded `Avalonia.WebView`, intercept active Microsoft Substrate session tokens and cookies with zero admin consent, and persist them securely in the platform's native credential store with automatic token expiry detection.

## Acceptance Criteria

- [ ] **Embedded Login Experience**:
  - Clicking "Sign In" opens an `Avalonia.WebView` window pointing to `https://m365.cloud.microsoft/chat`.
  - Supports standard enterprise authentication flows including MFA prompts, tenant redirects, and SSO.
- [ ] **Session Token Interception**:
  - Intercepts outgoing requests to `substrate.office.com/m365Copilot/` and extracts the Bearer `Authorization` token and authenticated session cookies.
  - Automatically signals login completion and closes/hides the webview once credentials are captured.
- [ ] **Secure Native Storage**:
  - Implements `ISecureStorage` storing encrypted session credentials in platform vaults:
    - Windows Credential Locker on Windows
    - Apple Keychain on macOS and iOS
    - Secret Service / libsecret on Linux
    - Android KeyStore on Android
- [ ] **Session Expiry & Lifecycle**:
  - Stored credentials are automatically read on app launch to attempt session restoration.
  - Detection of HTTP 401 Unauthorized responses marks session as expired and prompts re-authentication.
  - "Sign Out" action wipes stored credentials from the native secure store.
- [ ] **Automated Seam Verification**:
  - Unit tests verify credential serialization, token extraction parsing, and secure storage repository operations using test mock vaults.

## Verification
Run `dotnet test --filter "FullyQualifiedName~SecureStorageTests"` and `dotnet test --filter "FullyQualifiedName~SessionInterceptorTests"`.
