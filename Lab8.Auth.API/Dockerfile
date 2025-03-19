FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lab8.Auth.API/Lab8.Auth.API.csproj", "Lab8.Auth.API/"]
COPY ["Lab8.Auth.Application/Lab8.Auth.Application.csproj", "Lab8.Auth.Application/"]
COPY ["Lab8.Auth.Common/Lab8.Auth.Common.csproj", "Lab8.Auth.Common/"]
COPY ["Lab8.Auth.Domain/Lab8.Auth.Domain.csproj", "Lab8.Auth.Domain/"]
COPY ["Lab8.Auth.Infrastructure/Lab8.Auth.Infrastructure.csproj", "Lab8.Auth.Infrastructure/"]
COPY ["Lab8.Auth.Persistence/Lab8.Auth.Persistence.csproj", "Lab8.Auth.Persistence/"]
RUN dotnet restore "Lab8.Auth.API/Lab8.Auth.API.csproj"
COPY . .
WORKDIR "/src/Lab8.Auth.API"
RUN dotnet build "Lab8.Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lab8.Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lab8.Auth.API.dll"]
