FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CPMS.API/CPMS.API.csproj", "CPMS.API/"]
COPY ["CPMS.BuildingBlocks/CPMS.BuildingBlocks.csproj", "CPMS.BuildingBlocks/"]
COPY ["CPMS.Core/CPMS.Core.csproj", "CPMS.Core/"]
RUN dotnet restore "CPMS.API/CPMS.API.csproj"
COPY . .
WORKDIR "/src/CPMS.API"
RUN dotnet build "./CPMS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CPMS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CPMS.API.dll"]
