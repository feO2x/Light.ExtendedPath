---
trigger: glob
description: 
globs: ai-plans/**/*.md
---

- Each AI plan should describe a task in a concice and self-contained way (do not reference other AI plans)
- Each AI plan should describe the context why the change is performed, a list of check marks representing the acceptance criteria, and additional notes.
- The list of acceptance criteria must all be fulfilled for the task to be completed.
- Additional notes indicates how the implementation and tests should be designed. It should guide how things should be done when they cannot be expressed as acceptance criteria.
- Each plan should not only contain instructions for production code, but also for automated tests.