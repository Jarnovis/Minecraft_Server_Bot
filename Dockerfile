# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY *.csproj ./
RUN dotnet restore

# Copy rest of the source code and build publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Replace Minecraft_Server_Bot.dll with your actual DLL name
ENTRYPOINT ["dotnet", "Minecraft_Server_Bot.dll"]



