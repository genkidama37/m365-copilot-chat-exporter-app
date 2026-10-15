# Embedded Webview Session for Copilot Data Extraction

We extract Microsoft 365 Copilot chat conversations by authenticating the user via an embedded webview session rather than the official Microsoft Graph Copilot Export API. Microsoft Graph's export endpoint requires tenant-wide application permissions (`AiEnterpriseInteraction.Read.All`) that ordinary enterprise knowledge workers cannot obtain without IT admin consent, whereas an embedded webview delegates authentication directly to the user's active session against Microsoft Substrate endpoints with zero admin privileges required.

## Status

Accepted

## Considered Options

- **Microsoft Graph Export API**: Blocked for typical enterprise end-users due to mandatory tenant-level administrator consent and application-only permissions.
- **Browser Extension**: Avoids a standalone app binary, but limits multiplatform mobile availability and file system management.
- **Embedded Webview Session Extraction**: Works out of the box across platforms for any licensed M365 Copilot user supporting SSO, MFA, and Conditional Access policies.
