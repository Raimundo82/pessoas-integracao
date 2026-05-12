FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble@sha256:0c412724a61b030f27d4ca38a876db27a88c09a7c32344013562710f4117aec3

ENV http_proxy=http://proxy-s-wcg.marinha.pt:8080 \
    https_proxy=http://proxy-s-wcg.marinha.pt:8080 \
    no_proxy="10.50.1.190,.marinha.pt,localhost,host.docker.internal" \
    NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/root-ca-ca.crt

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