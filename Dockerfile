# ============================================================
# Stage 1: Build & publish (MiraiShop.Server + miraishop.client)
#
# MiraiShop.Server.csproj has a ProjectReference to miraishop.client.esproj,
# so a single `dotnet publish` builds the Angular app (via its own npm/ng
# build) and wires the output into wwwroot through the StaticWebAssets
# pipeline automatically. Node.js just needs to be present in this stage.
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

RUN apt-get update && apt-get install -y --no-install-recommends nodejs npm \
    && rm -rf /var/lib/apt/lists/*

COPY . .

RUN dotnet publish MiraiShop.Server/MiraiShop.Server.csproj \
    -c Release \
    -o /app/publish

# ============================================================
# Stage 2: Runtime
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MiraiShop.Server.dll"]
