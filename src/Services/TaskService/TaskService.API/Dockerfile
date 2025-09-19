FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5008

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/Services/TaskService/TaskService.API/TaskService.API.csproj", "src/Services/TaskService/TaskService.API/"]
COPY ["src/Services/TaskService/TaskService.Application/TaskService.Application.csproj", "src/Services/TaskService/TaskService.Application/"]
COPY ["src/Services/TaskService/TaskService.Domain/TaskService.Domain.csproj", "src/Services/TaskService/TaskService.Domain/"]
COPY ["src/Services/TaskService/TaskService.Infrastructure/TaskService.Infrastructure.csproj", "src/Services/TaskService/TaskService.Infrastructure/"]

# Restore packages
RUN dotnet restore "src/Services/TaskService/TaskService.API/TaskService.API.csproj"

# Copy all source code
COPY . .
# Change to the API project directory and publish using relative path
WORKDIR "/src/src/Services/TaskService/TaskService.API"
RUN dotnet build "TaskService.API.csproj" -c Release
RUN dotnet publish "TaskService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set ASP.NET Core environment
ENV ASPNETCORE_URLS=http://+:5008
ENV ASPNETCORE_ENVIRONMENT=Development

# RabbitMQ Configuration
ENV RabbitMQ__Host=rabbitmq
ENV RabbitMQ__Username=guest
# ENV RabbitMQ__Password=guest

ENTRYPOINT ["dotnet", "TaskService.API.dll"]
