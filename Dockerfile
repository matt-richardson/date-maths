# Multi-stage build
# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /app/DateMaths
COPY DateMaths/ .
RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Setup Node.js environment
FROM node:18-alpine AS node-build
WORKDIR /app
COPY package*.json ./
RUN npm install --production

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app

# Install Node.js and wget for health check in the final image
RUN apk add --no-cache nodejs npm wget

# Copy .NET published app
COPY --from=dotnet-build /app/publish ./DateMaths/
RUN chmod +x ./DateMaths/date-maths

# Copy Node.js app and dependencies
COPY --from=node-build /app/node_modules ./node_modules/
COPY server.js ./
COPY index.html ./
COPY package.json ./

# Create a non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001
USER nodejs

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1

# Start the Node.js server
CMD ["node", "server.js"]