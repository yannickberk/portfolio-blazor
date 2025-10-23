# Multi-stage build for Blazor WebAssembly static site
# 1) Build/publish using the .NET SDK
# 2) Serve the resulting static files using a minimal nginx image

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js (required by project npm scripts during publish) and other build deps
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
  ca-certificates \
  curl \
  gnupg \
  && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# Copy project files and restore/build
COPY . .
# Install npm dependencies so project scripts (sass build) are available during publish
RUN npm ci --no-audit --no-fund || npm install --no-audit --no-fund
RUN dotnet restore "BlazorApp.csproj"
RUN dotnet publish "BlazorApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

### Runtime image
FROM nginx:1.29.2-alpine3.22 AS runtime

# Ensure base packages are updated to pick up security patches and install curl for healthchecks
USER root
RUN apk update && apk upgrade --no-cache \
  && apk add --no-cache curl ca-certificates \
  && update-ca-certificates || true

# Replace default nginx config with SPA-friendly config that also sets basic security headers
COPY nginx.conf /etc/nginx/nginx.conf

# Copy published wwwroot content from build stage
COPY --from=build /app/publish/wwwroot/ /usr/share/nginx/html/

# Ensure runtime nginx dirs exist and set proper ownership/permissions so nginx master can create/use temp files.
# Creating dirs and chown them while running as root prevents permission denied errors when nginx starts.
RUN mkdir -p /var/cache/nginx/client_temp /var/run/nginx \
    && chown -R nginx:nginx /usr/share/nginx/html /var/cache/nginx /var/run/nginx \
    && chmod -R a+rX /usr/share/nginx/html

# Expose HTTP port
EXPOSE 80

# Basic healthcheck using curl (more commonly available and works well with rootless Podman)
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD curl --fail --silent http://localhost/ || exit 1

# Do not switch to USER nginx here. The nginx master needs to start with root privileges
# so it can bind to port 80 and create required directories; worker processes will drop to
# the 'nginx' user as configured in /etc/nginx/nginx.conf.
ENTRYPOINT ["nginx", "-g", "daemon off;"]