FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Lab8.Enrollment.API/Lab8.Enrollment.API.csproj", "Lab8.Enrollment.API/"]
COPY ["Lab8.Enrollment.Application/Lab8.Enrollment.Application.csproj", "Lab8.Enrollment.Application/"]
COPY ["Lab8.Enrollment.Domain/Lab8.Enrollment.Domain.csproj", "Lab8.Enrollment.Domain/"]
COPY ["Lab8.Enrollment.Infrastructure/Lab8.Enrollment.Infrastructure.csproj", "Lab8.Enrollment.Infrastructure/"]
COPY ["Lab8.Enrollment.Common/Lab8.Enrollment.Common.csproj", "Lab8.Enrollment.Common/"]
COPY ["Lab8.Enrollment.Persistence/Lab8.Enrollment.Persistence.csproj", "Lab8.Enrollment.Persistence/"]
RUN dotnet restore "Lab8.Enrollment.API/Lab8.Enrollment.API.csproj"
COPY . .
WORKDIR "/src/Lab8.Enrollment.API"
RUN dotnet build "Lab8.Enrollment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lab8.Enrollment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lab8.Enrollment.API.dll"]
