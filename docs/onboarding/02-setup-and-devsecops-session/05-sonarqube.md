# 05 — SonarQube Scan

## 📖 Concept

### What is SonarQube?

SonarQube is a **static analysis platform**. It analyses your code for:

| Category              | Examples                                                 |
| --------------------- | -------------------------------------------------------- |
| **Bugs**              | Null dereferences, resource leaks, incorrect comparisons |
| **Code smells**       | Long methods, duplicate code, dead code                  |
| **Security hotspots** | Hardcoded credentials, SQL injection risk, weak crypto   |
| **Coverage**          | Which lines of code are exercised by tests               |

Unlike the format check or the build, SonarQube analyses intent and patterns — things
the compiler doesn't catch.

### Two scans, two workflows

| Workflow               | Trigger          | Purpose                                                               |
| ---------------------- | ---------------- | --------------------------------------------------------------------- |
| `ci-pr-scan.yaml`      | Every PR         | Analyses only the **new code** in the PR — decorated on the PR itself |
| `post-merge-scan.yaml` | Push to `master` | Full analysis of the whole codebase on master                         |

### The PR scan workflow — step by step

```yaml
# ci-pr-scan.yaml (simplified)
① Install dotnet-sonarscanner and dotnet-coverage
② dotnet-sonarscanner begin  ← opens a SonarQube session
③ dotnet build               ← compiles with Sonar instrumentation
④ dotnet-coverage collect "dotnet test"  ← runs tests, captures coverage as XML
⑤ dotnet-sonarscanner end    ← uploads results to SonarQube server
⑥ Quality Gate check         ← polls SonarQube, fails if Quality Gate not met
```

### The Quality Gate

The **Quality Gate** is a set of pass/fail conditions configured on the SonarQube
server. Our PR cannot be merged if the Quality Gate fails.

`pessoas-integracao` **Quality Gate** conditions:

- New code has 0 issues
- All new security hotspots are reviewed
- Coverage on new code ≥ 80.0%
- Duplications on new code ≤ 3.0%

### What is excluded

```
/d:sonar.exclusions="**/.devcontainer/**,**/Migrations/**,**/bin/**,**/obj/**,**/Generated/**"
```

EF Core Migrations are excluded because they are auto-generated and don't benefit
from static analysis.

### SonarLint in VS Code (local feedback)

The dev container installs **SonarLint**. It provides inline feedback in VS Code
using the same rules as SonarQube — without waiting for CI.

To connect SonarLint to the SonarQube server (connected mode):

1. Open VS Code settings (`Ctrl+,`)
2. Search for "SonarLint connected mode"
3. Add a connection to `SONAR_HOST_URL` with your `SONARQUBE_USER_TOKEN`

In connected mode, SonarLint uses the exact same ruleset as the server, so there are
no surprises when the CI scan runs.

### dotnet-tools.json

The `.config/dotnet-tools.json` pins the exact versions of all .NET tools:

```json
{
  "dotnet-sonarscanner": { "version": "11.2.0" },
  "dotnet-coverage": { "version": "18.5.2" },
  "dotnet-ef": { "version": "10.0.5" },
  "reportgenerator": { "version": "5.5.4" }
}
```

To restore them inside the dev container:

```bash
dotnet tool restore
```

This is run by `post-script.sh` automatically after the container is created.

---

## 💻 Try it — Explore the SonarQube dashboard

### Step 1: Open the SonarQube dashboard

Go to the URL in `SONAR_HOST_URL` (ask a teammate if you don't have it). Log in with
your credentials.

### Step 2: Find the project

Search for `pessoas-integracao`. Click the project.

### Step 3: Explore the main branch overview

On the main dashboard you'll see:

- Overall rating (Reliability, Security, Maintainability)
- Test coverage percentage
- Lines of code, duplications

Click through to **Issues** and filter by:

- Type: Bug / Code Smell / Security Hotspot
- Status: Open

### Step 4: Find your PR decoration

Go back to Gitea and open your PR. SonarQube should have added a comment showing
the Quality Gate result for your PR. Click the link to see the analysis of only
the code you changed.

---

## ✅ Done when

- You've seen the SonarQube dashboard for `pessoas-integracao`
- You've found the Quality Gate result on your PR in Gitea
- You know the difference between the PR scan and the post-merge scan
