# 07 — Create a Pull Request

## 📖 Concept

A **Pull Request** (PR) is a formal proposal to merge your branch into another branch.
It's where:

- **Automated checks run** (lint, build, test, security scans)
- **Code review happens** (teammates comment and approve)
- **Merge happens** (after all checks pass and the PR is approved)

### PR vs direct push to master

```
Direct push to master:       Dangerous — bypasses all checks and review
PR to master:                Safe — checks run, teammates review, then merge
```

In our repo, direct pushes to `master` are **blocked**. All changes go through PRs.

### The PR lifecycle

```
Push branch → Open PR → Pipeline runs → Reviewer comments
                                              │
                              ┌───────────────┘
                              ▼
                         Fix comments → Re-push → Re-review
                              │
                              ▼
                         All checks green + approved → Squash merge → Delete branch
```

### What makes a good PR

| Element     | Good                                  | Bad                      |
| ----------- | ------------------------------------- | ------------------------ |
| Title       | `feat: add GetEmployeeByNumsap query` | `my changes`             |
| Size        | One logical change                    | Months of work in one PR |
| Description | What + why + how to test              | Empty                    |
| Checklist   | Filled                                | Ignored                  |
| Branch      | `feat/add-numsap-query`               | `master` or `dev`        |

### Our PR template

When you open a PR, Gitea loads `.gitea/PULL_REQUEST_TEMPLATE.md`:

```markdown
## Checklist

- [ ] Branch name follows Conventional Branch
- [ ] PR titles and Commit messages follow Conventional Commits
```

Always fill it. If your change is non-trivial, add a description of what it does
and why — reviewers will thank you.

---

## 💻 Try it — Push your branch and open a PR

### Step 1: Push your branch to your fork

```bash
git push origin docs/hello-<your-name>
```

The first time you push a new branch, Git will print a URL. You can click it to go
directly to the "Open PR" page on Gitea.

### Step 2: Open the PR on Gitea

1. Go to `https://devops-01.marinha.pt/<your-username>/pessoas-integracao`
2. You'll see a banner: **"You recently pushed docs/hello-<your-name> — Compare & pull request"**
3. Click **Compare & pull request**

Or manually:

1. Go to the **Pull Requests** tab
2. Click **New Pull Request**
3. Set **base repo** to `marinha-si/pessoas-integracao`, **base branch** to `master`
4. Set **head repo** to `<your-username>/pessoas-integracao`, **compare** to `docs/hello-<your-name>`

### Step 3: Fill in the PR

- **Title:** `docs: add participant file for <your-name>`  
  _(Must follow Conventional Commits — the pipeline checks this)_
- **Description:** Fill the checklist. Optionally add what you changed and why.
- **Reviewers:** Assign a teammate

### Step 4: Watch the pipeline

After opening the PR, go to the **Checks** tab. You should see the workflows start running:

- `branch-name-lint` — should pass ✅
- `pull-request-lint` — should pass ✅
- `ci-build-test` — build + tests run ✅

### Step 5: Respond to a review comment

Ask a teammate to leave a comment on your PR. Then:

1. Fix the issue locally (edit the file)
2. Stage and commit: `git add . && git commit -m "docs: address review comment"`
3. Push: `git push origin docs/hello-<your-name>`
4. Go back to the PR — the new commit appears automatically, and checks re-run

### Step 6: After approval — merge

Once approved and all checks are green:

1. Click **Squash and merge**
2. Confirm the squash message (should be your PR title)
3. Click **Delete branch** after merge

---

## ✅ Done when

- Your PR is open on Gitea with a valid title
- At least one pipeline check has run (green or red — you've seen it run)
- You've assigned at least one reviewer

---

## Quick reference — the full session workflow

```bash
# 1. Install & configure
git config --global user.name "Your Name"
git config --global user.email "your@email.com"

# 2. SSH key
ssh-keygen -t ed25519 -C "your@email.com"
# Add ~/.ssh/id_ed25519.pub to Gitea

# 3. Clone
git clone git@devops-01.marinha.pt:marinha-si/pessoas-integracao.git
cd pessoas-integracao

# 4-5. Branch (GitHub Flow + Conventional Branching)
git checkout master && git pull origin master
git checkout -b docs/hello-<your-name>

# 6. Commit (Semantic Commits)
git add .
git commit -m "docs: add participant file for <your-name>"

# 7. Push and open PR
git push origin docs/hello-<your-name>
# → Open PR on Gitea with title: docs: add participant file for <your-name>
```
