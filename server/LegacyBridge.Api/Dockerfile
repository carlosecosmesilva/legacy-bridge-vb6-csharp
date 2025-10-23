# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto na ordem correta (dependências primeiro)
COPY ["server/LegacyBridge.Domain/LegacyBridge.Domain.csproj", "server/LegacyBridge.Domain/"]
COPY ["server/LegacyBridge.Application/LegacyBridge.Application.csproj", "server/LegacyBridge.Application/"]
COPY ["server/LegacyBridge.Infrastructure/LegacyBridge.Infrastructure.csproj", "server/LegacyBridge.Infrastructure/"]
COPY ["server/LegacyBridge.Api/LegacyBridge.Api.csproj", "server/LegacyBridge.Api/"]

# Restaurar dependências
RUN dotnet restore "server/LegacyBridge.Api/LegacyBridge.Api.csproj"

# Copiar todo o código fonte
COPY server/LegacyBridge.Domain/ server/LegacyBridge.Domain/
COPY server/LegacyBridge.Application/ server/LegacyBridge.Application/
COPY server/LegacyBridge.Infrastructure/ server/LegacyBridge.Infrastructure/
COPY server/LegacyBridge.Api/ server/LegacyBridge.Api/

# Build e publicação
WORKDIR /src/server/LegacyBridge.Api
RUN dotnet publish "LegacyBridge.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expor portas
EXPOSE 80
EXPOSE 443

# Copiar binários publicados
COPY --from=build /app/publish .

# Configurar timezone (opcional, mas útil para logs)
ENV TZ=America/Sao_Paulo
ENV ASPNETCORE_URLS=http://+:80

# Criar diretório de logs
RUN mkdir -p /app/Logs

# Ponto de entrada
ENTRYPOINT ["dotnet", "LegacyBridge.Api.dll"]

