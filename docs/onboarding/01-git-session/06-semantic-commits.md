# 06 — Semantic Commits (Conventional Commits)

## 📖 Concept

**Conventional Commits** is a standard for commit message formatting. Like Conventional
Branching, it gives commits a machine-readable prefix so tools (changelogs, release
automation, linters) can understand the history.

### The format

```
<type>: <short description>

[optional body]

[optional footer]
```

### The rules

- **Type** is one of the valid keywords (see below)
- **Description** is lowercase, under 72 characters, no period at the end
- **Body** (optional) explains the _why_, not the _what_ — add after a blank line
- **Footer** (optional) references issues: `Closes #42`

### Valid types

| Type              | What changed                      | Triggers (in semantic-release) |
| ----------------- | --------------------------------- | ------------------------------ |
| `feat`            | New feature                       | Minor version bump (1.**1**.0) |
| `fix`             | Bug fix                           | Patch version bump (1.1.**1**) |
| `refactor`        | Code change, no functional impact | No version bump                |
| `test`            | Tests added or changed            | No version bump                |
| `docs`            | Documentation only                | No version bump                |
| `ci`              | CI/CD pipeline                    | No version bump                |
| `chore`           | Deps, tooling                     | No version bump                |
| `style`           | Formatting                        | No version bump                |
| `perf`            | Performance                       | Patch bump                     |
| `BREAKING CHANGE` | API change                        | Major version bump (**2**.0.0) |

### Real examples from this project

```
feat: add GetEmployeeByNi GraphQL query
fix: throw EmployeeByNiNotFoundException when ni not found
test: add snapshot test for FetchAllEmployeesSorted query
refactor: extract dbContext disposal to DisposeAsync
docs: add onboarding guide for module 01
ci: add Trivy scan to PR workflow
chore: update HotChocolate packages to 14.4
```

### Breaking changes

If your change breaks an existing API contract, add `BREAKING CHANGE` in the footer:

```
feat: change employeeByNi to require authentication header

BREAKING CHANGE: anonymous access to employeeByNi is no longer allowed.
Callers must supply a valid Bearer token.
```

### Why it matters in our pipeline

Our `pull-request-lint` workflow checks the **PR title** against this standard:

```yaml
# .gitea/workflows/pull-request-lint.yaml
# If the title is "added some stuff" → pipeline fails
# If the title is "feat: add address field" → pipeline passes
```

The PR title becomes the squash-merge commit message — so keeping it clean keeps
`master`'s history clean.

---

## 💻 Try it — Amend your commit message and add a new commit

### Step 1: Check your last commit message

```bash
git log --oneline -3
```

### Step 2: Fix the message if it doesn't follow the standard

If your last commit message doesn't follow Conventional Commits, amend it:

```bash
git commit --amend -m "docs: add participant file for <your-name>"
```

> `--amend` rewrites the last commit. Only do this before pushing — never amend
> commits that are already on a shared branch.

### Step 3: Make another commit following the standard

Add a second line to your participant file:

```bash
echo "Attended the Git hands-on session." >> docs/onboarding/git-session/participants/<your-name>.md

git add docs/onboarding/git-session/participants/<your-name>.md
git commit -m "docs: add session attendance note for <your-name>"
```

### Step 4: Look at your history

```bash
git log --oneline
```

You should see two clean, Conventional Commit messages.

### Common mistakes to avoid

```bash
# ❌ These would fail pull-request-lint
git commit -m "Added the address field"   # no type, capitalised, past tense
git commit -m "fix"                       # no description
git commit -m "feat: Add address field."  # capitalised, has period

# ✅ Correct
git commit -m "feat: add address field to Employee model"
git commit -m "fix: handle null BiometricDetails in GetEmployeeByNi"
```

---

## ✅ Done when

- `git log --oneline -3` shows at least two commits with valid Conventional Commit messages
- You can explain what `feat:` vs `fix:` vs `chore:` means for semantic versioning
