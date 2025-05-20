FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CPMS.Proxy/CPMS.Proxy.csproj", "CPMS.Proxy/"]
COPY ["CPMS.BuildingBlocks/CPMS.BuildingBlocks.csproj", "CPMS.BuildingBlocks/"]
COPY ["CPMS.Core/CPMS.Core.csproj", "CPMS.Core/"]
RUN dotnet restore "CPMS.Proxy/CPMS.Proxy.csproj"
COPY . .
WORKDIR "/src/CPMS.Proxy"
RUN dotnet build "./CPMS.Proxy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CPMS.Proxy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CPMS.Proxy.dll"]
