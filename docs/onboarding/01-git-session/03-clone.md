# 03 — Clone a Repo

## 📖 Concept

A **clone** copies the repository from the server to your local machine:

```
Gitea server                              Your laptop
marinha-si/pessoas-integracao  ──clone──▶  ~/projects/pessoas-integracao/
```

After cloning you have a full local copy — all files, all history — and you can work
offline.

### What happens when you clone

Git automatically sets up one **remote** called `origin`, pointing back at the repo you
cloned from:

| Remote          | Name     | Points to                                                    |
| --------------- | -------- | ------------------------------------------------------------ |
| The shared repo | `origin` | `git@devops-01.marinha.pt:marinha-si/pessoas-integracao.git` |

`origin` is where you push your branches and open Pull Requests from.

### Our workflow

In this project everyone clones the shared repo directly and works in branches.
You never commit to `master` directly — that's what PRs are for:

```
origin/master (shared repo)
       │
       ▼ clone
local master        ← keep in sync with origin
       │
       └── feat/your-branch   ← your work lives here
                │
                ▼ push + open PR
       origin/feat/your-branch   → reviewed → merged to master
```

---

## 💻 Try it — Clone the repo

### Step 1: Clone

```bash
# Create a projects folder if you don't have one
mkdir -p ~/projects
cd ~/projects

# Clone the shared repo
git clone git@devops-01.marinha.pt:marinha-si/pessoas-integracao.git

cd pessoas-integracao
```

### Step 2: Verify the remote

```bash
git remote -v
```

Expected output:

```
origin  git@devops-01.marinha.pt:marinha-si/pessoas-integracao.git (fetch)
origin  git@devops-01.marinha.pt:marinha-si/pessoas-integracao.git (push)
```

### Step 3: Explore the repo

```bash
# See the branch you're on and overall status
git status

# See the commit history
git log --oneline -10

# List all files
ls -la
```

### Keeping your local copy up to date (for future reference)

Before starting any new piece of work, always pull the latest changes from origin:

```bash
git checkout master
git pull origin master
```

---

## ✅ Done when

- `git remote -v` shows `origin` pointing to `marinha-si/pessoas-integracao.git`
- `git log --oneline -5` shows the last 5 commits of the project
