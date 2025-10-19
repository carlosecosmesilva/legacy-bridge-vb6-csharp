FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MonitorService/MonitorService.csproj", "MonitorService/"]
RUN dotnet restore "MonitorService/MonitorService.csproj"
COPY . .
WORKDIR "/src/MonitorService"
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
