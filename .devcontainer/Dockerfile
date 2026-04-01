FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble

ENV http_proxy=http://proxy-n-wcg.marinha.pt:8080 \
    https_proxy=http://proxy-n-wcg.marinha.pt:8080 \
    no_proxy=".marinha.pt,localhost,host.docker.internal"

COPY marinha.crt /usr/local/share/ca-certificates/marinha.crt

RUN update-ca-certificates \
    && rm -f /etc/apt/sources.list.d/yarn.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
    docker.io \
    docker-compose-v2 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*