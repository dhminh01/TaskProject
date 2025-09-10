FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/TaskService/TaskService.API/TaskService.API.csproj", "Services/TaskService/TaskService.API/"]
COPY ["src/Services/TaskService/TaskService.Application/TaskService.Application.csproj", "Services/TaskService/TaskService.Application/"]
COPY ["src/Services/TaskService/TaskService.Domain/TaskService.Domain.csproj", "Services/TaskService/TaskService.Domain/"]
COPY ["src/Services/TaskService/TaskService.Infrastructure/TaskService.Infrastructure.csproj", "Services/TaskService/TaskService.Infrastructure/"]
RUN dotnet restore "Services/TaskService/TaskService.API/TaskService.API.csproj"
COPY . .
WORKDIR "/src/Services/TaskService/TaskService.API"
RUN dotnet build "TaskService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskService.API.dll"]
