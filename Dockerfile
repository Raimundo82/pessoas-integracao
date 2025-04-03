# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ENV http_proxy: http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy: http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy: "devops-01.marinha.pt, marinha.pt,.marinha.pt, localhost, 10.45.1.165"

WORKDIR /app
COPY ./SigdnRhStaggingApi ./
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

ENV http_proxy: http://proxy-n-wcg.marinha.pt:8080
ENV https_proxy: http://proxy-n-wcg.marinha.pt:8080
ENV no_proxy: "devops-01.marinha.pt, marinha.pt,.marinha.pt, localhost, 10.45.1.165"

WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
CMD ["dotnet", "SigdnRhStaggingApi.dll"]
