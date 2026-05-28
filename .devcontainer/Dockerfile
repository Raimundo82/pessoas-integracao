FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble@sha256:0c412724a61b030f27d4ca38a876db27a88c09a7c32344013562710f4117aec3

ARG http_proxy
ARG https_proxy
ARG no_proxy

ENV NODE_USE_SYSTEM_CA=1

COPY marinha.crt /usr/local/share/ca-certificates/marinha.crt
COPY root-ca-ca.crt /usr/local/share/ca-certificates/root-ca-ca.crt


RUN update-ca-certificates \
    && rm -f /etc/apt/sources.list.d/yarn.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
    docker.io \
    docker-compose-v2 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*