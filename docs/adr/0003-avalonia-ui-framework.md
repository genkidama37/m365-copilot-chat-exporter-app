# Avalonia UI for Five-Platform Native User Interface

We choose Avalonia UI as the cross-platform application framework over .NET MAUI and Uno Platform. While Microsoft's .NET MAUI supports Windows, macOS, iOS, and Android, it lacks official first-party Linux support. Avalonia UI provides first-class native rendering, rich desktop controls, and production-ready support across all five required platforms: Windows, macOS, Linux, iOS, and Android.

## Status

Accepted

## Considered Options

- **.NET MAUI**: Discarded due to lack of official Linux desktop support.
- **Uno Platform**: Viable alternative, but Avalonia has broader community adoption and mature desktop Linux rendering via Skia/X11/Wayland.
- **Avalonia UI**: Chosen for unified XAML dialect, deep multiplatform support, and native WebView bindings across all five targets.
