FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MonitorService.csproj", "./"]
RUN dotnet restore "MonitorService.csproj"
COPY . .
RUN dotnet build "MonitorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MonitorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/monitor /app/logs

# Configurar permissões
RUN chmod 755 /app/monitor /app/logs

ENTRYPOINT ["dotnet", "MonitorService.dll"]
