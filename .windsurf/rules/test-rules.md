---
trigger: glob
description: 
globs: tests/**/*.cs
---

- Use xunit3
- Use FluentAssertions
- Try to combine all tests for a certain path into data-driven tests (`Theory`) instead of writing several facts for the same path. For example, use a data-driven test method for the happy path, and one for each exception that should be provoked.
- If a path only has one value, then write a fact instead of a theory. For example, a `NullReferenceException` is only triggered by a parameter being `null` -> Fact instead of Theory.
- The root namespace for the test project is set to `Light.ExtendedPath` - please reflect that when declaring the file-scoped namespace.