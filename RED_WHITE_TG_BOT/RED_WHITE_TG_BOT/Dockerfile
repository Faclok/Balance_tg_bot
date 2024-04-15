#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RED_WHITE_TG_BOT/RED_WHITE_TG_BOT.csproj", "RED_WHITE_TG_BOT/"]
RUN dotnet restore "./RED_WHITE_TG_BOT/RED_WHITE_TG_BOT.csproj"
COPY . .
WORKDIR "/src/RED_WHITE_TG_BOT"
RUN dotnet build "./RED_WHITE_TG_BOT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RED_WHITE_TG_BOT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RED_WHITE_TG_BOT.dll"]