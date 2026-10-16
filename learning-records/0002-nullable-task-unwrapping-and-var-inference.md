# Unwrapping Nullable `Task<T?>` and Reading Through `var` Inference

The learner independently deduced that awaiting a method returning `Task<Conversation?>` yields a nullable `Conversation?`, and that accessing `conversation.Turns.Count` without a null check throws an exception when the conversation ID is not found in SQLite.

## Evidence
- Initially blocked in Lesson 0001 Section 3 because `_repository.GetConversationAsync` was not shown in Section 2 and `var conversation` hid the inferred `Conversation?` type.
- Once prompted to trace what `await` unwraps `Task<Conversation?>` into, immediately identified that a missing `conversationId` makes `conversation` null and causes `conversation.Turns` to throw at runtime.

## Implications
- When introducing C# `var` in future lessons, explicitly show the compiler-inferred type alongside it (especially when nullability `?` is involved).
- The learner prefers guided discovery ("help me get to the answer myself") over being handed solutions directly.
