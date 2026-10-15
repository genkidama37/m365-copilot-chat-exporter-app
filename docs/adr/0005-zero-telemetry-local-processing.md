# Zero Telemetry and 100% Local-Only Data Processing

The application enforces a strict zero-telemetry, zero-remote-server architecture. All network communication is strictly confined to official Microsoft endpoints required for authentication and conversation retrieval (`login.microsoftonline.com`, `substrate.office.com`, `m365.cloud.microsoft`).

## Status

Accepted

## Context and Decision

Microsoft 365 Copilot conversations in enterprise environments contain sensitive business communications, intellectual property, and confidential data. Third-party telemetry, crash-reporting SDKs (e.g., Sentry, AppCenter), or intermediary backend proxies introduce unacceptable compliance and security risks for enterprise knowledge workers. 

All database caching (SQLite) and file generation (Markdown, HTML, JSON) are executed entirely on the local device with no outbound telemetry.
