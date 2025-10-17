# Padrão de Arquitetura — Projeto de Modernização (VB6 + API C# + PostgreSQL + Windows Service)

Este documento define o padrão de arquitetura, princípios, responsabilidades e fluxos essenciais do projeto “Legacy Bridge”. Serve como base para entendimento, implementação, manutenção e avaliação da prova.

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
  |         \ Observabilidade (Serilog, Health)
  v
[PostgreSQL]         [Windows Service (.NET Worker)]
                          |
                          | FileSystemWatcher (Created/Deleted)
                          v
                       Logs de Sustentação
```

-   VB6: front-end legado que consome REST.
-   API C#: backend com endpoints de clientes e produtos.
-   PostgreSQL: camada de persistência e função de busca (ILIKE).
-   Windows Service: monitoramento de pasta e auditoria.

## Princípios e diretrizes

-   Separação de responsabilidades (UI x API x Dados x Sustentação).
-   Baixo acoplamento, alta coesão; contratos estáveis via DTOs.
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
    -   Forms: VB6/Forms/frmProductAPI.frm (UI e eventos).
    -   Modules: VB6/Modules/modApiConsumer.bas (HTTP), VB6/Modules/modUtils.bas (utilidades).
-   Contratos:
    -   GET /api/clientes?term=texto
    -   GET /api/produtos?clienteId={id}
-   Requisitos:
    -   MSXML2.XMLHTTP, timeout, tratamento de status HTTP, mensagens amigáveis.
    -   Log básico (arquivo) com tempo de resposta e erros.

Fluxo típico (consulta de produtos):

```
[frmProductAPI] --> [modApiConsumer.GET /api/produtos?clienteId]
                     -> JSON
                     -> [modUtils.Parse]
                     -> [frmProductAPI preenche ListView]
```

### 2) API C# (.NET 8) (API/)

-   Objetivo: expor endpoints REST para clientes/produtos e health.
-   Padrão de camadas:
    -   Controllers (ou Minimal API), Services (regras), Repositories (acesso a dados), Models (DTOs).
-   Endpoints (exemplos):
    -   GET /api/health
    -   GET /api/clientes?term=texto -> SELECT \* FROM fn_busca_clientes(term)
    -   GET /api/produtos?clienteId={id} -> retorna [{ id, nome, preco }]
-   Cross-cutting:
    -   Serilog com correlação (X-Correlation-Id).
    -   CORS configurável; rate limiting básico.
    -   Config por appsettings.{Environment}.json e variáveis.

### 3) Banco de Dados (DB/)

-   Objetivo: persistência e função de busca performática.
-   Artefatos:
    -   Tabelas (clientes, produtos, etc.).
    -   Índices (pg_trgm para acelerar ILIKE).
    -   Função fn_busca_clientes(term TEXT, limit, offset).
-   Práticas:
    -   Scripts versionados e idempotentes.
    -   Limites e paginação para requisições de leitura.

### 4) Sustentação / Windows Service (MonitorService/)

-   Objetivo: monitorar pasta de integrações (Created/Deleted) e registrar em .log.
-   Stack: .NET Worker instalado como Windows Service.
-   Config: appsettings.json (FolderPath, filtros, rotação de logs).
-   Resiliência: retries para IO, debounce para flood de eventos.
-   Observabilidade: Serilog em arquivo (rolling), eventos auditáveis.

## Contratos e modelos (API)

-   Cliente: { id, nome, documento, status }
-   Produto: { id, nome, preco }
-   Saúde: GET /health -> 200 OK quando saudável
-   Regras:
    -   Entrada validada e saneada (term, ids).
    -   Respostas com códigos HTTP adequados (200/400/404/500).
    -   Paginação em buscas (limit/offset).

## Operação e observabilidade

-   Logs:
    -   API: estrutura JSON, correlação por requisição.
    -   Service: eventos de arquivos (Created/Deleted/Renamed/Changed) com metadados.
-   Health:
    -   API: /health para liveness/readiness; pode incluir verificação de DB.
-   Métricas (opcional):
    -   Requisições/s, latência p95/p99, erros por endpoint.
    -   Tamanho e taxa de eventos do FileSystemWatcher.
-   Troubleshooting: seguir seção correspondente no README.

## Segurança e conformidade

-   Configuração segura: segredos via variáveis de ambiente/secret manager.
-   CORS restrito ao host do VB6/ambientes necessários.
-   Rate limiting e proteção de entrada (validação/normalização).
-   Acesso controlado à pasta monitorada (ACLs) e rotação de logs.

## Performance e resiliência

-   DB: pg_trgm + ILIKE, índices corretos, limites padrão (ex.: 50).
-   API: timeouts para DB, retry com backoff quando aplicável, caching leve para dados estáticos.
-   VB6: timeout nas requisições, até 3 tentativas com atraso incremental.
-   Service: debounce de eventos para evitar duplicidade.

## Deploy, Docker e ambientes

-   Dev local com Docker Compose: API + PostgreSQL.
-   API containerizada (Dockerfile multi-stage).
-   Variáveis de ambiente para connection strings e CORS.
-   Healthcheck configurado no Compose, quando aplicável.

## CI/CD (sugestão)

-   CI: restore, build, test, análise estática, publish.
-   CD: build/push de imagem, deploy por ambiente, migrações de DB controladas.
-   Gates: testes unitários, qualidade mínima, scan de vulnerabilidades.
-   Versionamento semântico e tags.

## Respostas às questões da prova (mapa de entrega)

-   VB6 (a): rotina MSXML consumindo API e preenchendo ListView
    -   Código/Local: VB6/Modules/modApiConsumer.bas (HTTP), VB6/Forms/frmProductAPI.frm (UI)
    -   Aceite: ListView exibe Nome e Preço dos produtos retornados pela API.
-   VB6 (b) e C# (b) — Função PostgreSQL com ILIKE
    -   Código/Local: DB/functions.sql com fn_busca_clientes(term TEXT, limit, offset)
    -   Aceite: consulta case-insensitive; resultados paginados e ordenados.
-   C# (a): Windows Service monitorando pasta e registrando eventos
    -   Código/Local: MonitorService/ (Worker + FileSystemWatcher + Serilog)
    -   Aceite: logs registram Created/Deleted (arquivo, timestamp, caminho).

## Decisões de arquitetura (ADRs resumidas)

-   Integração gradual preservando VB6: reduz risco e mantém operação.
-   API .NET 8 com camadas claras: facilita testes, manutenção e observabilidade.
-   PostgreSQL com pg_trgm: melhor performance para buscas parciais (ILIKE).
-   Worker como Windows Service: aderência ao ambiente Windows e auditoria por arquivos.

## Checklist de conformidade

-   [ ] API expõe /api/clientes e /api/produtos com DTOs definidos.
-   [ ] Função fn_busca_clientes criada e index pg_trgm aplicado.
-   [ ] VB6 consome endpoints e preenche ListView (Nome/Preço).
-   [ ] Windows Service registra Created/Deleted em log com metadados.
-   [ ] Serilog habilitado (API e Service), health em /health.
-   [ ] Configuração por ambiente e segredos fora do código.
-   [ ] Docker/Compose funcionando para dev.

## Referências internas

-   README.md: instruções de setup, execução e troubleshooting.
-   DB/\*.sql: criação de tabelas, índices e função de busca.
-   API/\*: endpoints, configuração e camadas.
-   MonitorService/\*: serviço de monitoramento e logging.
-   VB6/\*: forms e módulos de consumo da API.
