# Understanding `await using` as a Compiler-Generated `try` / `finally`

The learner articulated `await using var x = ...` in their own words: a compact way to tell the C# compiler to wrap the remainder of the variable's block in a `try { ... } finally { await x.DisposeAsync(); }` so the resource is cleaned up no matter how the function exits.

## Evidence
- Asked to unpack the abstract phrase "scoped resource cleanup statement" in Lesson 0001.
- After tracing what happens when an assertion throws mid-function without `try`/`finally`, immediately synthesized that `await using` instructs the compiler to generate the `try`/`finally` cleanup automatically.

## Implications
- In `SqliteConversationRepository.cs` (Lesson 0002), the learner already has the exact mental model needed for `await using var command`, `await using var transaction`, and `await using var reader`.
- Prefer showing what the C# compiler lowers syntax sugar into (e.g. `try`/`finally`) over abstract terminology ("scoped resource cleanup statement").
