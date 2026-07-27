# ADR Guidelines

This document defines the general rules and guidance for creating and maintaining Architecture Decision Records (ADRs) in this repository. The guidelines are based on the MADR (Markdown Architecture Decision Records) principles, adapted to this repository's conventions.

## 1.1 Purpose of an ADR

An ADR documents an architectural decision, the context that led to it, the options considered, the selected option, and its consequences.

An ADR is a durable record of architectural reasoning and should not become a general implementation guide, project plan, or task description.

An ADR should be created when a decision has meaningful long-term consequences for areas such as:

- System architecture
- Technology selection
- Integration patterns
- Data management
- Security architecture
- Deployment architecture
- Infrastructure
- Significant operational characteristics
- Important cross-cutting technical decisions

Avoid creating ADRs for routine implementation details that do not represent meaningful architectural decisions.

## 1.2 ADR Naming Convention

ADR filenames must follow this convention:

```
NNN-PPP-description-with-dashes.md
```

Where:

- `NNN` is the sequential ADR number **per project/assembly** (e.g., `001`, `002`, `003` for SYNC; `001`, `002`, `003` for GEN). The numbering restarts at `001` for each assembly alias.
- `PPP` identifies the assembly and must be one of the assembly aliases defined in `docs/adr/README.md`.
- `description-with-dashes` is a short, descriptive, kebab-case description of the decision.

Examples:

```
001-SYNC-sync-consumers-integration-contract.md
002-SYNC-adopt-event-driven-integration.md
001-GEN-adopt-datetimeoffset-for-all-dates.md
002-GEN-explicit-midnight-for-daily-granularity.md
003-A2DIP-use-redis-for-distributed-cache.md
```

The implementation must inspect `docs/adr/README.md` to determine the valid assembly aliases and their corresponding numbering schemes. Do not invent new assembly aliases.

When creating a new ADR, the agent must determine the next available sequential `NNN` value **for the specific assembly alias (PPP)** and avoid duplicate ADR numbers within that assembly's numbering scheme. The agent must not silently overwrite an existing ADR.

## 1.3 ADR Directory

All ADRs must be stored under:

```
docs/adr/
```

The canonical ADR templates and guidelines must also be stored under this directory:

```
docs/adr/adr-template-en.md
docs/adr/adr-template-pt.md
docs/adr/adr-guidelines.md
```

## 1.4 ADR Lifecycle and Status

The supported ADR lifecycle states and their intended meaning are:

- **Proposed**: The decision is still being discussed. An ADR must not be marked `Accepted` unless the decision has actually been made. When a decision is still being discussed, the ADR should be `Proposed`.
- **Accepted**: The decision has been made and approved by the team.
- **Rejected**: An option was considered but not selected.
- **Deprecated**: The decision is no longer recommended or in use.
- **Superseded**: A new ADR has been created that replaces or modifies this decision.

Additionally, the following rules must be followed:

- An ADR must not be used to imply that a recommendation from the AI agent was approved by the team.
- The agent must distinguish between a recommendation, a proposal, and an actual decision.

If the repository's canonical template does not include a `Status` section, status should be represented as a metadata line after the title (e.g., `Status: Proposed`) or tracked in the `docs/adr/README.md` index table, without unnecessarily diverging from the canonical MADR minimal template.

## 1.5 Decision vs. Implementation Details

ADRs should focus on architectural decisions and their rationale.

Avoid using ADRs for:

- Routine implementation details
- Class or method naming
- Minor refactoring
- Temporary code structure
- Detailed step-by-step implementation instructions
- Operational runbooks
- Project management tasks

Implementation details may be included when they are themselves part of the architectural decision.

## 1.6 Context and Problem Statement

The `Context and Problem Statement` section must explain:

- Why the decision was necessary
- The problem being solved
- Relevant constraints
- Important decision drivers
- The scope of the decision

The context should provide enough information for a future reader to understand the decision without having access to the original conversation. The context must not simply repeat the decision.

## 1.7 Considered Options

The ADR should identify the relevant alternatives considered for the decision.

The agent must not invent alternatives and present them as if they were actually evaluated by the team. If the agent identifies potentially relevant alternatives that were not explicitly considered, it may suggest them to the user, but they must not be represented as evaluated options without confirmation.

For significant architectural decisions, the ADR should explain why the selected option was preferred over the relevant alternatives.

## 1.8 Decision Outcome

The `Decision Outcome` section must explicitly state what was decided. The decision must be clear and unambiguous.

Avoid vague decision statements such as:

- "We may use..."
- "We prefer..."
- "We will consider..."
- "This could be implemented with..."

unless the ADR is explicitly documenting a proposal rather than an accepted decision.

The ADR should make it possible for a future reader to answer:

> What are we committing to?

## 1.9 Consequences and Trade-offs

The ADR must document meaningful consequences of the decision. Consequences should include both positive and negative consequences when applicable.

Consider relevant areas such as:

- Benefits
- Costs
- Risks
- Performance
- Security
- Operational impact
- Maintenance
- Developer experience
- Required skills
- Migration effort
- Vendor or platform lock-in
- Scalability
- Reliability

The agent must not present an architectural decision as universally positive when meaningful trade-offs exist.

## 1.10 Facts, Assumptions, Recommendations, and Decisions

The ADR creation process must clearly distinguish between:

- Facts explicitly provided or verified
- Assumptions inferred from available context
- Recommendations made by the agent
- Decisions actually made by the decision-makers

The agent must never convert an assumption or recommendation into an accepted decision.

The agent must never fabricate:

- Metrics
- Benchmarks
- Costs
- Security properties
- Compliance requirements
- Stakeholder approvals
- Team decisions
- Production incidents
- Dates
- Constraints
- Technical capabilities

If important information is missing, the agent should either ask for clarification or explicitly identify the information as an assumption.

## 1.11 Existing ADRs and Relationships

Before creating a new ADR, the agent should inspect existing ADRs when they are available.

The agent should look for:

- Existing ADRs documenting the same decision
- Related ADRs
- Conflicting decisions
- Decisions that may be superseded
- Decisions that the new ADR depends on

When relevant, ADRs should reference related decisions. Possible relationships include:

- Related to
- Depends on
- Supersedes
- Superseded by
- Amends
- Conflicts with

The agent must not silently create a new ADR that contradicts an existing decision.

If a new decision materially replaces an existing accepted decision, the preferred approach is to create a new ADR that supersedes the previous ADR rather than rewriting the historical ADR.

## 1.12 Preserve ADR History

ADRs are historical records. Once an ADR documents an accepted decision, the agent must not silently rewrite it to represent a later decision.

When a decision materially changes:

1. Create a new ADR.
2. Document the new decision.
3. Mark the relationship with the previous ADR appropriately.
4. Preserve the original ADR as historical documentation.

## 1.13 ADR Quality Principles

The guidelines should encourage ADRs to be:

- Clear
- Concise
- Specific
- Durable
- Focused on architectural reasoning
- Understandable without the original conversation

Avoid unnecessary implementation details that are likely to become obsolete. Prefer documenting durable reasoning and trade-offs.

## 1.14 Language

ADRs may be written in pt-PT or en-US. Maintain consistency with the language used in the existing ADRs and the project context.

When creating a new ADR, the appropriate template should be used based on the language of the ADR:

- For ADRs written in en-US, use `docs/adr/adr-template-en.md`.
- For ADRs written in pt-PT, use `docs/adr/adr-template-pt.md`.
