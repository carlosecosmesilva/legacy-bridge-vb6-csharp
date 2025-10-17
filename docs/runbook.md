# Runbook — Legacy Bridge (VB6 + API C# + PostgreSQL + Windows Service)

Este runbook descreve procedimentos de operação, sustentação, troubleshooting, e rotina de deploy do projeto.

Sumário:

-   Escopo e visão
-   Componentes e portas
-   Pré-requisitos
-   Operação (subir, parar, reiniciar)
-   Health checks e verificação rápida
-   Logs e observabilidade
-   Configuração e segredos
-   Banco de dados (provisionamento, backup/restore)
-   Troubleshooting
-   Playbooks de incidentes
-   Deploy, validação e rollback
-   Segurança e conformidade
-   Anexos (atalhos de comandos)

---

## 1) Escopo e visão

-   Objetivo: modernização gradual integrando VB6 (legado) a uma API C# e PostgreSQL, com um Windows Service para sustentação (monitoramento de pasta).
-   Módulos:
    -   VB6/Forms + VB6/Modules (cliente e produtos, consumo via MSXML).
    -   API/ (ASP.NET Core/.NET 8 — clientes, produtos, health).
    -   DB/ (scripts SQL e função de busca com ILIKE).
    -   MonitorService/ (Worker .NET como Windows Service, FileSystemWatcher).

---

## 2) Componentes e portas

-   API C#: HTTP em localhost (porta definida no appsettings/Docker).
    -   Endpoint de health: /health
    -   Endpoints principais: /api/clientes, /api/produtos
-   PostgreSQL: 5432 (default)
-   Windows Service: sem porta (monitoramento de filesystem)

Observação: confirme as portas no docker-compose.yml ou em API/appsettings\*.json.

---

## 3) Pré-requisitos

-   Windows 10/11
-   .NET SDK 8.x
-   Docker Desktop (para execução com containers)
-   PostgreSQL (local ou container)
-   Permissões de administrador para gerenciar serviços Windows

---

## 4) Operação

### 4.1 Dev com Docker (API + PostgreSQL)

-   Subir:

    docker compose up -d --build

-   Parar:

    docker compose down

-   Logs:

    docker compose logs -f api

    docker compose logs -f db

-   Health:
    curl "http://localhost:<porta>/health"

### 4.2 Dev local sem Docker

-   Banco de dados (PostgreSQL):

    -   Criar schema e funções (ajuste host/credenciais conforme ambiente):
        psql -h localhost -U postgres -d legacy -f DB/002_create_customers_table.sql
        psql -h localhost -U postgres -d legacy -f DB/functions.sql
        psql -h localhost -U postgres -d legacy -f DB/seed.sql

-   API:
    -   Configure a connection string (variável de ambiente ou appsettings.Development.json).
    -   Executar:
        dotnet restore
        dotnet run --project API

### 4.3 Windows Service (produção/homolog)

-   Rodar em console (teste rápido):

    dotnet run --project MonitorService

-   Publicar e instalar serviço:

    dotnet publish MonitorService -c Release -o .\publish

    sc create LegacyBridgeMonitor binPath= "\"%CD%\publish\MonitorService.exe\"" start= auto

    sc start LegacyBridgeMonitor

-   Parar/Remover:

    sc stop LegacyBridgeMonitor

    sc delete LegacyBridgeMonitor

-   Verificar status:
    sc query LegacyBridgeMonitor

---

## 5) Health checks e verificação rápida

-   API:

    -   GET /health → esperado: 200 (Healthy).

        curl "http://localhost:<porta>/health"

-   DB: testar conexão (psql):

    psql -h localhost -U postgres -d legacy -c "SELECT 1;"

-   VB6: abrir o form e listar produtos/cliente. Esperado: ListView preenchido (Nome, Preço).
-   Service: criar/deletar um arquivo na pasta monitorada. Esperado: evento logado.

---

## 6) Logs e observabilidade

-   API (Serilog):
    -   Console em dev; arquivo em produção (config em appsettings).
    -   Campos recomendados: traceId/correlationId, método, path, statusCode, duração, exceção.
-   Service (Serilog):
    -   Rolling file (ex.: .\logs\integracoes-\*.log).
    -   Campos: ação (Created/Deleted/Renamed/Changed), arquivo, tamanho, timestamp.
-   Coleta/Análise:
    -   Ver logs no console (dev) ou no diretório configurado.
    -   Em incidentes, correlacionar timestamp de API, DB e Service.

---

## 7) Configuração e segredos

-   API (appsettings.\*.json ou variáveis de ambiente):
    -   ConnectionStrings: Default
    -   Cors: AllowedOrigins
    -   Serilog: MinimumLevel
-   Windows Service (MonitorService/appsettings.json):
    -   Pasta monitorada (FolderPath), filtros (Include/Exclude), política de logs.
-   VB6:
    -   API_BASE_URL (em configuração do módulo ou INI/Registry).
-   Boas práticas:
    -   Não versionar segredos.
    -   Em produção, usar variáveis de ambiente/Secret Manager/Vault.
-   Exemplos (PowerShell):
    $env:ConnectionStrings\_\_Default = "Host=localhost;Port=5432;Database=legacy;Username=postgres;Password=postgres"
    $env:Cors\_\_AllowedOrigins = "http://localhost;http://localhost:3000"

---

## 8) Banco de dados

### 8.1 Provisionamento

-   Extensão:

    psql -h localhost -U postgres -d legacy -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"

-   Tabelas e função:

    psql -h localhost -U postgres -d legacy -f DB/002_create_customers_table.sql

    psql -h localhost -U postgres -d legacy -f DB/functions.sql

-   Carga inicial (opcional):
    psql -h localhost -U postgres -d legacy -f DB/seed.sql

### 8.2 Backup/Restore

-   Backup:

    pg_dump -h localhost -U postgres -d legacy -F c -f backup_legacy.dump

-   Restore:
    pg_restore -h localhost -U postgres -d legacy -c -1 backup_legacy.dump

### 8.3 Verificações

-   Função de busca:

    psql -h localhost -U postgres -d legacy -c "SELECT \* FROM fn_busca_clientes('jo', 10, 0);"

-   Índice trigram:
    psql -h localhost -U postgres -d legacy -c "\d+ clientes"

---

## 9) Troubleshooting

-   API não inicia (sem Docker):

    -   Verificar porta ocupada:

        netstat -ano | findstr :5000

    -   Conferir ConnectionStrings e permissões no PostgreSQL.
    -   Rodar com logs detalhados (ASPNETCORE_ENVIRONMENT=Development).

-   API no Docker sem acesso ao DB:

    -   Validar rede do compose e variáveis de ambiente.
    -   Garantir que o serviço db está saudável (docker compose ps; logs).

-   Erros de CORS:

    -   Ajustar Cors.AllowedOrigins no appsettings ou via env.
    -   Confirmar origem do VB6 (se houver controle Web embutido).

-   VB6 sem resposta:

    -   Testar endpoint diretamente com curl/Postman.
    -   Checar MSXML2.XMLHTTP instalado/registrado.
    -   Verificar URL base e timeout.

-   Service não loga eventos:

    -   Confirmar FolderPath e permissões da pasta (ACL).
    -   Verificar Services.msc e Event Viewer (Application log).
    -   Checar se o FileSystemWatcher tem filtros corretos.

-   DB performance lenta em buscas:
    -   Garantir extensão pg_trgm e índice GIN em clientes.nome.
    -   Revisar LIMIT/OFFSET e term normalizado.

---

## 10) Playbooks de incidentes

-   API 5xx generalizado:

    -   Ação: coletar logs da API, checar /health, testar conectividade com DB.
    -   Mitigação: reiniciar API; se persistir, fallback para modo degradado (limites mais baixos, desabilitar funcionalidades não críticas).
    -   Pós: abrir incidente, anexar logs e timeline.

-   DB indisponível:

    -   Ação: validar serviço PostgreSQL; checar espaço em disco.
    -   Mitigação: reiniciar serviço do DB; considerar restore se corrupção for detectada.
    -   Pós: revisar alertas, ajustar limites de conexões e timeouts.

-   Service sem eventos:

    -   Ação: criar arquivo de teste na pasta; revisar logs/permissions.
    -   Mitigação: reiniciar serviço; ajustar debounce/filtros.
    -   Pós: documentar causa (ex.: antivírus bloqueando).

-   CORS bloqueando VB6:
    -   Ação: revisar origem; ajustar AllowedOrigins.
    -   Mitigação: liberar localhost temporariamente.
    -   Pós: restringir novamente conforme necessidade.

---

## 11) Deploy, validação e rollback

-   Pré-deploy (checklist):

    -   [ ] CI verde (build/test).
    -   [ ] Scripts DB aplicados em staging.
    -   [ ] Variáveis de ambiente definidas (ConnectionStrings, CORS).
    -   [ ] Backup do DB atualizado.
    -   [ ] Janela aprovada e comunicação feita.

-   Deploy API:

    -   Docker:

        docker compose pull && docker compose up -d --no-deps --build api

    -   Sem Docker:

        dotnet publish API -c Release -o .\out

        dotnet .\out\Api.dll

-   Deploy Service:

    sc stop LegacyBridgeMonitor

    xcopy /Y /E .\publish "C:\Program Files\LegacyBridgeMonitor"

    sc start LegacyBridgeMonitor

-   Validação pós-deploy:

    -   [ ] /health = 200
    -   [ ] /api/clientes e /api/produtos respondem
    -   [ ] Logs sem erros anormais
    -   [ ] Service registra Created/Deleted

-   Rollback:
    -   API: retornar à tag/artefato anterior (compose ou pasta publish anterior).
    -   DB: aplicar rollback/migração inversa ou restaurar backup (se necessário).
    -   Service: reinstalar binário anterior e reiniciar.

---

## 12) Segurança e conformidade

-   Segredos: apenas via variáveis de ambiente/secret store.
-   Princípio do menor privilégio (DB e filesystem).
-   Rate limiting básico na API.
-   Rotação de logs e retenção conforme política.
-   Certificados HTTPS confiáveis em produção (se aplicável).

---

## 13) Anexos — Atalhos de comandos

-   Curl (health):

    curl "http://localhost:<porta>/health"

-   Consultas API:

    curl "http://localhost:<porta>/api/clientes?term=jo"

    curl "http://localhost:<porta>/api/produtos?clienteId=1"

-   PowerShell (variáveis):

    $env:ASPNETCORE_ENVIRONMENT = "Development"

    $env:ConnectionStrings\_\_Default = "Host=localhost;Port=5432;Database=legacy;Username=postgres;Password=postgres"

-   PostgreSQL:

    psql -h localhost -U postgres -d legacy -c "SELECT COUNT(\*) FROM clientes;"

-   Windows Service:
    sc query LegacyBridgeMonitor
