FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5009

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

COPY ["src/Services/EmailService/EmailService.API/EmailService.API.csproj", "src/Services/EmailService/EmailService.API/"]
COPY ["src/Services/TaskService/TaskService.Domain/TaskService.Domain.csproj", "src/Services/TaskService/TaskService.Domain/"]
COPY ["src/_schema/proto/task_notification.proto", "src/_schema/proto/"]
COPY ["src/Services/EmailService/EmailService.API/.env", "src/Services/EmailService/EmailService.API/"]
COPY ["src/Services/EmailService/EmailService.API/token.json", "src/Services/EmailService/EmailService.API/token.json"]

RUN dotnet restore "src/Services/EmailService/EmailService.API/EmailService.API.csproj"

COPY . .
WORKDIR "/src/src/Services/EmailService/EmailService.API"
RUN dotnet build "EmailService.API.csproj" -c Release
RUN dotnet publish "EmailService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/src/Services/EmailService/EmailService.API/token.json/ ./token.json/

# Set ASP.NET Core environment
ENV ASPNETCORE_URLS=http://+:5009
ENV ASPNETCORE_ENVIRONMENT=Development

# RabbitMQ Configuration
ENV RabbitMQ__Host=rabbitmq
ENV RabbitMQ__Username=guest
# ENV RabbitMQ__Password=guest

ENTRYPOINT ["dotnet", "EmailService.API.dll"]
