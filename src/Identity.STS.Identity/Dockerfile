FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/Identity.STS.Identity/Identity.STS.Identity.csproj", "src/Identity.STS.Identity/"]
COPY ["src/Identity.Admin.EntityFramework.Shared/Identity.Admin.EntityFramework.Shared.csproj", "src/Identity.Admin.EntityFramework.Shared/"]
RUN dotnet restore "src/Identity.STS.Identity/Identity.STS.Identity.csproj"
COPY . .
WORKDIR "/src/src/Identity.STS.Identity"
RUN dotnet build "Identity.STS.Identity.csproj" -c Release -o /app

FROM build AS publish
ARG BUILD_VERSION
RUN dotnet publish "Identity.STS.Identity.csproj" -c Release -o /app -p:Version=$BUILD_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.STS.Identity.dll"]
