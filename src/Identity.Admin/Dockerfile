ARG DOCKER_REGISTRY
FROM $DOCKER_REGISTRY/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM $DOCKER_REGISTRY/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/Identity.Admin/Identity.Admin.csproj", "src/Identity.Admin/"]
COPY ["src/Identity.Admin.EntityFramework.Shared/Identity.Admin.EntityFramework.Shared.csproj", "src/Identity.Admin.EntityFramework.Shared/"]
RUN dotnet restore "src/Identity.Admin/Identity.Admin.csproj"
COPY . .
WORKDIR "/src/src/Identity.Admin"
RUN dotnet build "Identity.Admin.csproj" -c Release -o /app

FROM build AS publish
ARG BUILD_VERSION
RUN dotnet publish "Identity.Admin.csproj" -c Release -o /app -p:Version=$BUILD_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.Admin.dll"]
