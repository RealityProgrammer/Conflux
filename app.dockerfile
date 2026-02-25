# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project
COPY . ./Conflux

WORKDIR /src/Conflux/Conflux.Web

# Install NodeJS environment for npm building.
RUN apt-get update && \
	apt-get install -y curl && \
	curl -sL https://deb.nodesource.com/setup_24.x | bash - \
	&& apt-get install -y nodejs
	
RUN npm install
RUN npm run build-prod

# Restore
RUN dotnet restore ./Conflux.Web.csproj

# Build the project
RUN dotnet build ./Conflux.Web.csproj -c Release -o /app/build

# Publish app
FROM build AS publish
RUN dotnet publish ./Conflux.Web.csproj -c Release -o /app/publish

# ENTRYPOINT ["tail", "-f", "/dev/null"]

# Finalize image:
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Vite manifest into published wwwroot
COPY --from=publish /src/Conflux/Conflux.Web/wwwroot/.vite/manifest.json ./wwwroot/.vite/manifest.json

ENTRYPOINT ["dotnet", "Conflux.Web.dll"]