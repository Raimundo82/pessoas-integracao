# 04 — GitHub Flow

## 📖 Concept

**GitHub Flow** is a lightweight branching strategy built around one rule:
`master` is always deployable.

Everything else follows from that:

```
master ─────────────────────────────────────────────────────▶  always deployable
           │                              ▲
           └── feat/my-thing ────────────┘
                  (short-lived branch)        (PR merged)
```

### The 6 steps of GitHub Flow

```
1. Create a branch from master
2. Make commits on the branch
3. Open a Pull Request
4. Discuss and review the code
5. Deploy / run checks
6. Merge to master
```

### Why not Gitflow (develop/release branches)?

| Gitflow                     | GitHub Flow                  |
| --------------------------- | ---------------------------- |
| Long-lived `develop` branch | Only `master`                |
| Release branches            | Tags on master               |
| Complex merge ceremonies    | Simple squash merge          |
| Good for scheduled releases | Good for continuous delivery |

We use **GitHub Flow** because we want `master` to always reflect what's running,
and we release via Docker image tags — not branches.

### Key habits

- **Branch from master.** Never branch from another feature branch.
- **Keep branches short-lived.** A branch older than a week is a warning sign.
- **One thing per branch.** Don't bundle unrelated changes.
- **Delete the branch after merge.** Gitea will prompt you.

---

## 💻 Try it — Create a branch and make a commit

### Step 1: Make sure master is up to date

```bash
git checkout master
git pull origin master
```

### Step 2: Create a feature branch

```bash
git checkout -b docs/hello-<your-name>
```

The `-b` flag creates the branch and switches to it in one step.

Verify you're on the new branch:

```bash
git branch
# The * indicates the current branch
```

### Step 3: Make a change

Create a file:

```bash
mkdir -p docs/onboarding/git-session/participants
echo "# Hello from <your-name>" > docs/onboarding/git-session/participants/<your-name>.md
```

### Step 4: Stage and commit

```bash
# Check what changed
git status

# Stage the new file
git add docs/onboarding/git-session/participants/<your-name>.md

# Confirm what will be committed
git diff --staged

# Commit
git commit -m "docs: add participant file for <your-name>"
```

### Step 5: Useful branch commands

```bash
# See all local branches
git branch

# See all branches (including remote)
git branch -a

# Switch between branches
git checkout master
git checkout docs/hello-<your-name>

# Delete a branch (after it's merged)
git branch -d docs/hello-<your-name>
```

---

## ✅ Done when

- `git branch` shows your new branch with a `*` next to it
- `git log --oneline -3` shows your commit at the top
