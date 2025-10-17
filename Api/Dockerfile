# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos do projeto e restaurar dependências
COPY ["Api.csproj", "./"]
RUN dotnet restore "Api.csproj"

# Copiar todo o código e compilar
COPY . .
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expor porta 80 (HTTP)
EXPOSE 80

# Copiar binários publicados
COPY --from=build /app/publish .

# Configurar timezone (opcional, mas útil para logs)
ENV TZ=America/Sao_Paulo

# Ponto de entrada
ENTRYPOINT ["dotnet", "Api.dll"]

