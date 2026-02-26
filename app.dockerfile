# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ./Conflux.Domain/Conflux.Domain.csproj ./Conflux/Conflux.Domain/Conflux.Domain.csproj
COPY ./Conflux.Application/Conflux.Application.csproj ./Conflux/Conflux.Application/Conflux.Application.csproj
COPY ./Conflux.Infrastructure/Conflux.Infrastructure.csproj ./Conflux/Conflux.Infrastructure/Conflux.Infrastructure.csproj
COPY ./Conflux.Web/Conflux.Web.csproj ./Conflux/Conflux.Web/Conflux.Web.csproj

RUN dotnet restore ./Conflux/Conflux.Web/Conflux.Web.csproj

COPY ./Conflux.Domain/ ./Conflux/Conflux.Domain/
COPY ./Conflux.Application/ ./Conflux/Conflux.Application/
COPY ./Conflux.Infrastructure/ ./Conflux/Conflux.Infrastructure/
COPY ./Conflux.Web/ ./Conflux/Conflux.Web/

WORKDIR /src/Conflux/Conflux.Web

# Install NodeJS environment for npm building.
RUN apt-get update && \
	apt-get install -y curl && \
	curl -sL https://deb.nodesource.com/setup_24.x | bash - \
	&& apt-get install -y nodejs
	
RUN npm install
RUN npm run build-prod

# Build the project
RUN dotnet build ./Conflux.Web.csproj -c Release -o /app/build

# Publish app
FROM build AS publish
RUN dotnet publish ./Conflux.Web.csproj -c Release -o /app/publish

# Finalize image:
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Vite manifest into published wwwroot
COPY --from=publish /src/Conflux/Conflux.Web/wwwroot/.vite/manifest.json ./wwwroot/.vite/manifest.json

ENTRYPOINT ["dotnet", "Conflux.Web.dll"]