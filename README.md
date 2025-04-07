# SIGDN RH Stagging API

## 🌐 Production

The application is deployed and running in production at:

**http://paas-01.marinha.pt/rh-stagging**

You can test swagger API with SSO

**http://paas-01.marinha.pt/rh-stagging/swagger**

For now, it doesn't deliver yet any relevant functionality.

## 🛠️ Development Environment with Dev Containers

This project is configured to run in a [Dev Container](https://containers.dev/) environment using Visual Studio Code and the Dev Containers extension. This setup ensures a consistent and reproducible development experience across different machines.

### 📦 What's Included

The development environment includes:

- **.NET SDK** (latest version)
- **PostgreSQL** database
- **Node.js 18** with Yarn, PNPM, and node-gyp dependencies
- Pre-installed **VS Code extensions**:
  - C# development kit (`ms-dotnettools.csdevkit`)
  - C# extension (`ms-dotnettools.csharp`)
  - Prettier (`esbenp.prettier-vscode`)
  - SQLTools and PostgreSQL driver (`mtxr.sqltools`, `mtxr.sqltools-driver-pg`)

### 📁 Dev Container Specs

The container is defined in `.devcontainer/devcontainer.json` and uses a Docker Compose setup with the following structure:

- **Primary service**: `app`
- **Workspace folder**: `/workspaces/${localWorkspaceFolderBasename}`
- **PostgreSQL** is included and exposed via Docker Compose
- **Post-creation script**: Automatically installs .NET tools (`.devcontainer/script/install-dotnet-tools.sh`)

### 🚀 Getting Started

> **Pre-requisites**:
>
> - [Docker](https://docs.docker.com/get-docker/)
> - [Visual Studio Code](https://code.visualstudio.com/)
> - [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

1. **Clone the repository** (if you haven't already):

   ```bash
   git clone https://github.com/your-org/your-repo.git
   cd your-repo
   ```

2. **Open in VS Code**:

- Press F1 (or Ctrl+Shift+P) and run:
  Dev Containers: Open Folder in Container...Then choose the project folder.

- VS Code will automatically:
  - Build the dev container using docker-compose.yml
  - Mount the project into /workspaces/sigdn-rh-stagging-api
  - Install required dependencies and extensions
