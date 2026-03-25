# 04 — CI Phases — ci-build-test

## 📖 Concept

The `ci-build-test` workflow (`.gitea/workflows/ci-build-test.yaml`) runs on every
Pull Request. It is the main quality gate — all phases must pass for a PR to be
mergeable.

### The phases in order

```
PR opened / new commit pushed
        │
        ▼
① Checkout (fetch-depth: 0)
        │
        ▼
② Setup Node.js 24.10.x + npm ci
  (installs Lefthook, Prettier, semantic-release)
        │
        ▼
③ Setup .NET SDK
  (version from global.json → 10.0.201)
        │
        ▼
④ dotnet format --verify-no-changes
  ← FAILS FAST if any .cs file is not formatted
        │
        ▼
⑤ dotnet build --no-incremental
  ← FAILS if code doesn't compile
        │
        ▼
⑥ dotnet list package --vulnerable --include-transitive --no-restore
  ← WARNS/FAILS if any NuGet package has a known CVE
        │
        ▼
⑦ dotnet test --no-build
  ← FAILS if any test fails
        │
        ▼
        ✅ All green → PR can be reviewed and merged
```

### Why this order matters

Each phase is ordered from cheapest to most expensive:

- **Format check** — fastest, fails in seconds, no compilation needed
- **Build** — slow for a cold cache, but catches typos and missing references
- **Vulnerability scan** — reads the NuGet lock, no network call needed
- **Tests** — slowest, but only runs if everything else passed

If the format check fails, you never waste time waiting for the build and tests.

### `fetch-depth: 0`

```yaml
- name: Checkout
  uses: actions/checkout@v6
  with:
    fetch-depth: 0
```

`fetch-depth: 0` fetches the complete Git history, not just the latest commit.
This is required by SonarQube (needs history to compute blame and new-code periods)
and by the semantic-release tool.

### `dotnet build --no-incremental`

`--no-incremental` forces a clean build every time, ignoring any cached build
artifacts. In CI this prevents false positives where a previous cached build hides
a real compile error.

### `dotnet test --no-build`

`--no-build` skips re-compiling before running tests. Since the build step already
ran, this is safe and saves time.

### The `.NET SDK version is pinned`

```json
// global.json
{
  "sdk": {
    "version": "10.0.201",
    "rollForward": "latestPatch"
  }
}
```

`setup-dotnet@v5` reads this file and installs exactly that version. This means CI
always uses the same SDK as the dev container — no "works locally but not in CI"
surprises.

---

## 💻 Try it — Read the CI output on your PR

### Step 1: Open your PR from Module 01

Go to your PR on Gitea (`devops-01.marinha.pt`). Click the **Checks** tab.

### Step 2: Open the `ci-build-test` run

Click on the `Lint, Build and Test` workflow. You'll see the job with its steps.

Expand each step and read the output:

| Step                      | What to look for                                  |
| ------------------------- | ------------------------------------------------- |
| Format code               | `No changes needed` or a diff of what was wrong   |
| Build                     | `Build succeeded` and how many warnings           |
| Check vulnerable packages | Any packages flagged, or `No vulnerable packages` |
| Test                      | How many tests ran, passed, failed                |

### Step 3: Run all CI phases locally

You can mirror the entire CI locally:

```bash
# Phase 4: format check
dotnet format --verify-no-changes

# Phase 5: build
dotnet build --no-incremental

# Phase 6: vulnerability scan
dotnet list package --vulnerable --include-transitive --no-restore

# Phase 7: tests
dotnet test --no-build
```

Run this before every push — if it all passes locally, CI will pass too.

---

## ✅ Done when

- You've read the output of all 7 CI steps on your PR
- You can run the four core commands locally and they all pass
