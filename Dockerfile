# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ENV http_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy=".marinha.pt,localhost,127.0.0.1,.defesa.pt"


WORKDIR /app
COPY ./SigdnRhStaggingApi ./
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

ENV http_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy=http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy=".marinha.pt,localhost,127.0.0.1,.defesa.pt"

WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
CMD ["dotnet", "SigdnRhStaggingApi.dll"]
