# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:f061e5a7532b36fa1d1b684857fe1f504ba92115b9934f154643266613c44c62 AS build

ENV http_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy=".marinha.pt,localhost,127.0.0.1,.defesa.pt"


WORKDIR /app
COPY ./SigdnRhStaggingApi ./
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:ccdca44cd4f256d50187f920dc8ccc2a9ea7a8a4597ac1d51e08fddb2e3b3205 AS runtime

ENV http_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy=".marinha.pt,localhost,127.0.0.1,.defesa.pt"

WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
CMD ["dotnet", "SigdnRhStaggingApi.dll"]
