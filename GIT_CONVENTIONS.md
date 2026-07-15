# Git Commit Conventions

This document defines the standards for creating commits in the Pessoas-Integracao project. The goal is to maintain a clean, readable, and granular history that facilitates auditing and automated changelog generation.

## 1. Conventional Commits Format

All commit messages must follow the Conventional Commits specification:
`<typetype>(<scopescope>): <descriptiondescription>`

### Types
- `feat`: A new feature.
- `fix`: A bug fix.
- `test`: Adding or correcting tests.
- `refactor`: A code change that neither fixes a bug nor adds a feature.
- `chore`: Maintenance tasks, dependency updates, or configuration changes.
- `docs`: Documentation only changes.
- `style`: Formatting, missing semi-colons, etc; no code change.
- `perf`: A code change that improves performance.

### Scope
The scope should be the module or layer affected (e.g., `sync`, `core`, `admin`, `worker`, `devcontainer`).

### Description
- Use the imperative, present tense: "change" not "changed" nor "changes".
- Do not capitalize the first letter.
- No dot (.) at the end.
- Must be a concise one-liner.

## 2. Granularity Rules

Commits must be as granular as possible. A single commit should represent one logical unit of work.

- **Separate Logic from Tests**: If a feature requires a new test, create two commits: one for the implementation (`feat`) and one for the test (`test`).
- **Separate Fixes from Refactors**: Do not mix a bug fix with a general cleanup of the file.
- **Atomic Changes**: If a change affects multiple files but for the same logical reason, they belong in one commit. If they are for different reasons, split them.

## 3. Workflow for AI Agents

When proposing commits, the agent must:
1. Analyze `git status` and `git diff`.
2. Group changes by logical intent.
3. Propose a sequence of commits.
4. **Wait for human approval** before executing `git add` and `git commit`.
