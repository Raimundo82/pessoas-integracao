# 02 — Configure SSH Keys

## 📖 Concept

When you push code to Gitea, Gitea needs to know who you are. There are two ways to
authenticate:

| Method | How                                                   | Verdict     |
| ------ | ----------------------------------------------------- | ----------- |
| HTTPS  | Username + password (or token) on every push          | ❌ Annoying |
| SSH    | A cryptographic key pair — set up once, works forever | ✅ Use this |

### How SSH keys work

SSH uses a **key pair**:

- **Private key** — stays on your machine, never shared. Think of it as your password.
- **Public key** — you give this to Gitea. Think of it as your username/lock.

When you push, your machine uses the private key to prove it holds the matching pair.
Gitea checks against your registered public key. No password needed.

```
Your machine                    Gitea
    │                             │
    │── "I want to push" ────────▶│
    │◀─ "Prove who you are" ──────│
    │── [signs with private key] ─▶│
    │◀─ "Verified ✅ Access granted"│
```

---

## 💻 Try it — Generate and register an SSH key

### Step 1: Check for existing keys

```bash
ls ~/.ssh/
```

If you see `id_ed25519` and `id_ed25519.pub` — you already have a key pair.
Skip to Step 3.

### Step 2: Generate a new key

```bash
ssh-keygen -t ed25519 -C "your.email@marinha.pt"
```

When prompted:

- **File location:** Press Enter to accept the default (`~/.ssh/id_ed25519`)
- **Passphrase:** Press Enter to skip (or add one for extra security)

This creates two files:

- `~/.ssh/id_ed25519` — your **private key** (never share this)
- `~/.ssh/id_ed25519.pub` — your **public key** (you'll give this to Gitea)

### Step 3: Copy your public key

```bash
# macOS
cat ~/.ssh/id_ed25519.pub | pbcopy

# Linux
cat ~/.ssh/id_ed25519.pub | xclip -selection clipboard
# Or just print it and copy manually:
cat ~/.ssh/id_ed25519.pub

# Windows (Git Bash)
cat ~/.ssh/id_ed25519.pub | clip
```

The key looks like:

```
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAA... your.email@marinha.pt
```

### Step 4: Add the key to Gitea

1. Go to `https://devops-01.marinha.pt`
2. Click your avatar → **Settings** → **SSH / GPG Keys**
3. Click **Add Key**
4. Give it a name (e.g. `My Laptop`)
5. Paste the public key into the **Key** field
6. Click **Add Key**

### Step 5: Test the connection

```bash
ssh -T git@devops-01.marinha.pt
```

Expected output:

```
Hi <your-username>! You've successfully authenticated, but Gitea does not provide shell access.
```

If you see this — SSH is working. ✅

### Troubleshooting

```bash
# Verbose output to debug connection issues
ssh -vT git@devops-01.marinha.pt

# Check the SSH agent has your key loaded
ssh-add -l

# Add key to agent if missing
ssh-add ~/.ssh/id_ed25519
```

---

## ✅ Done when

`ssh -T git@devops-01.marinha.pt` returns a welcome message with your username.
