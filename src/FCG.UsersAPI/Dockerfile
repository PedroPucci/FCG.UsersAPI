FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER app
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copia o NuGet.Config e o pacote local
COPY ["NuGet.config", "/src/nuget.config"]
COPY ["nuget-packages/", "/src/nuget-packages/"]

# Copia os arquivos de projeto
COPY ["src/FCG.UsersAPI/FCG.UsersAPI.csproj", "src/FCG.UsersAPI/"]
COPY ["src/FCG.UsersAPI.Application/FCG.UsersAPI.Application.csproj", "src/FCG.UsersAPI.Application/"]
COPY ["src/FCG.UsersAPI.Domain/FCG.UsersAPI.Domain.csproj", "src/FCG.UsersAPI.Domain/"]
COPY ["src/FCG.UsersAPI.Infrastructure/FCG.UsersAPI.Infrastructure.csproj", "src/FCG.UsersAPI.Infrastructure/"]
COPY ["src/FCG.UsersAPI.Shared/FCG.UsersAPI.Shared.csproj", "src/FCG.UsersAPI.Shared/"]

RUN dotnet restore "src/FCG.UsersAPI/FCG.UsersAPI.csproj" \
    --configfile "/src/nuget.config"

COPY . .

RUN dotnet publish "src/FCG.UsersAPI/FCG.UsersAPI.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.UsersAPI.dll"]