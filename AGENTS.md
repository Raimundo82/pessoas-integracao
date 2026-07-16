# AGENTS.md - Automation and AI Agent Guidelines

This document defines the standards, expectations, and constraints for automated agents (AI assistants, CI/CD bots, scripting tools, and automation frameworks) interacting with this repository.

## 1. Project Overview and Purpose

The **Pessoas-Integracao (PIIP)** project is a specialized data staging layer designed to synchronize personnel information from **SIGDN-RHV** with the operational reality of the **Portuguese Navy**.

Its primary purpose is to act as a reliable intermediary data store (Staging Area) that consumes all personnel-related data from the source system—expected to synchronize at scheduled intervals... - and serves as the authoritative source of truth for all future applications developed by the Navy.

**Core Objectives:**

- **Data Decoupling:** Ensure that future Navy applications do not depend directly on the external SIGDN RHV system.
- **Reliable Synchronization:** Maintain an up-to-date staging environment reflecting the current personnel data.
- **Foundation for Ecosystem:** Provide a stable and standardized data source for any subsequent software development within the organization.

## 2. Context Acquisition & Documentation

Before proposing any code changes or implementing new features, agents **MUST** consume the following documentation to ensure alignment with the current system design:

- **General Guidelines**: Read `README.md` and `CONTRIBUTING.md` for the overall project vision and contribution rules.
- **Deep Domain Knowledge**: Explore the `/docs` directory at the root of the project.
- **Technical Architecture**: When the task involves development or code generation, it is **mandatory** to analyze:

  - `/docs/domain`: For detailed business rules and domain logic.
  - `/docs/sequence-diagrams`: To understand the operational flow. These PlantUML diagrams represent the **current actual structure** of the project and must be followed strictly.

**Instruction**: If a contradiction is found between the existing code and the documentation in `/docs`, the agent should flag this in the PR description instead of assuming the code is the only source of truth.

## 3. Architecture and Design Conventions

The project is built on **Clean Architecture** principles. The fundamental rule is that **dependencies flow inwards**: outer layers (Infrastructure/Presentation) can depend on the Core, but the Core must never depend on any outer layer.

### 3.1 Layer Responsibilities & Anatomy

**`Pessoas.Integracao.Core` (The Core)**
This project is the heart of the system and is divided into three main areas:

- **Domain**: The purest layer.
  - `Entities`: Core business objects (e.g., `Pessoa.cs`).
  - `Value Objects`: Immutable attributes (e.g., `DadosPessoais.cs`).
  - `Enums`: Domain-specific enumerations.
- **Application**: Orchestration and contracts.
  - `Contracts` & `Abstractions`: All interfaces that define how the system should behave (e.g., `IPessoasRepository.cs`, `IUnitOfWork.cs`).
  - `UseCases`: The actual business logic implementation (e.g., `ImportPessoas.cs`).
  - `DTOs` & `Models`: Data structures for transferring information between layers.
- **Infrastructure (Internal)**: Core-level technical implementations.
  - `Repositories`: Concrete data access logic (e.g., `PessoaRepository.cs`).
  - `Data`: Database context and initialization (`AppDbContext.cs`).

  **Outer Layers (Worker, Admin, Consulta, Analitica)**

- These projects handle external triggers, SOAP integrations, and API endpoints, depending entirely on the `Core` project.

### 3.2 Project Mapping Matrix

| Project | Role | Key Contents |
|---|---|---|
| `Pessoas.Integracao.Core` | **Domain + Application** | Entities, Value Objects, Use Cases, Contracts, DTOs, Internal Repositories. |
| `Pessoas.Integracao.Worker` | **Infrastructure + Background** | SOAP Integrations (SIGDN-RH), Quartz Jobs, Data Sync Logic. |
| `Pessoas.Integracao.Sync` | **Data Synchronization** | Raw data fetching from external systems, persistence in external schema, daily cron jobs, providing raw data to Core and Analitica. |
| `Pessoas.Integracao.Admin` | **Presentation (REST API)** | Admin Controllers, Import endpoints, Management logic. |
| `Pessoas.Integracao.Consulta`| **Presentation (Frontend/API)**| Read-only endpoints and User Interface logic. |
| `Pessoas.Integracao.Analitica`| **Infrastructure (Analytics)** | EF-scaffolded Models, Analytical Views, Reporting logic. |
| `Pessoas.Integracao.Tests` | **Quality Assurance** | Unit + Integration tests for Core and Admin. |
| `Pessoas.Integracao.Worker.Tests` | **Quality Assurance** | Tests for SOAP clients, Translators, and Providers. |
  
## 4. Rules for Modifying Files

- **Safe Zones**: Agents are encouraged to suggest improvements in `src/` and `tests/`.
- **Restricted Zones**: Do NOT modify `.gitea/workflows/`, `global.json`, or `Directory.Packages.props` without explicit confirmation, as these affect the entire build pipeline and dependency management.
- **File Creation**: New files must be placed in the appropriate directory according to the project structure.
- **Consistency**: Maintain the existing indentation and formatting defined in `.editorconfig` and `.prettierrc`.

## 5. Testing and Validation Requirements

- **Test-Driven Approach**: Any logic change MUST be accompanied by a corresponding test case in `tests/`.
- **Validation**: Agents should verify that new code does not break existing functionality. If the agent has access to a terminal, it should attempt to run tests before finalizing suggestions.
- **Edge Cases**: Ensure that null checks, exception handling, and logging are implemented for all new public endpoints or service methods.

## 6. CI/CD Expectations

- **Pipeline Integrity**: Automation must not bypass CI checks.
- **Build Success**: All proposed changes must result in a successful build of the `.slnx` solution.
- **Version Control**: Respect the versioning strategy defined in `.releaserc`.

## 7. Security Considerations

- **Secrets**: NEVER commit API keys, connection strings, or credentials. Use `appsettings.json` placeholders or environment variables.
- **Input Validation**: All external inputs must be validated to prevent injection attacks.
- **Dependency Updates**: When suggesting dependency updates (e.g., via Renovate), ensure the updated package is compatible with the current .NET target framework.

## 8. Guidelines for Commit Messages and PRs

- **Format**: Use conventional commits (e.g., `feat:`, `fix:`, `chore:`, `docs:`, `test:`). Refer to `GIT_CONVENTIONS.md` for detailed standards and granularity rules.
- **Clarity**: Commit messages should clearly describe *what* was changed and *why*.
- **PR Descriptions**: Automated PRs should include a summary of changes and a checklist confirming that tests were run.

---

*This file complements README.md and CONTRIBUTING.md, focusing specifically on machine-readable and automation-friendly guidance.*
