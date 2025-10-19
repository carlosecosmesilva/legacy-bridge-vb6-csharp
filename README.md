# Legacy Bridge

Modernização que integra um sistema legado VB6 com uma API moderna em C# e banco PostgreSQL.

-   Integração gradual: VB6 consome a API via HTTP (MSXML).
-   Backend em ASP.NET Core com camadas de Controllers, Services e Repositories.
-   Persistência no PostgreSQL (inclui função de busca com ILIKE).
-   Serviço de monitoramento (Windows Service) baseado em .NET Worker.

## Estrutura do projeto

```
legacy-bridge-vb6-csharp/
│
├── Api/                                # API REST em ASP.NET Core 8.0
│   ├── Controllers/                    # Endpoints REST
│   │   ├── CustomersController.cs      # CRUD de clientes
│   │   └── ProductsController.cs       # CRUD de produtos
│   ├── Services/                       # Camada de negócio
│   │   ├── Interfaces/
│   │   │   ├── ICustomerService.cs
│   │   │   └── IProductService.cs
│   │   ├── CustomerService.cs
│   │   └── ProductService.cs
│   ├── Repositories/                   # Camada de acesso a dados
│   │   ├── Interfaces/
│   │   │   ├── ICustomerRepository.cs
│   │   │   └── IProductRepository.cs
│   │   ├── CustomerRepository.cs
│   │   └── ProductRepository.cs
│   ├── Models/                         # Entidades e DTOs
│   │   ├── Customer.cs
│   │   ├── Product.cs
│   │   └── ApiResponse.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Config/                         # Arquivos de configuração
│   ├── Logs/                           # Logs (gerado em runtime)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs                      # Bootstrap da aplicação
│   ├── appsettings.json
│   ├── Api.csproj
│   └── Dockerfile
│
├── MonitorService/                     # Windows Service (.NET 8 Worker)
│   ├── Services/                       # Serviços do monitor
│   │   ├── FileWatcherService.cs
│   │   ├── FileEventProcessor.cs
│   │   ├── FileEventLogger.cs
│   │   ├── HealthCheckService.cs
│   │   ├── ConfigurationService.cs
│   │   ├── RetryService.cs
│   │   ├── CircuitBreakerService.cs
│   │   └── MonitorBackgroundService.cs
│   ├── Interfaces/
│   │   ├── IFileWatcherService.cs
│   │   ├── IFileEventProcessor.cs
│   │   ├── IFileEventLogger.cs
│   │   └── IHealthCheckService.cs
│   ├── Models/
│   │   ├── FileEvent.cs
│   │   ├── FileWatcherConfiguration.cs
│   │   ├── RetryPolicy.cs
│   │   └── ServiceHealth.cs
│   ├── Logs/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── MonitorService.csproj
│   └── Dockerfile
│
├── VB6/                                # Aplicação legado em Visual Basic 6
│   ├── Forms/                          # Formulários (telas)
│   │   ├── frmMain.frm
│   │   ├── frmCustomers.frm
│   │   └── frmProductAPI.frm
│   ├── Modules/                        # Módulos (.bas)
│   │   ├── modApi.bas                  # Rotinas MSXML para consumir API
│   │   ├── JsonConverter.bas
│   │   └── modUtils.bas
│   ├── LegacyBridge.vbp
│   ├── LegacyBridge.vbw
│   └── MSSCCPRJ.SCC
│
├── Db/                                 # Scripts SQL (PostgreSQL)
│   ├── 001_create_database.sql
│   ├── 002_create_customers_table.sql
│   ├── 003_create_products_table.sql
│   ├── 004_create_search_function.sql
│   ├── 005_inserts_schema.sql
│   └── Dump e Backup/
│       ├── dump_legacy_bridge_db.sql
│       └── legacy_bridge_db.backup
│
├── docs/                               # Documentação do projeto
│   ├── arquitetura.md
│   ├── ci-cd.md
│   └── runbook.md
│
├── .github/                            # GitHub Actions
├── .gitignore
├── docker-compose.yml                  # Orquestração de containers
├── LegacyBridge.sln                    # Solution do Visual Studio
├── LICENSE                             # Licença MIT
└── README.md                           # Este arquivo
```

## Requisitos

-   **Visual Studio 2022** (recomendado) ou **Visual Studio Code**
-   **.NET 8.0 SDK** ou superior
-   **PostgreSQL 15** ou superior
-   **Docker Desktop** (opcional, para execução containerizada)
-   **Visual Basic 6.0** (para trabalhar com a aplicação legada)
-   **MSXML6** (normalmente já presente no Windows)

## Comece rápido

### Opção 1: Visual Studio 2022 (Recomendado)

1. **Clone o repositório**

```bash
git clone https://github.com/carlosecosmesilva/legacy-bridge-vb6-csharp.git
cd legacy-bridge-vb6-csharp
```

2. **Abra a Solution**

```powershell
# Abrir no Visual Studio
start LegacyBridge.sln

# Ou via linha de comando
devenv LegacyBridge.sln
```

3. **Configure o banco de dados**

```powershell
# Criar banco de dados
psql -U postgres -c "CREATE DATABASE legacy_bridge_db;"

# Executar scripts na ordem
psql -U postgres -d legacy_bridge_db -f "Db\001_create_database.sql"
psql -U postgres -d legacy_bridge_db -f "Db\002_create_customers_table.sql"
psql -U postgres -d legacy_bridge_db -f "Db\003_create_products_table.sql"
psql -U postgres -d legacy_bridge_db -f "Db\004_create_search_function.sql"
psql -U postgres -d legacy_bridge_db -f "Db\005_inserts_schema.sql"
```

4. **Configure a connection string**

Edite `Api\appsettings.json`:

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=legacy_bridge_db"
	}
}
```

5. **Restaure pacotes NuGet**

-   Visual Studio restaura automaticamente
-   Ou clique direito na Solution → **Restore NuGet Packages**
-   Ou execute: `dotnet restore`

6. **Executar os projetos**

**Para executar apenas a API:**

-   Clique direito em `Api` → **Set as Startup Project**
-   Pressione `F5` para debug ou `Ctrl+F5` sem debug
-   Acesse: http://localhost:5000 ou https://localhost:5001

**Para executar apenas o MonitorService:**

-   Clique direito em `MonitorService` → **Set as Startup Project**
-   Pressione `F5` para debug ou `Ctrl+F5` sem debug

**Para executar ambos simultaneamente:**

-   Clique direito na Solution → **Configure Startup Projects**
-   Selecione **Multiple startup projects**
-   Defina ambos como **Start**
-   Pressione `F5`

### Opção 2: .NET CLI (Linha de Comando)

1. **Clone e restaure**

```powershell
git clone https://github.com/carlosecosmesilva/legacy-bridge-vb6-csharp.git
cd legacy-bridge-vb6-csharp
dotnet restore
```

2. **Configure o banco** (veja Opção 1, passo 3)

3. **Execute os projetos**

```powershell
# Apenas API
dotnet run --project Api

# Apenas MonitorService
dotnet run --project MonitorService

# Ambos (em terminais separados)
Start-Process powershell -ArgumentList "dotnet run --project Api"
Start-Process powershell -ArgumentList "dotnet run --project MonitorService"
```

4. **Acesse a API**

-   **Swagger**: http://localhost:5000/swagger
-   **Health Check**: http://localhost:5000/health
-   **API Base**: http://localhost:5000/api

### Opção 3: Docker Compose

1. **Clone o repositório**

```bash
git clone https://github.com/carlosecosmesilva/legacy-bridge-vb6-csharp.git
cd legacy-bridge-vb6-csharp
```

2. **Suba os containers**

```bash
docker compose up -d --build
```

Isso irá iniciar:

-   **PostgreSQL** na porta `5432`
-   **API** na porta `5000`
-   **MonitorService** em background

3. **Acesse os serviços**

-   **API**: http://localhost:5000
-   **Swagger**: http://localhost:5000/swagger
-   **Health Check**: http://localhost:5000/health
-   **PostgreSQL**: localhost:5432 (usuário: `paschoal`, senha: `paschoal_pass`, database: `paschoal_db`)

4. **Parar e remover containers**

```bash
docker compose down
```

Para remover também os volumes (dados do banco):

```bash
docker compose down -v
```

## 📡 Endpoints da API

### Customers (Clientes)

| Método   | Endpoint                             | Descrição                        |
| -------- | ------------------------------------ | -------------------------------- |
| `GET`    | `/api/customers`                     | Lista todos os clientes          |
| `GET`    | `/api/customers/{id}`                | Busca cliente por ID             |
| `GET`    | `/api/customers/search?term={termo}` | Busca clientes por termo (ILIKE) |
| `POST`   | `/api/customers`                     | Cria novo cliente                |
| `PUT`    | `/api/customers/{id}`                | Atualiza cliente existente       |
| `DELETE` | `/api/customers/{id}`                | Remove cliente                   |

### Products (Produtos)

| Método   | Endpoint             | Descrição                  |
| -------- | -------------------- | -------------------------- |
| `GET`    | `/api/products`      | Lista todos os produtos    |
| `GET`    | `/api/products/{id}` | Busca produto por ID       |
| `POST`   | `/api/products`      | Cria novo produto          |
| `PUT`    | `/api/products/{id}` | Atualiza produto existente |
| `DELETE` | `/api/products/{id}` | Remove produto             |

### Health Check

| Método | Endpoint  | Descrição                           |
| ------ | --------- | ----------------------------------- |
| `GET`  | `/health` | Verifica saúde da aplicação e banco |

### Exemplos de Requisições

**Buscar clientes por termo:**

```bash
curl "http://localhost:5000/api/customers/search?term=silva"
```

**Criar novo cliente:**

```bash
curl -X POST "http://localhost:5000/api/customers" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "document": "11111111111",
    "active": true
  }'
```

**Atualizar cliente:**

```bash
curl -X PUT "http://localhost:5000/api/customers/1" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva Atualizado",
    "document": "11111111111",
    "active": true
  }'
```

**Listar todos os produtos:**

```bash
curl "http://localhost:5000/api/products"
```

**Criar novo produto:**

```bash
curl -X POST "http://localhost:5000/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Novo Plano",
    "price": 99.90,
    "active": true
  }'
```

**Health Check:**

```bash
curl "http://localhost:5000/health"
```

### Estrutura do Payload para Clientes

```json
{
	"id": 1,
	"name": "João Silva",
	"document": "11111111111",
	"active": true,
	"created_at": "2025-10-19T10:30:00Z"
}
```

### Estrutura do Payload para Produtos

```json
{
	"id": 1,
	"name": "Plano Básico",
	"price": 49.9,
	"active": true,
	"created_at": "2025-10-19T10:30:00Z"
}
```

## 🗄️ Banco de Dados

### Estrutura das Tabelas

#### Tabela `customers`

```sql
- id (BIGSERIAL PRIMARY KEY)
- name (TEXT, NOT NULL) - Nome do cliente
- document (VARCHAR(20)) - CPF / CNPJ / etc (opcional)
- active (BOOLEAN, DEFAULT TRUE) - Status do cliente
- created_at (TIMESTAMP, DEFAULT NOW()) - Data de criação
```

**Índices:**

-   `ux_customers_document` - Índice único por documento (quando informado)
-   `idx_customers_name_trgm` - Índice de busca textual com GIN (pg_trgm)

#### Tabela `products`

```sql
- id (BIGSERIAL PRIMARY KEY)
- name (TEXT, NOT NULL, UNIQUE) - Nome do produto
- price (NUMERIC(12,2), NOT NULL, CHECK >= 0) - Preço do produto
- active (BOOLEAN, DEFAULT TRUE) - Status do produto
- created_at (TIMESTAMP, DEFAULT NOW()) - Data de criação
```

**Índices:**

-   `ux_products_name` - Índice único por nome
-   `ix_products_active_name` - Índice composto para listagens

### Funções SQL

#### `search_customers_by_name(p_term TEXT, p_limit INT, p_offset INT)`

Função de busca case-insensitive de clientes por nome usando ILIKE.

**Parâmetros:**

-   `p_term` - Termo de busca (suporta busca parcial)
-   `p_limit` - Limite de registros (padrão: 50)
-   `p_offset` - Deslocamento (padrão: 0)

**Retorna:**

-   `id`, `name`, `document`, `active`

**Exemplo de uso:**

```sql
SELECT * FROM search_customers_by_name('joao', 10, 0);
```

### Scripts de Inicialização

Os scripts estão numerados para execução na ordem correta:

1. **001_create_database.sql**
    - Cria o banco de dados `legacy_bridge_db`
    - Configuração UTF-8 com locale pt_BR
2. **002_create_customers_table.sql**
    - Cria tabela `customers`
    - Habilita extensão `pg_trgm`
    - Cria índices para busca e unicidade
3. **003_create_products_table.sql**
    - Cria tabela `products`
    - Cria índices para otimização de queries
4. **004_create_search_function.sql**
    - Cria função `search_customers_by_name()` para busca otimizada
5. **005_inserts_schema.sql**
    - Insere dados de exemplo (5 clientes e 5 produtos)
    - Usa `ON CONFLICT` para idempotência

### Dados de Exemplo

**Clientes incluídos:**

-   João Silva (11111111111) - Ativo
-   Maria Oliveira (22222222222) - Ativo
-   Carlos Souza (33333333333) - Inativo
-   Mariana Costa (44444444444) - Ativo
-   José Santos (55555555555) - Ativo

**Produtos incluídos:**

-   Plano Básico - R$ 49,90 - Ativo
-   Plano Profissional - R$ 129,90 - Ativo
-   Plano Empresarial - R$ 299,90 - Ativo
-   Add-on Relatórios - R$ 19,90 - Ativo
-   Produto Descontinuado - R$ 9,90 - Inativo

### Extensões PostgreSQL Utilizadas

-   **pg_trgm** - Busca textual fuzzy usando trigrams (para buscas ILIKE otimizadas)

## 🖥️ Aplicação VB6

### Configuração

1. Abra o projeto `VB6\LegacyBridge.vbp` no Visual Basic 6.0
2. Configure a URL base da API no módulo `modApi.bas`:

```vb
Public Const API_BASE_URL As String = "http://localhost:5000/api"
```

### Forms Disponíveis

-   **frmMain.frm** - Tela principal do sistema
-   **frmCustomers.frm** - Gerenciamento de clientes
-   **frmProductAPI.frm** - Gerenciamento de produtos via API

### Módulos

-   **modApi.bas** - Funções de consumo da API (MSXML6)
-   **JsonConverter.bas** - Parser JSON simplificado para VB6
-   **modUtils.bas** - Funções auxiliares e utilitárias

### Requisitos VB6

-   **MSXML6** instalado (Microsoft XML Core Services)
-   **CORS** habilitado na API para aceitar requisições do VB6

## 📊 Monitor Service

O MonitorService é um Windows Service que monitora diretórios e processa arquivos automaticamente.

### Configuração

Edite `MonitorService\appsettings.json`:

```json
{
	"FileWatcher": {
		"MonitorPath": "C:\\Integration\\Drop",
		"IncludeFilter": "*.txt;*.csv;*.xml",
		"ExcludeFilter": "*.tmp;*.log",
		"ProcessingMode": "Move",
		"ArchivePath": "C:\\Integration\\Archive",
		"ErrorPath": "C:\\Integration\\Error"
	}
}
```

### Executar em modo console (desenvolvimento)

```powershell
dotnet run --project MonitorService
```

### Instalar como Windows Service

```powershell
# Publicar
dotnet publish MonitorService -c Release -o .\publish

# Instalar serviço
sc create LegacyBridgeMonitor binPath= "%CD%\publish\MonitorService.exe" start= auto

# Iniciar serviço
sc start LegacyBridgeMonitor

# Verificar status
sc query LegacyBridgeMonitor
```

### Remover Windows Service

```powershell
# Parar serviço
sc stop LegacyBridgeMonitor

# Remover serviço
sc delete LegacyBridgeMonitor
```

## 📝 Observabilidade

### Logging (Serilog)

Logs são escritos em:

-   **Console** - Desenvolvimento
-   **Arquivo** - `Api/Logs/api-YYYYMMDD.log` e `MonitorService/Logs/monitor-YYYYMMDD.log`
-   **Retenção** - 7 dias de histórico

### Health Checks

A API expõe um endpoint de health check em `/health`:

```json
{
	"status": "Healthy",
	"checks": {
		"PostgreSQL": "Healthy"
	},
	"duration": "00:00:00.0234567"
}
```

### Métricas

-   Tempo de resposta de requisições
-   Status de conexão com banco de dados
-   Arquivos processados pelo MonitorService

## 🔧 Troubleshooting

### API não inicia

**Problema**: Erro de connection string

```
Npgsql.NpgsqlException: Connection refused
```

**Solução**: Verifique se o PostgreSQL está rodando e a connection string em `appsettings.json` está correta.

### Erro de CORS no VB6

**Problema**: Access-Control-Allow-Origin

**Solução**: Adicione a origem no `appsettings.json`:

```json
{
	"Cors": {
		"AllowedOrigins": ["http://localhost", "*"]
	}
}
```

### Docker não builda

**Problema**: Erro ao construir imagem Docker

**Solução**: Verifique se o Docker Desktop está rodando e execute:

```bash
docker compose build --no-cache
```

### Porta em uso

**Problema**: Port already in use

**Solução**: Altere a porta no `docker-compose.yml` ou `launchSettings.json`:

```yaml
ports:
    - "5001:80" # Usar porta 5001 ao invés de 5000
```

### MonitorService não processa arquivos

**Problema**: Arquivos não são detectados

**Solução**: Verifique permissões das pastas e se o caminho existe:

```powershell
# Criar diretórios
New-Item -ItemType Directory -Force -Path "C:\Integration\Drop"
New-Item -ItemType Directory -Force -Path "C:\Integration\Archive"
New-Item -ItemType Directory -Force -Path "C:\Integration\Error"
```

## 📚 Documentação Adicional

-   [docs/arquitetura.md](docs/arquitetura.md) - Diagramas e decisões arquiteturais
-   [docs/runbook.md](docs/runbook.md) - Guia operacional e sustentação
-   [docs/ci-cd.md](docs/ci-cd.md) - Pipeline de integração e deploy

## 📄 Licença

Este projeto está licenciado sob a **Licença MIT** - veja o arquivo [LICENSE](LICENSE) para detalhes.

A licença MIT permite:

-   ✅ Uso comercial
-   ✅ Modificação
-   ✅ Distribuição
-   ✅ Uso privado

Mantendo apenas:

-   ℹ️ Atribuição de copyright
-   ℹ️ Isenção de garantias
