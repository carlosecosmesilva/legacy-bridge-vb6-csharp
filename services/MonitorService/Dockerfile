# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivo de projeto e restaurar dependências
COPY ["services/MonitorService/MonitorService.csproj", "services/MonitorService/"]
RUN dotnet restore "services/MonitorService/MonitorService.csproj"

# Copiar todo o código fonte
COPY services/MonitorService/ services/MonitorService/

# Build e publicação
WORKDIR /src/services/MonitorService
RUN dotnet publish "MonitorService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar binários publicados
COPY --from=build /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/monitor /app/logs

# Configurar permissões
RUN chmod 755 /app/monitor /app/logs

# Configurar timezone
ENV TZ=America/Sao_Paulo

# Ponto de entrada
ENTRYPOINT ["dotnet", "MonitorService.dll"]
