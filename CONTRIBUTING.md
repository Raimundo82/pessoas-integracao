# Contributing Guidelines

This project follows [**GitHub Flow**](https://docs.github.com/en/get-started/using-github/github-flow)

---

## 📂 Branching Strategy

- **`master`** → always deployable, production‑ready code  
- **Feature branches** → created from `master` for new work

### Rules

#### **Feature branching approach** based on the [Conventional Branching](https://conventional-branch.github.io/#summary) adapted

Branches must be named using the following format:

```
<type>/<kebab-case-description>
```

Where `<type>` is one of:

```
fix | bugfix | hotfix | release | feat | feature | docs | style | refactor | perf | test  | ci | chore
```

Example: `feat/add-login-endpoint`

#### **Pull Requests and commit messages** follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary)
