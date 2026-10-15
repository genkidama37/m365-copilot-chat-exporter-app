# 08: Mobile Export Package & Native Share Sheet Delivery Slice

Status: ready-for-agent

**Blocked by:** 07: Incremental Export & Desktop Native Folder Delivery Slice

## What to build
An end-to-end mobile export workflow on iOS and Android that compiles the selected Export Bundle into an ephemeral sandbox cache directory, compresses the files into a single `.zip` archive, and invokes the native OS Share Sheet so the user can easily save to Apple Files, iCloud, Google Drive, or send via email.

## Acceptance Criteria

- [ ] **Sandbox Export Bundle Creation**:
  - Generates the selected export files within the mobile app's temporary cache directory.
- [ ] **Zip Archive Packaging**:
  - Compresses the generated bundle into a standard `.zip` archive named `Copilot_Export_{YYYYMMDD_HHMMSS}.zip`.
  - Preserves internal directory structure (`assets/`, `index.html`, etc.) within the archive.
- [ ] **Native OS Share Sheet Invocation**:
  - Dispatches the `.zip` file via platform share sheets (iOS `UIActivityViewController`, Android `ACTION_SEND` Intent).
  - Permits saving to Apple Files, iCloud, Google Drive, OneDrive, or sending via Mail/AirDrop.
- [ ] **Resource Cleanup**:
  - Deletes temporary uncompressed files and zip archives upon share sheet dismissal or completion.
- [ ] **Database State Consistency**:
  - Updates `LastExportedAt` timestamps in SQLite upon successful export dispatch.
- [ ] **Automated Seam Verification**:
  - Tests verify zip archive creation, entry integrity, compression verification, and temporary file cleanup.

## Verification
Run `dotnet test --filter "FullyQualifiedName~MobileShareDeliveryTests"`.
