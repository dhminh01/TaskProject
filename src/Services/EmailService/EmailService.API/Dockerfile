FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/EmailService/EmailService.API/EmailService.API.csproj", "Services/EmailService/EmailService.API/"]
RUN dotnet restore "Services/EmailService/EmailService.API/EmailService.API.csproj"
COPY . .
WORKDIR "/src/Services/EmailService/EmailService.API"
RUN dotnet build "EmailService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailService.API.dll"]
