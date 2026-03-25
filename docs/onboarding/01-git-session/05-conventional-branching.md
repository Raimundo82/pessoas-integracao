# 05 — Conventional Branching

## 📖 Concept

**Conventional Branching** is a naming standard for Git branches. The idea is the same
as Conventional Commits (which we'll cover next): a consistent, machine-readable prefix
that tells you — and the pipeline — what kind of work this branch contains.

### The format

```
<type>/<kebab-case-description>
```

### Valid types

| Type                        | Use for                                 |
| --------------------------- | --------------------------------------- |
| `feat` / `feature`          | New functionality                       |
| `fix` / `bugfix` / `hotfix` | Bug fixes                               |
| `refactor`                  | Code restructuring, no behaviour change |
| `test`                      | Adding or fixing tests                  |
| `docs`                      | Documentation only                      |
| `ci`                        | CI/CD and pipeline changes              |
| `chore`                     | Dependencies, tooling, config           |
| `style`                     | Formatting, whitespace                  |
| `perf`                      | Performance improvements                |
| `release`                   | Release preparation                     |

### Why it matters

Our `branch-name-lint` workflow validates every branch name when a PR is opened.
An invalid name **fails the pipeline before anything else runs.**

```yaml
# .gitea/workflows/branch-name-lint.yaml (simplified)
PATTERN='^(fix|bugfix|hotfix|feat|feature|docs|...)\\/[a-z0-9]+([.-][a-z0-9]+)*$'
if [[ "$BRANCH_NAME" =~ $PATTERN ]]; then
echo "✅ Valid"
else
echo "❌ Invalid — pipeline fails"
exit 1
fi
```

### Good vs bad examples

```bash
# ✅ Valid
feat/add-employee-address
fix/duplicate-ni-check
test/employee-query-snapshot
docs/onboarding-module-01
ci/add-trivy-scan
chore/update-hotchocolate

# ❌ Invalid — will fail the pipeline
myfeature                    # no type prefix
feature/AddEmployeeAddress   # uppercase letters
feat/add employee address    # spaces
feature_add_address          # underscore separator
```

### The kebab-case rule

The description must be **lowercase** with **hyphens** as separators. No underscores,
no slashes (after the first one), no uppercase.

```bash
# ✅ kebab-case
add-employee-address-field
fix-null-reference-in-service

# ❌ Not kebab-case
addEmployeeAddressField    # camelCase
add_employee_address       # snake_case
Add-Employee-Address       # PascalCase
```

---

## 💻 Try it — Rename your branch to follow the convention

### Step 1: Check your current branch name

```bash
git branch
```

If the branch you created in the previous exercise (`docs/hello-<your-name>`) already
follows the convention — great, nothing to change.

If not, rename it:

```bash
# Rename local branch
git branch -m old-name docs/hello-<your-name>
```

### Step 2: Create a new branch that deliberately breaks the rule

```bash
git checkout master
git checkout -b MyTestBranch
```

Note how this would fail the `branch-name-lint` pipeline (no type, PascalCase).

### Step 3: Delete it and create a valid one

```bash
git checkout master
git branch -d MyTestBranch

git checkout -b test/branch-naming-exercise
```

### Step 4: Go back to your working branch

```bash
git checkout docs/hello-<your-name>
```

---

## ✅ Done when

- Your working branch is named `docs/hello-<your-name>` (or similar valid name)
- You can explain why `MyFeatureBranch` would fail the pipeline
