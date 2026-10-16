namespace CopilotChatExporter.Core.Models;

/// <summary>
/// A reference link attached to a response pointing to an enterprise document (SharePoint, OneDrive)
/// or external web page used for grounding.
/// </summary>
public sealed record Citation(
    string Id,
    string Title,
    string Url);
