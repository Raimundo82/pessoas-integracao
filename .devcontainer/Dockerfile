FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

ENV http_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy=".marinha.pt,localhost"

COPY marinha.crt /usr/local/share/ca-certificates/marinha.crt
RUN update-ca-certificates

# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1
