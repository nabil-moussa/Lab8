FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lab8.Course.API/Lab8.Course.API.csproj", "Lab8.Course.API/"]
COPY ["Lab8.Course.Infrastructure/Lab8.Course.Infrastructure.csproj", "Lab8.Course.Infrastructure/"]
COPY ["Lab8.Course.Common/Lab8.Course.Common.csproj", "Lab8.Course.Common/"]
COPY ["Lab8.Course.Domain/Lab8.Course.Domain.csproj", "Lab8.Course.Domain/"]
COPY ["Lab8.Course.Application/Lab8.Course.Application.csproj", "Lab8.Course.Application/"]
COPY ["Lab8.Course.Persistence/Lab8.Course.Persistence.csproj", "Lab8.Course.Persistence/"]
RUN dotnet restore "Lab8.Course.API/Lab8.Course.API.csproj"
COPY . .
WORKDIR "/src/Lab8.Course.API"
RUN dotnet build "Lab8.Course.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lab8.Course.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lab8.Course.API.dll"]
