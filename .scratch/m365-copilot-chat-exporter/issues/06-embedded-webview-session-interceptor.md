# Embedded Webview Session Interceptor

Status: ready-for-agent

## Description

Implement the authentication webview in `CopilotChatExporter.Infrastructure` using `Avalonia.WebView` to facilitate user sign-in and intercept Substrate session tokens without IT admin consent.

## Scope of Work

1. Create authentication window using `Avalonia.WebView` navigating to `https://m365.cloud.microsoft/chat`.
2. Intercept outgoing HTTP navigation and network requests:
   - Monitor requests targeting `https://substrate.office.com/m365Copilot/` or Sydney endpoints.
   - Extract the Bearer `Authorization` header token and authenticated cookies.
3. Notify the host application upon successful session capture, passing extracted credentials (`SessionToken`, `Cookies`, `Expiration`).
4. Automatically close or hide the login window once authentication succeeds.
5. Provide manual re-authentication trigger when session expires.

## Acceptance Criteria

- Authenticates users successfully through enterprise Microsoft Entra ID (including MFA, SSO, and Conditional Access).
- Successfully captures valid session tokens and cookies.
- Operates entirely client-side without sending tokens to any third-party service.

## Comments
