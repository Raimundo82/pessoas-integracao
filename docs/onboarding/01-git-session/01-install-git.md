# 01 — Install Git

## 📖 Concept

Git is a **distributed version control system**. It tracks changes to files over time
and lets multiple people work on the same codebase without stepping on each other.

Unlike older systems (SVN, TFS), every developer has a complete copy of the history
on their own machine. You can commit, branch, and look at history without a network
connection.

### Git vs Gitea vs GitHub

| Tool       | What it is                                                     |
| ---------- | -------------------------------------------------------------- |
| **Git**    | The command-line tool you install locally                      |
| **Gitea**  | A self-hosted web platform for hosting Git repos (what we use) |
| **GitHub** | The same idea, but cloud-hosted by Microsoft                   |

You install Git once. Gitea/GitHub are just websites that store repos and add
collaboration features (PRs, issues, CI/CD).

---

## 💻 Try it — Check and install Git

### Step 1: Check if Git is already installed

```bash
git --version
```

If you see something like `git version 2.47.0` — you're done, skip to step 3.

### Step 2: Install Git (if not installed)

**Windows:**
Download from [https://git-scm.com/download/win](https://git-scm.com/download/win)
and run the installer. Accept all defaults.

**macOS:**

```bash
xcode-select --install
# Or with Homebrew:
brew install git
```

**Linux (Debian/Ubuntu):**

```bash
sudo apt update && sudo apt install git -y
```

**Inside the dev container:**
Git is already installed. Run `git --version` to confirm.

### Step 3: Set your identity

Git tags every commit with your name and email. Set them now:

```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@marinha.pt"
```

Verify:

```bash
git config --global --list
```

You should see `user.name` and `user.email` in the output.

### Step 4: Set a default editor (optional but recommended)

```bash
# VS Code
git config --global core.editor "code --wait"

# Nano (simpler)
git config --global core.editor "nano"
```

---

## ✅ Done when

- `git --version` returns a version number
- `git config --global user.name` returns your name
- `git config --global user.email` returns your email
