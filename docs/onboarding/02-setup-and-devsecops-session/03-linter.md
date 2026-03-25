# 03 — The Linter — dotnet format & Prettier

## 📖 Concept

This project uses two formatters — one for C#, one for everything else:

| Tool            | Covers                                    | Config          |
| --------------- | ----------------------------------------- | --------------- |
| `dotnet format` | All `.cs` files                           | `.editorconfig` |
| `Prettier`      | `.md`, `.yml`, `.yaml`, `.json`, `.jsonc` | `.prettierrc`   |

They run in two places:

- **Locally** via the Lefthook `pre-commit` hook (on staged files only)
- **In CI** via `ci-build-test.yaml` (on the whole codebase, in verify mode)

### `dotnet format`

`dotnet format` reads formatting rules from `.editorconfig` and applies them. The
`.editorconfig` in this project is comprehensive — it defines indent size, line
endings, spacing around braces, blank line rules, and many Roslyn analyser settings.

Two modes:

| Mode       | Command                             | Use for                                                    |
| ---------- | ----------------------------------- | ---------------------------------------------------------- |
| **Fix**    | `dotnet format`                     | Fix all formatting issues in the solution                  |
| **Verify** | `dotnet format --verify-no-changes` | Check only — exit with error code if anything would change |

CI uses `--verify-no-changes`. If your code isn't formatted, CI fails immediately.

Running it locally on a single file (what the hook does):

```bash
dotnet format --include src/Pessoas.Integracao.Core/Domain/Entities/Pessoa.cs
```

Running it on the whole solution:

```bash
dotnet format
```

### Prettier

Prettier enforces consistent formatting for non-C# files. The `.prettierrc` config
defines rules like quote style and trailing commas. Prettier is strict — it rewrites
files to its own opinion of correct.

```bash
# Fix all files
npx prettier --write .

# Check only (CI mode)
npx prettier --check .
```

### SonarLint in VS Code

The dev container installs **SonarLint** (`SonarSource.sonarlint-vscode`). It gives
you inline warnings in the editor as you type — the same rules that SonarQube runs
in CI. You can connect it to the SonarQube server in connected mode for an even
tighter feedback loop (requires `SONARQUBE_USER_TOKEN`).

### EditorConfig

`.editorconfig` is the source of truth for formatting rules. VS Code respects it
automatically (the EditorConfig extension is installed in the container). This ensures
that even without explicitly running `dotnet format`, VS Code's auto-format on save
will produce correctly formatted code.

---

## 💻 Try it — Break and fix formatting

### Step 1: Break C# formatting

Open `src/Pessoas.Integracao.Core/Domain/Entities/Pessoa.cs` and introduce a formatting issue:

```csharp
// Add an extra blank line between properties, or wrong spacing
public int Id{get;set;}   // no spaces around braces
```

### Step 2: Run the formatter in verify mode

```bash
dotnet format --verify-no-changes
```

You should see an error — this is exactly what CI sees when formatting is broken.

### Step 3: Fix it automatically

```bash
dotnet format
```

Run verify again to confirm it passes:

```bash
dotnet format --verify-no-changes
# Should exit 0 with no output
```

### Step 4: Try Prettier on a YAML file

Open `.gitea/workflows/ci-build-test.yaml` and change the indentation of one line
from 2 spaces to 3 spaces.

```bash
npx prettier --check .gitea/workflows/ci-build-test.yaml
# Should report a formatting issue

npx prettier --write .gitea/workflows/ci-build-test.yaml
# Fixes it

npx prettier --check .gitea/workflows/ci-build-test.yaml
# Passes
```

### Step 5: Revert your changes

```bash
git checkout .
```

---

## ✅ Done when

- You've seen `dotnet format --verify-no-changes` fail on broken code
- You've seen `dotnet format` fix it automatically
- You understand the difference between fix mode and verify mode
