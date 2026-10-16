# Teaching Notes

- **Learner Profile**: Experienced software developer new to C# and .NET 10.
- **Primary Goal**: Read, review, and modify the `m365-copilot-chat-exporter-app` codebase as AI agents build it; trace from xUnit seam tests down to SQLite and file exporters; spot subtle bugs and architectural smells in agent diffs.
- **Pedagogical Style**:
  - Ground every lesson and quiz in actual code from `src/CopilotChatExporter.Core` and `tests/CopilotChatExporter.Core.Tests`.
  - Keep explanations concise by mapping C# idioms to TypeScript/Python/Java equivalents.
  - Emphasize code-review sharpness (finding subtle bugs in diffs) alongside reading/writing fluency.
  - **Socratic Guidance**: When the learner is stuck on an exercise, guide them with targeted questions so they reach the "aha!" moment themselves rather than spoiling the answer.
  - **Bridge `var` and Collaborator Signatures**: Because C#'s `var` hides inferred types (including `?` nullability), always make collaborator return types explicit in the reading section before asking the learner to spot a bug in a diff.
  - **Never Test Untaught Material**: Every concept, term, or syntax construct tested in a lesson's interactive exercise or quiz MUST be explicitly introduced in that same lesson's prose first (never hidden only in a separate reference sheet).
  - **Concrete Compiler Rewrites Over Jargon**: Explain C# syntax sugar by showing the simple code the compiler rewrites it into (e.g., `await using` → `try / finally`) rather than abstract phrases ("scoped resource cleanup statement").
- **Next Session**:
  - The learner completed [Lesson 0001](lessons/0001-reading-the-domain-and-seam.html) and its quiz, and has remaining questions about some parts of Lesson 0001. Start the next `/teach` session by unpacking those questions before authoring Lesson 0002.
