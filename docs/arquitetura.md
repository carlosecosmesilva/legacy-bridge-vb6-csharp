# Padrão de Arquitetura — Projeto de Modernização (VB6 + API C# + PostgreSQL + Windows Service)

Este documento define o padrão de arquitetura, princípios, responsabilidades e fluxos essenciais do projeto “Legacy Bridge”. Serve como base para entendimento, implementação, manutenção e evolução do sistema.

## Objetivos de arquitetura

-   Modernizar gradualmente sem interromper o legado VB6.
-   Separar responsabilidades entre UI legado, API, dados e sustentação.
-   Otimizar buscas e I/O com foco em performance e observabilidade.
-   Sustentação orientada a logs estruturados, health checks e automação.

## Diagrama lógico (alto nível)

```
[Operador (VB6)]
      |
      | HTTP (MSXML) - JSON
      v
[API C# (.NET 8)]
  |        \
  |         \ Observabilidade (Serilog + /health)
  v
[PostgreSQL]         [Windows Service (.NET Worker)]
                          |
                          | FileSystemWatcher (Created/Changed/Renamed/Deleted)
                          v
                       Logs de Sustentação
```

-   VB6: front-end legado que consome REST.
-   API C#: backend com endpoints de customers e products.
-   PostgreSQL: camada de persistência e função de busca (ILIKE).
-   Windows Service: monitoramento de pasta e auditoria.

## Princípios e diretrizes

-   Separação de responsabilidades (UI x API x Dados x Sustentação).
-   Baixo acoplamento, alta coesão; contratos estáveis via DTOs/Contracts.
-   Observabilidade por padrão: logs estruturados, health checks.
-   Resiliência: timeouts, retries controlados, paginação e limites.
-   Segurança básica: CORS restrito, inputs validados, segredos fora do código.
-   Configuração por ambiente (variáveis/appsettings); sem segredos no repositório.
-   Automação: Docker/Compose para dev, scripts de DB versionados.
-   Documentação viva: README, este documento e OpenAPI/Swagger.

## Módulos e responsabilidades

### 1) Legado VB6 (VB6/)

-   Objetivo: consumir a API moderna e exibir dados em ListView.
-   Estrutura:
    -   Forms: VB6/Forms/frmMain.frm, VB6/Forms/frmCustomers.frm, VB6/Forms/frmProductAPI.frm (UI e eventos).
    -   Modules: VB6/Modules/modApi.bas (HTTP via MSXML), VB6/Modules/JsonConverter.bas (parser), VB6/Modules/modUtils.bas (utilidades).
-   Contratos (principais):
    -   GET /api/customers/search?term={texto}
    -   GET /api/products
-   Requisitos:
    -   MSXML2.XMLHTTP, timeout, tratamento de status HTTP, mensagens amigáveis.
    -   Log básico (arquivo) com tempo de resposta e erros.

Fluxo típico (consulta de produtos):

```
[frmProductAPI] --> [modApi.GET /api/products]
                    -> JSON
                    -> [JsonConverter]
                    -> [frmProductAPI preenche ListView]
```

### 2) API C# (.NET 8) (Api/)

-   Objetivo: expor endpoints REST para customers/products e health.
-   Padrão de camadas:
    -   Controllers, Services (regras) + Interfaces, Repositories (dados) + Interfaces,
        Models (entidades), DTOs, Contracts (Requests/Responses), Mappings (AutoMapper),
        Extensions (bootstrapping), Middleware (ex.: ExceptionHandlingMiddleware).
-   Endpoints (exemplos):
    -   GET /health
    -   GET /api/customers
    -   GET /api/customers/{id}
    -   GET /api/customers/search?term={texto}
    -   POST /api/customers
    -   PUT /api/customers/{id}
    -   DELETE /api/customers/{id}
    -   GET /api/products
    -   GET /api/products/{id}
    -   POST /api/products
    -   PUT /api/products/{id}
    -   DELETE /api/products/{id}
-   Cross-cutting:
    -   Serilog com correlação (X-Correlation-Id) e logs estruturados em arquivo/console.
    -   Health check em /health (incluindo verificação de DB quando aplicável).
    -   CORS configurável; rate limiting básico (opcional).
    -   Config por appsettings.{Environment}.json e variáveis de ambiente.

### 3) Banco de Dados (Db/)

-   Objetivo: persistência e função de busca performática.
-   Artefatos:
    -   Tabelas (customers, products, etc.).
    -   Índices (pg_trgm para acelerar ILIKE).
    -   Função search_customers_by_name(term TEXT, limit INT, offset INT).
    -   Migrações EF Core em Api/Migrations.
-   Práticas:
    -   Scripts versionados (Db/legacy/\*.sql) e idempotentes quando possível.
    -   Limites e paginação para requisições de leitura.
    -   Uso de migrações EF Core para evolução do schema da API.

### 4) Sustentação / Windows Service (MonitorService/)

-   Objetivo: monitorar pasta de integrações (Created/Changed/Renamed/Deleted) e registrar em .log.
-   Stack: .NET Worker instalado como Windows Service.
-   Config: appsettings.json (FileWatcher: MonitorPath, filtros de inclusão/exclusão, pastas de Archive/Error).
-   Resiliência: retries para IO, debounce para flood de eventos.
-   Observabilidade: Serilog em arquivo (rolling), eventos auditáveis e health interno via logs.

## Contratos e modelos (API)

-   Customer: { id, name, document, active, created_at }
-   Product: { id, name, price, active, created_at }
-   Saúde: GET /health -> 200 OK quando saudável (inclui status do PostgreSQL quando configurado)
-   Regras:
    -   Entrada validada e saneada (term, ids, payloads de criação/atualização).
    -   Respostas com códigos HTTP adequados (200/201/204/400/404/500).
    -   Paginação em buscas (limit/offset) e ordenação estável quando aplicável.

## Operação e observabilidade

-   Logs:
    -   API: estrutura JSON, correlação por requisição (X-Correlation-Id) e logs em Api/Logs.
    -   Service: eventos de arquivos (Created/Deleted/Renamed/Changed) com metadados, logs em MonitorService/Logs.
-   Health:
    -   API: /health para liveness/readiness; inclui verificação de DB quando configurado.
-   Métricas (opcional):
    -   Requisições/s, latência p95/p99, erros por endpoint (API).
    -   Tamanho e taxa de eventos do FileSystemWatcher; tempo médio de processamento por arquivo (Service).
-   Troubleshooting: seguir seção correspondente no README.

## Segurança e conformidade

-   Configuração segura: segredos via variáveis de ambiente/secret manager.
-   CORS restrito ao host do VB6/ambientes necessários.
-   Rate limiting e proteção de entrada (validação/normalização).
-   Acesso controlado à pasta monitorada (ACLs) e rotação de logs.

## Performance e resiliência

-   DB: pg_trgm + ILIKE, índices corretos, limites padrão (ex.: 50) e planos de execução monitorados.
-   API: timeouts para DB, retry com backoff quando aplicável, caching leve para dados estáticos (opcional).
-   VB6: timeout nas requisições, até 3 tentativas com atraso incremental.
-   Service: debounce de eventos para evitar duplicidade e política de retry para IO.

## Deploy, Docker e ambientes

-   Dev local com Docker Compose: API + PostgreSQL + MonitorService.
-   API containerizada (Dockerfile multi-stage).
-   Variáveis de ambiente para connection strings e CORS.
-   Healthcheck configurado no Compose, quando aplicável.

## CI/CD (sugestão)

-   CI: restore, build, test, análise estática, publish.
-   CD: build/push de imagem, deploy por ambiente, migrações de DB controladas.
-   Gates: testes unitários (Api.Tests), qualidade mínima, scan de vulnerabilidades.
-   Versionamento semântico e tags.

<!-- Seção removida: respostas a questões da prova não fazem parte da documentação técnica do projeto. -->

## Decisões de arquitetura (ADRs resumidas)

-   Integração gradual preservando VB6: reduz risco e mantém operação.
-   API .NET 8 com camadas claras (Controllers/Services/Repositories/DTOs/Contracts/Mappings): facilita testes, manutenção e observabilidade.
-   PostgreSQL com pg_trgm: melhor performance para buscas parciais (ILIKE) via função search_customers_by_name.
-   Worker como Windows Service: aderência ao ambiente Windows e auditoria por arquivos.

## Checklist de conformidade

-   [ ] API expõe /api/customers e /api/products, e /api/customers/search com DTOs definidos.
-   [ ] Função search_customers_by_name criada e índice pg_trgm aplicado.
-   [ ] VB6 consome endpoints e preenche ListViews (Customers/Products).
-   [ ] Windows Service registra Created/Changed/Renamed/Deleted em log com metadados.
-   [ ] Serilog habilitado (API e Service), health em /health.
-   [ ] Configuração por ambiente e segredos fora do código.
-   [ ] Docker/Compose funcionando para dev (API + DB + MonitorService).

## Referências internas

-   README.md: instruções de setup, execução e troubleshooting.
-   Db/legacy/\*.sql: criação de tabelas, índices e função de busca; Dump e Backup.
-   Api/\*: endpoints, configuração e camadas (Contracts, DTOs, Mappings, Extensions, Migrations).
-   Api.Tests/\*: testes de unidade.
-   MonitorService/\*: serviço de monitoramento e logging.
-   VB6/\*: forms e módulos de consumo da API.
