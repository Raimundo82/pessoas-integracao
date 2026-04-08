# 01 — Project Setup — Docker & Dev Container

## 📖 Concept

### Why a Dev Container?

Setting up a .NET + PostgreSQL + Node.js environment manually is fragile — it works
differently on every machine, and "works on my machine" is never a useful answer.

A **Dev Container** solves this by packaging the entire development environment into
a Docker container defined in code. Everyone on the team has the exact same:

- .NET SDK version (`10.0.201`, pinned in `global.json`)
- Node.js version (`24.10.0`)
- PostgreSQL 17
- VS Code extensions (SonarLint, C# Dev Kit, Prettier, EditorConfig...)
- Environment variables and proxy settings

### How it works

```
Your laptop
└── Docker Desktop (the engine)
    └── Dev Container (defined in .devcontainer/)
        ├── app service  ← your code runs here (.NET 10, Node 24)
        └── db service   ← PostgreSQL 17
```

VS Code's **Dev Containers extension** detects `.devcontainer/devcontainer.json` and
offers to reopen the project inside the container. From that point your terminal,
your build commands, and your debugger all run inside the container.

### What's in `.devcontainer/`

| File                  | Purpose                                                                                 |
| --------------------- | --------------------------------------------------------------------------------------- |
| `Dockerfile`          | Base image (`dotnet:10.0-noble`) + Marinha CA cert + proxy config                       |
| `docker-compose.yaml` | Two services: `app` (your code) + `db` (Postgres 17)                                    |
| `devcontainer.json`   | VS Code config: extensions, features (.NET, Node, Docker-in-Docker), post-create script |
| `.env.example`        | Template for the required environment variables                                         |
| `post-script.sh`      | Runs after container creation — installs .NET tools                                     |

### Environment variables

The `docker-compose.yaml` reads from `.devcontainer/.env`:

| Variable               | Used for                        |
| ---------------------- | ------------------------------- |
| `DB_PASSWORD`          | PostgreSQL password             |
| `READ_API_KEY`         | API key for read operations     |
| `WRITE_API_KEY`        | API key for write operations    |
| `APP_SUB_PATH`         | Application sub-path (optional) |
| `SONARQUBE_CI_TOKEN`   | SonarQube CI token              |
| `SONARQUBE_USER_TOKEN` | SonarQube connected mode token  |
| `SONAR_HOST_URL`       | SonarQube server URL            |

> ⚠️ `.env` is in `.gitignore` — never commit it. Ask a teammate for the values.

---

## 💻 Try it — Install Docker and open the project

### Step 1: Install Docker Desktop

Download from [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)

After installing, start Docker Desktop and wait until the whale icon in the taskbar
shows "Docker Desktop is running".

Verify from a terminal:

```bash
docker --version
docker compose version
```

### Step 2: Install the VS Code Dev Containers extension

In VS Code, go to Extensions (`Ctrl+Shift+X`) and search for:

```
Dev Containers   (ms-vscode-remote.remote-containers)
```

Install it and reload VS Code.

### Step 3: Clone the repo (if you haven't already)

```bash
git clone git@devops-01.marinha.pt:marinha-si/pessoas-integracao.git
cd pessoas-integracao
```

### Step 4: Set up environment variables

```bash
cp .devcontainer/.env.example .devcontainer/.env
```

Open `.devcontainer/.env` and fill in the values. Ask a teammate for:

- `DB_PASSWORD`
- `READ_API_KEY` and `WRITE_API_KEY`
- `SONARQUBE_CI_TOKEN`, `SONARQUBE_USER_TOKEN`, `SONAR_HOST_URL`

### Step 5: Open in Dev Container

In VS Code:

1. Press `F1` (or `Ctrl+Shift+P`)
2. Type and select: **Dev Containers: Open Folder in Container...**
3. Choose the project folder
4. Wait — VS Code will build the Docker image and start the containers

The first build takes a few minutes. Subsequent opens are fast.

### Step 6: Verify the environment

Once inside the container, open a terminal (`Ctrl+`` `) and run:

```bash
dotnet --version       # should show 10.0.xxx
node --version         # should show v24.10.x
psql --version         # should show postgres client, in the postgres docker container

dotnet restore
dotnet build
dotnet test
```

All three commands should succeed with no errors.

---

## ✅ Done when

- `dotnet build` completes with no errors inside the dev container
- `dotnet test` passes all tests
- You can see two running containers in Docker Desktop: `app` and `db`
