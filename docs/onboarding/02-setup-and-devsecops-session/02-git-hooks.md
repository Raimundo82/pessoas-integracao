# 02 — Git Hooks — Lefthook

## 📖 Concept

### What are git hooks?

Git hooks are scripts that run automatically at specific points in the Git workflow.
We use two:

| Hook         | When it runs              | What it does          |
| ------------ | ------------------------- | --------------------- |
| `pre-commit` | Before every `git commit` | Formats staged files  |
| `pre-push`   | Before every `git push`   | _(can be configured)_ |

Without hooks, you might commit unformatted code and only discover the problem when
the pipeline fails on Gitea. Hooks move that feedback to your machine, before the
commit even exists.

### Lefthook

We use **Lefthook** (`lefthook.yml`) to manage git hooks. It's faster than Husky,
works across all platforms, and is configured in a single YAML file.

Lefthook is installed as a dev dependency (`npm ci`) and registered via the npm
`prepare` script (`lefthook install`). The `post-script.sh` that runs after the
dev container is created handles this automatically.

### Our `pre-commit` hook

```yaml
# lefthook.yml
pre-commit:
  parallel: true
  commands:
    prettier:
      stage_fixed: true
      glob: '**/*.{md,mdx,yml,yaml,json,jsonc}'
      run: npx prettier --write --no-error-on-unmatched-pattern {staged_files}
    dotnet-format:
      stage_fixed: true
      glob: '**/*.cs'
      run: dotnet format --include {staged_files}
```

Breaking this down:

| Setting             | What it means                                                  |
| ------------------- | -------------------------------------------------------------- |
| `parallel: true`    | Both formatters run at the same time                           |
| `stage_fixed: true` | Files are automatically re-staged after formatting             |
| `glob: '**/*.cs'`   | Only runs on staged `.cs` files — fast, not the whole codebase |
| `{staged_files}`    | Lefthook injects the list of staged files here                 |

### What actually happens on `git commit`

```
git commit -m "feat: add something"
    │
    ▼
Lefthook: pre-commit
    ├── prettier runs on staged .md / .yml / .json files → fixes in-place → re-stages
    └── dotnet format runs on staged .cs files → fixes in-place → re-stages
    │
    ▼ (if no errors)
Commit is created with already-formatted code
```

If a file can't be formatted (syntax error), the commit is blocked until you fix it.

### The `stage_fixed: true` magic

This is the most important setting. Without it:

1. You stage a file
2. Hook formats it
3. Hook exits
4. Commit includes the **unformatted** version (the formatting changes are unstaged)

With `stage_fixed: true`:

1. You stage a file
2. Hook formats it
3. Hook **re-stages the fixed version**
4. Commit includes the **formatted** version

You never have to manually `git add` after a hook fixes a file.

---

## 💻 Try it — Trigger the pre-commit hook

### Step 1: Check that hooks are installed

```bash
cat .git/hooks/pre-commit
```

You should see a Lefthook invocation script. If the file is empty or missing:

```bash
npm ci
lefthook install
```

### Step 2: Break formatting intentionally

Open `src/Pessoas.Integracao.Core/Domain/Entities/Pessoa.cs` file and mess up the indentation — add extra spaces or remove them:

```csharp
// Original
public class Pessoa
{
    public int Id { get; set; }
...
}
// Broken (add extra spaces or wrong indentation)
public class Pessoa
{
        public int Id { get; set; }  // extra indent
...
}
```

Save the file.

### Step 3: Stage and try to commit

```bash
git add src/Pessoas.Integracao.Core/Domain/Entities/Pessoa.cs
git commit -m "test: trigger formatting hook"
```

Watch the output — Lefthook will run `dotnet format` on the staged file, fix the
indentation, re-stage the fixed version, and then allow the commit to proceed.

```
╔══════════════════════════════════════╗
║  Lefthook v2.x                       ║
╚══════════════════════════════════════╝
  RUNNING dotnet-format...
  ✔ dotnet-format (fixed and re-staged)
  COMMIT CREATED
```

### Step 4: Verify the commit has clean code

```bash
git show HEAD -- src/Pessoas.Integracao.Core/Domain/Entities/Pessoa.cs | head -20
```

The committed version should be properly formatted, even if your working copy had
broken indentation before the commit.

### Step 5: Undo your test commit

```bash
git reset HEAD~1
git checkout src/Pessoas.Integracao.Core/Domain/Entities/Pessoa.cs
```

---

## ✅ Done when

- You've seen Lefthook run in your terminal during a `git commit`
- You understand that `stage_fixed: true` means you never get a "formatted but unstaged" situation
