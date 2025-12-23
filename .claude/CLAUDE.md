Use the project-graph MCP to track and update the project status (tasks, usecases, scenarios, ...).
Use the process-runner MCP to start, debug and kill test processes.
Use playwright MCP to test UIs.
Use Unity-MCP to manage and monitor the Unity project, but edit code as much as possible by directly editing files.
IMPORTANT: When using Find GameObject (Unity-MCP:GameObject_Find) , ALWAYS use 'briefData=true' to avoid crashing the Editor.

When starting, check the project status and list the likely next tasks.

Upon the completion of a task, make sure the project graph entities are up to date, kill the remaining processes, then commit.

## Coding guidelines
 - do not use 'any' unless absolutely necessary
 - use shared types as much as possible, especially in method signatures that are exposed to the outside