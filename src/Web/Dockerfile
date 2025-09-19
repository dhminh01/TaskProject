# Build stage
FROM node:20-alpine as node-build

# Set working directory
WORKDIR /app

# Copy package files from src/Web directory
COPY src/Web/package*.json ./

# Install dependencies with exact versions and include dev dependencies
RUN npm install

# Copy project files from src/Web directory
COPY src/Web/ .

# Build the application
RUN npm run build

FROM node-build as publish

# Production stage
FROM nginx:stable-alpine as production

# Copy built assets from build stage
COPY --from=publish /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY src/Web/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]