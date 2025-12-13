# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj and restore the nuget packages.
COPY /Conflux/Conflux.csproj .
RUN dotnet restore "Conflux.csproj"

# Build the project
COPY /Conflux/ .

# Install NodeJS environment for npm building.
RUN apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_24.x | bash - \
	&& apt-get install -y nodejs
	
RUN npm install
RUN npm run build-prod

# Build the project
RUN dotnet build "Conflux.csproj" -c Release -o /app/build

# Publish app
FROM build AS publish
RUN dotnet publish "Conflux.csproj" -c Release -o /app/publish

# ENTRYPOINT ["tail", "-f", "/dev/null"]

# Finalize image:
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Vite manifest into published wwwroot
COPY --from=publish /src/wwwroot/.vite/manifest.json /app/publish/.vite/manifest.json

ENTRYPOINT ["dotnet", "Conflux.dll"]