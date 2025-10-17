# Legacy Bridge

Modernização que integra um sistema legado VB6 com uma API moderna em C# e banco PostgreSQL.

-   Integração gradual: VB6 consome a API via HTTP (MSXML).
-   Backend em ASP.NET Core com camadas de Controllers, Services e Repositories.
-   Persistência no PostgreSQL (inclui função de busca com ILIKE).
-   Serviço de monitoramento (Windows Service) baseado em .NET Worker.

## Estrutura do projeto

```
/ProjetoModernizacao
│
├── API/                        # Backend moderno em C#
│   ├── Controllers/            # Endpoints (ClientesController, ProdutosController)
│   ├── Services/               # Regras de negócio (ClienteService, ProdutoService)
│   ├── Repositories/           # Acesso a dados (chamadas à função PostgreSQL)
│   ├── Models/                 # DTOs e entidades
│   ├── Config/                 # Configurações (Serilog, CORS, HealthChecks)
│   └── Program.cs              # Bootstrap da API (Minimal API ou ASP.NET Core)
│
├── DB/                         # Scripts SQL
│   ├── 001_create_schema.sql
│   ├── 002_create_customers_table.sql
│   ├── 003_create_search_function.sql
│   ├── 004_create_products_table.sql
│   └── 005_seed.sql            # Dados de exemplo
│
├── VB6/                        # Projeto legado
│   ├── Forms/                  # Telas
│   │   └── frmProductAPI.frm   # Form principal com ListView
│   └── Modules/                # Módulos (.bas)
│       ├── modApiConsumer.bas  # Rotinas MSXML para consumir API
│       └── modUtils.bas        # Funções auxiliares (parse JSON, logs)
│
├── MonitorService/             # Windows Service (C#)
│   ├── FileWatcherService.cs   # FileSystemWatcher
│   ├── appsettings.json        # Caminho monitorado, filtros
│   └── Program.cs              # Bootstrap do Worker/Service
│
├── docker-compose.yml          # Sobe API + PostgreSQL em dev
├── Dockerfile                  # Build da API C#
├── LICENSE                     # Licença MIT
├── README.md                   # Documentação do projeto
└── docs/
    ├── ARQUITETURA.md          # Diagrama lógico + narrativa
    ├── ci-cd.md                # Pipeline de build/deploy
    └── runbook.md              # Operação e sustentação
```

## Requisitos

    Visual Studio 2022 (recomendado) ou Visual Studio Code

## Comece rápido

### Opção 1: Visual Studio (Recomendado)

1. Clone o repositório

```bash
git clone https://github.com/carlosecosmesilva/legacy-bridge-vb6-csharp.git
cd legacy-bridge-vb6-csharp
```

2. Abra a Solution

```powershell
# Abrir no Visual Studio
start LegacyBridge.sln

# Ou via linha de comando
devenv LegacyBridge.sln
```

3. Restaurar pacotes NuGet

-   Visual Studio restaura automaticamente
-   Ou clique direito na Solution → **Restore NuGet Packages**

4. Executar

-   Pressione `F5` para debug
-   Ou `Ctrl+F5` para executar sem debug

### Opção 2: Docker (Rápido)

1. Clone o repositório

```bash
git clone https://github.com/carlosecosmesilva/legacy-bridge-vb6-csharp.git
cd legacy-bridge-vb6-csharp
```

2. Suba com Docker (API + PostgreSQL)

```bash
docker compose up -d --build
```

-   Verifique as portas mapeadas no arquivo docker-compose.yml para acessar a API (ex.: http://localhost:<porta>).
-   Os scripts em DB/ podem ser aplicados automaticamente conforme configuração do compose; caso contrário, veja a seção Banco de Dados.

3. Teste a API

```bash
curl "http://localhost:<porta>/health"
```

Resposta esperada: Healthy.

## Executar sem Docker (dev local)

-   Banco de dados:
    -   Suba um PostgreSQL local ou via container.
    -   Crie o banco e rode os scripts:
        ```bash
        psql -h localhost -U postgres -c "CREATE DATABASE legacy;"
        psql -h localhost -U postgres -d legacy -f "Db\000_init.sql"
        ```
-   API:
    -   Configure a connection string em API/appsettings.Development.json (ou variáveis de ambiente):
        ```json
        {
        	"ConnectionStrings": {
        		"Default": "Host=localhost;Port=5432;Database=legacy;Username=postgres;Password=postgres"
        	},
        	"Serilog": { "MinimumLevel": "Information" },
        	"Cors": {
        		"AllowedOrigins": ["http://localhost:3000", "http://localhost"]
        	}
        }
        ```
    -   Execute:
        ```bash
        dotnet restore
        dotnet run --project API
        ```

## Endpoints (exemplos)

-   Clientes
    -   GET /api/clientes?term=joao
    -   GET /api/clientes/{id}
    -   POST /api/clientes
    -   PUT /api/clientes/{id}
    -   DELETE /api/clientes/{id}
-   Produtos
    -   GET /api/produtos
    -   GET /api/produtos/{id}

Exemplo de busca usando a função de ILIKE no PostgreSQL (via repository):

```bash
curl "http://localhost:<porta>/api/clientes?term=jo"
```

Health checks:

```bash
GET /health
```

## Banco de Dados

-   Scripts em DB/ definem schema, tabelas, índices, função de busca (fn_busca_clientes) e dados de exemplo.
-   Estrutura:
    -   001_create_schema.sql: cria schema app e extensão pg_trgm
    -   002_create_customers_table.sql: tabela de clientes com índices
    -   003_create_search_function.sql: função fn_busca_clientes (ILIKE)
    -   004_create_products_table.sql: tabela de produtos
    -   005_seed.sql: dados iniciais para desenvolvimento
-   Em produção, aplicar via migrations/tooling de banco ou pipeline de CI/CD (ver docs/ci-cd.md).

## Aplicação VB6 (legado)

-   Atualize a URL base da API em VB6/Modules/modApiConsumer.bas (ex.: API_BASE_URL).
-   Abra VB6/Forms/frmProductAPI.frm no VB6, compile e execute.

### Forms

-   frmProductAPI.frm
    -   Lista de produtos (ListView) e ações de consulta/inclusão/edição via API.
    -   Consome funções expostas nos módulos para chamadas HTTP e parsing.

### Módulos

-   modApiConsumer.bas
    -   Rotinas MSXML para GET/POST/PUT/DELETE.
    -   Funções para montar headers e tratar status/erros.
-   modUtils.bas

    -   Auxiliares de JSON, logs e conversões de tipos/strings.

-   Requisitos comuns:
    -   MSXML6 instalado para chamadas HTTP.
    -   CORS habilitado na API para o host de execução, se houver chamadas a partir de um controle Web.

## Windows Service (MonitorService)

-   Configuração: MonitorService/appsettings.json
    -   Ex.: FolderPath, IncludeFilter, ExcludeFilter, ProcessingMode.
-   Executar em modo console (dev):
    ```bash
    dotnet run --project MonitorService
    ```
-   Publicar e instalar como serviço no Windows:
    ```bash
    dotnet publish MonitorService -c Release -o .\publish
    sc create LegacyBridgeMonitor binPath= "\"%CD%\publish\MonitorService.exe\"" start= auto
    sc start LegacyBridgeMonitor
    ```
    Para remover:
    ```bash
    sc stop LegacyBridgeMonitor
    sc delete LegacyBridgeMonitor
    ```

## Observabilidade

-   Serilog configurado para Console e arquivo (ajustável em appsettings).
-   HealthChecks expostos em /health para uso com monitoramento e load balancers.

## CI/CD e Operação

    -   [docs/arquitetura.md](docs/arquitetura.md)
    -   [docs/runbook.md](docs/runbook.md)
    -   [docs/ci-cd.md](docs/ci-cd.md)

## Troubleshooting

-   API não sobe no Docker: verifique logs do container e variáveis de ambiente/connection string.
-   Erros de CORS: ajuste AllowedOrigins em API/Config (ou appsettings).
-   VB6 sem internet/SSL: teste com HTTP local; se usar HTTPS, configure certificado de desenvolvimento confiável.
-   Conflito de portas: edite as portas no docker-compose.yml ou desligue processos na porta em uso.

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

A licença MIT permite uso comercial, modificação, distribuição e uso privado, mantendo apenas a atribuição de copyright e isenção de garantias.
