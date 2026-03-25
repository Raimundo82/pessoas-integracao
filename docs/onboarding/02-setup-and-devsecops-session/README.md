# Module 02 — Project Setup & DevSecOps Pipeline

**Format:** Demo → You try it  
**Duration:** ~3 hours  
**Prerequisites:** Module 01 (Git session) completed, Gitea access confirmed

---

## What this module covers

| #   | Topic                                                           | Format     | Time   |
| --- | --------------------------------------------------------------- | ---------- | ------ |
| 1   | [Project Setup — Docker & Dev Container](./01-project-setup.md) | Demo → Try | 40 min |
| 2   | [Git Hooks — Lefthook](./02-git-hooks.md)                       | Demo → Try | 25 min |
| 3   | [The Linter — dotnet format & Prettier](./03-linter.md)         | Demo → Try | 20 min |
| 4   | [CI Phases — ci-build-test](./04-ci-phases.md)                  | Demo → Try | 25 min |
| 5   | [SonarQube Scan](./05-sonarqube.md)                             | Demo → Try | 25 min |
| –   | Wrap-up & Q&A                                                   | Discussion | 10 min |

---

## The big picture

The tooling in this project forms two rings of quality protection:

```
Your machine (before pushing)          Gitea pipeline (after pushing)
─────────────────────────────          ──────────────────────────────
Git hooks (Lefthook)                   branch-name-lint
  → pre-commit: format staged files    pull-request-lint
                                       ci-build-test
EditorConfig + Prettier + dotnet         → format check
format keep everything consistent        → build
                                         → vulnerability scan
SonarLint in VS Code gives you           → tests
inline feedback as you type            ci-pr-scan
                                         → SonarQube analysis
                                         → Quality Gate
```

The local ring catches issues before they ever reach CI. The pipeline is the final safety net.
