# AGENTS.md - Automation and AI Agent Guidelines

This document defines the standards, expectations, and constraints for automated agents (AI assistants, CI/CD bots, scripting tools, and automation frameworks) interacting with this repository.

## 1. Project Overview and Purpose

The **Pessoas-Integracao (PIIP)** project is a specialized data staging layer designed to synchronize personnel information from **SIGDN-RHV** with the operational reality of the **Portuguese Navy**.

Its primary purpose is to act as a reliable intermediary data store (Staging Area) that consumes all personnel-related data from the source system—expected to synchronize every x (not define...) hours—and serves as the authoritative source of truth for all future applications developed by the Navy.

**Core Objectives:**

- **Data Decoupling:** Ensure that future Navy applications do not depend directly on the external SIGDN RHV system.
- **Reliable Synchronization:** Maintain an up-to-date staging environment reflecting the current personnel data.
- **Foundation for Ecosystem:** Provide a stable and standardized data source for any subsequent software development within the organization.

## 2. Architecture and Design Conventions

- **Clean Architecture**: The project follows Clean Architecture principles to ensure a strict separation of concerns:
  - **Core (`Pessoas.Integracao.Core`)**: The central hub containing domain entities, business logic, and application interfaces. It must have no dependencies on external infrastructure.
  - **Infrastructure/Presentation (`Pessoas.Integracao.Worker`, `.Admin`, `.Consulta`, `.Analitica`)**: These projects handle external concerns such as SOAP clients, database persistence, and API endpoints. They depend on the Core layer.
- **Dependency Injection**: Always use constructor injection. Avoid the Service Locator pattern.
- **Naming Conventions**: Follow standard .NET C# coding conventions (PascalCase for classes/methods, camelCase for private fields).

## 3. Rules for Modifying Files

- **Safe Zones**: Agents are encouraged to suggest improvements in `src/` and `tests/`.
- **Restricted Zones**: Do NOT modify `.github/workflows/`, `global.json`, or `Directory.Packages.props` without explicit confirmation, as these affect the entire build pipeline and dependency management.
- **File Creation**: New files must be placed in the appropriate directory according to the project structure.
- **Consistency**: Maintain the existing indentation and formatting defined in `.editorconfig` and `.prettierrc`.

## 4. Testing and Validation Requirements

- **Test-Driven Approach**: Any logic change MUST be accompanied by a corresponding test case in `tests/`.
- **Validation**: Agents should verify that new code does not break existing functionality. If the agent has access to a terminal, it should attempt to run tests before finalizing suggestions.
- **Edge Cases**: Ensure that null checks, exception handling, and logging are implemented for all new public endpoints or service methods.

## 5. CI/CD Expectations

- **Pipeline Integrity**: Automation must not bypass CI checks.
- **Build Success**: All proposed changes must result in a successful build of the `.slnx` solution.
- **Version Control**: Respect the versioning strategy defined in `.releaserc`.

## 6. Security Considerations

- **Secrets**: NEVER commit API keys, connection strings, or credentials. Use `appsettings.json` placeholders or environment variables.
- **Input Validation**: All external inputs must be validated to prevent injection attacks.
- **Dependency Updates**: When suggesting dependency updates (e.g., via Renovate), ensure the updated package is compatible with the current .NET target framework.

## 7. Guidelines for Commit Messages and PRs

- **Format**: Use conventional commits (e.g., `feat:`, `fix:`, `chore:`, `docs:`, `test:`).
- **Clarity**: Commit messages should clearly describe *what* was changed and *why*.
- **PR Descriptions**: Automated PRs should include a summary of changes and a checklist confirming that tests were run.

---

*This file complements README.md and CONTRIBUTING.md, focusing specifically on machine-readable and automation-friendly guidance.*
