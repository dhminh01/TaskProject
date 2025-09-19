FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

COPY ["src/Gateway/Gateway.API/Gateway.API.csproj", "src/Gateway/Gateway.API/"]

RUN dotnet restore "src/Gateway/Gateway.API/Gateway.API.csproj"

COPY . .
WORKDIR "/src/src/Gateway/Gateway.API"

RUN dotnet build "Gateway.API.csproj" -c Release
RUN dotnet publish "Gateway.API.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set ASP.NET Core environment
ENV ASPNETCORE_URLS=http://+:5000,https://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Gateway.API.dll"]