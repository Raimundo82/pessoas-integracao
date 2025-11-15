# SIGDN RH Stagging API

GraphQL API for RH Stagging (Marinha Portuguesa), running in production with SSO

## Index

1. [🌐 Production](#-production)
2. [🛠️ Development Environment](#️-development-environment)
   - [Dev Container Specs](#-dev-container-specs)
   - [Getting Started](#-getting-started)
3. [📐 Domain Model](#-domain-model)
4. [📦 Tech Stack](#-tech-stack)
5. [📜 License](#-license)

## 🌐 Production

The application is deployed and running in production at:

**http://paas-01.marinha.pt/rh-stagging**

You can test GraphQL API with SSO

**http://paas-01.marinha.pt/rh-stagging/graphql**

For now, it doesn't deliver yet any relevant functionality.

## 🛠️ Development Environment

This project is configured to run in a [Dev Container](https://containers.dev/) environment using Visual Studio Code and the Dev Containers extension. This setup ensures a consistent and reproducible development experience across different machines.

### 📁 Dev Container Specs

The container is defined in `.devcontainer/devcontainer.json` and uses a Docker Compose setup with the following structure:

- **Primary service**: `app`
- **Workspace folder**: `/workspaces/${localWorkspaceFolderBasename}`
- **PostgreSQL** is included and exposed via Docker Compose
- **Post-creation script**: Automatically installs .NET tools (`.devcontainer/script/install-dotnet-tools.sh`)

### 🚀 Getting Started

#### Requirements:

- [Docker](https://docs.docker.com/get-docker/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

#### Steps

1. **Clone the repository**

   ```bash
   git clone https://devops-01.marinha.pt/marinha-si/sigdn-rh-stagging-api.git
   cd sigdn-rh-stagging-api
   ```

2. **Open in VS Code**:

- Press F1 (or Ctrl+Shift+P) and run:
  Dev Containers: Open Folder in Container...Then choose the project folder.

- VS Code will automaically:
  - Build the dev container
  - Mount the project
  - Install tools and extensions

## 📐 Domain Model

The full domain diagram is available here:  
➡️ [Domain model link](docs/diagrams/domain_model.md)
