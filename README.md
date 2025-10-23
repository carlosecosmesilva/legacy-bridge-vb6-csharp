# Legacy Bridge

Modernização que integra um sistema legado VB6 com uma API moderna em C# e banco PostgreSQL.

-   Integração gradual: VB6 consome a API via HTTP (MSXML).
-   Backend em ASP.NET Core seguindo **Clean Architecture**.
-   Separação em camadas: Domain, Application, Infrastructure e API.
-   Persistência no PostgreSQL com Entity Framework Core.
-   Serviço de monitoramento (Windows Service) baseado em .NET Worker.

## 🏗️ Arquitetura

Este projeto segue os princípios da **Clean Architecture** (Robert C. Martin), organizando o código em camadas com responsabilidades bem definidas:

### Camadas da Aplicação

```
┌─────────────────────────────────────────────────────────┐
│                   LegacyBridge.Api                      │
│              (Controllers, Middleware)                  │
└────────────────────┬────────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        ▼                         ▼
┌──────────────────┐      ┌──────────────────────┐
│ Application      │      │  Infrastructure      │
│ (Services, DTOs) │      │ (Repositories, EF)   │
└────────┬─────────┘      └──────────┬───────────┘
         │                           │
         └────────────┬──────────────┘
                      ▼
              ┌──────────────┐
              │   Domain     │
              │  (Entities)  │
              └──────────────┘
```

#### 📦 LegacyBridge.Domain (Núcleo)

-   **Responsabilidade**: Entidades de negócio e interfaces de repositórios
-   **Dependências**: Nenhuma (projeto puro .NET)
-   **Conteúdo**:
    -   `Entities/`: Customer, Product
    -   `Interfaces/Repositories/`: ICustomerRepository, IProductRepository

#### 🎯 LegacyBridge.Application (Casos de Uso)

-   **Responsabilidade**: Lógica de aplicação, serviços e DTOs
-   **Dependências**: Apenas Domain
-   **Conteúdo**:
    -   `Services/`: CustomerService, ProductService
    -   `Interfaces/Services/`: ICustomerService, IProductService
    -   `DTOs/`: CustomerDto, ProductDto
    -   `Contracts/`: Requests, Responses, Common
    -   `Mappings/`: AutoMapper Profiles

#### 🔧 LegacyBridge.Infrastructure (Implementação)

-   **Responsabilidade**: Acesso a dados e recursos externos
-   **Dependências**: Domain, Application
-   **Conteúdo**:
    -   `Repositories/`: Implementações concretas dos repositórios
    -   `Persistence/AppDbContext`: Configuração do EF Core
    -   `Persistence/Configurations/`: Entity Type Configurations
    -   `Persistence/Migrations/`: Migrations do EF Core

#### 🌐 LegacyBridge.Api (Apresentação)

-   **Responsabilidade**: Endpoints HTTP, middleware e configuração
-   **Dependências**: Application, Infrastructure
-   **Conteúdo**:
    -   `Controllers/`: CustomersController, ProductsController
    -   `Middleware/`: ExceptionHandlingMiddleware
    -   `Extensions/`: ServiceCollectionExtensions
    -   `Program.cs`: Configuração e inicialização

### Princípios Aplicados

✅ **Separation of Concerns**: Cada camada tem responsabilidade única  
✅ **Dependency Inversion**: Camadas externas dependem de abstrações  
✅ **Testability**: Fácil mockar dependências e testar isoladamente  
✅ **Maintainability**: Mudanças em uma camada não afetam outras  
✅ **Framework Independence**: Domain não depende de frameworks externos

## 🛠️ Tecnologias Utilizadas

### Backend (.NET 8)

-   **ASP.NET Core 8.0** - Framework web
-   **Entity Framework Core 9.0** - ORM para acesso a dados
-   **AutoMapper 12.0** - Mapeamento objeto-objeto
-   **Serilog** - Logging estruturado
-   **Swashbuckle (Swagger)** - Documentação de API
-   **Npgsql** - Driver PostgreSQL para .NET

### Testes

-   **xUnit** - Framework de testes
-   **Moq** - Biblioteca de mocking
-   **FluentAssertions** - Assertions fluentes

### Banco de Dados

-   **PostgreSQL 15+** - Banco de dados relacional
-   **pg_trgm** - Extensão para busca full-text

### DevOps & Infraestrutura

-   **Docker** - Containerização
-   **Docker Compose** - Orquestração de containers
-   **Health Checks** - Monitoramento de saúde

### Legacy

-   **Visual Basic 6.0** - Sistema legado
-   **MSXML6** - Cliente HTTP para VB6

## ✨ Funcionalidades Principais

### API REST

-   ✅ CRUD completo de Clientes e Produtos
-   ✅ Busca de clientes com função PostgreSQL otimizada (ILIKE)
-   ✅ Validação de dados em múltiplas camadas
-   ✅ Tratamento centralizado de exceções
-   ✅ Logging estruturado com Serilog
-   ✅ Health checks integrados
-   ✅ Documentação automática com Swagger/OpenAPI
-   ✅ Suporte a CORS para integração frontend

### Arquitetura

-   ✅ Clean Architecture (Domain, Application, Infrastructure, API)
-   ✅ Dependency Injection nativo do ASP.NET Core
-   ✅ Repository Pattern
-   ✅ Service Layer Pattern
-   ✅ AutoMapper para transformação de objetos
-   ✅ Entity Framework Core com Code-First
-   ✅ Migrations automáticas de banco de dados

### Qualidade de Código

-   ✅ Testes unitários com xUnit
-   ✅ Mocking com Moq
-   ✅ Assertions fluentes com FluentAssertions
-   ✅ Separação clara de responsabilidades
-   ✅ Código testável e manutenível

### Integração Legacy

-   ✅ Endpoints compatíveis com VB6
-   ✅ Respostas padronizadas (ApiResponse)
-   ✅ Tratamento de erros amigável
-   ✅ Conversão automática de tipos

### DevOps Ready

-   ✅ Dockerfiles otimizados
-   ✅ Docker Compose para ambiente completo
-   ✅ Health checks para orquestração
-   ✅ Logs centralizados em arquivo
-   ✅ Configuração por ambiente (appsettings)

## Estrutura do projeto

```
legacy-bridge-vb6-csharp/
├── server/
│   ├── LegacyBridge.Domain/
│   │   ├── Entities/
│   │   │   ├── Customer.cs
│   │   │   └── Product.cs
│   │   ├── Interfaces/
│   │   │   └── Repositories/
│   │   │       ├── ICustomerRepository.cs
│   │   │       └── IProductRepository.cs
│   │   └── Common/
│   │
│   ├── LegacyBridge.Application/
│   │   ├── DTOs/
│   │   │   ├── CustomerDto.cs
│   │   │   └── ProductDto.cs
│   │   ├── Contracts/
│   │   │   ├── Common/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Interfaces/
│   │   │   └── Services/
│   │   │       ├── ICustomerService.cs
│   │   │       └── IProductService.cs
│   │   ├── Services/
│   │   │   ├── CustomerService.cs
│   │   │   └── ProductService.cs
│   │   ├── Mappings/
│   │   │   ├── CustomerProfile.cs
│   │   │   └── ProductProfile.cs
│   │   └── Extensions/
│   │       └── MappingExtensions.cs
│   │
│   ├── LegacyBridge.Infrastructure/
│   │   ├── Repositories/
│   │   │   ├── CustomerRepository.cs
│   │   │   └── ProductRepository.cs
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   └── Extensions/
│   │       └── InfrastructureExtensions.cs
│   │
│   └── LegacyBridge.Api/
│       ├── Controllers/
│       │   ├── CustomersController.cs
│       │   └── ProductsController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Extensions/
│       │   ├── ServiceCollectionExtensions.cs
│       │   └── ApiResponseExtensions.cs
│       ├── Properties/
│       ├── Logs/
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
│
├── tests/
│   └── LegacyBridge.UnitTests/
│       ├── Services/
│       ├── Extensions/
│       └── Repositories/
│
├── services/
│   └── MonitorService/
│
├── legacy/
│   └── VB6/
│
├── Db/
├── docs/
│   ├── arquitetura.md
│   ├── ci-cd.md
│   ├── runbook.md
│   └── migracao-clean-architecture.md (este documento)
│
├── docker-compose.yml
├── LegacyBridge.sln
├── .gitignore
├── README.md
└── LICENSE
```

### Organização de Pastas

#### 📂 `/server` - Aplicação Backend

Contém todos os projetos da API organizados em camadas Clean Architecture:

-   **LegacyBridge.Domain**: Camada de domínio (entidades e interfaces)
-   **LegacyBridge.Application**: Camada de aplicação (serviços, DTOs, contratos)
-   **LegacyBridge.Infrastructure**: Camada de infraestrutura (repositórios, EF Core)
-   **LegacyBridge.Api**: Camada de apresentação (controllers, middleware)

#### 🧪 `/tests` - Testes Automatizados

-   **LegacyBridge.UnitTests**: Testes unitários com xUnit, Moq e FluentAssertions

#### ⚙️ `/services` - Serviços Auxiliares

-   **MonitorService**: Windows Service para monitoramento de arquivos

#### 🗄️ `/legacy` - Sistema Legado

-   **VB6**: Código-fonte da aplicação Visual Basic 6.0

#### 📊 `/Db` - Scripts de Banco de Dados

-   Scripts SQL para criação do schema e dados iniciais
-   Dumps e backups do PostgreSQL

#### 📖 `/docs` - Documentação

-   `arquitetura.md`: Documentação detalhada da arquitetura
-   `ci-cd.md`: Pipeline de integração e deploy
-   `runbook.md`: Guia operacional
-   `migracao-clean-architecture.md`: Guia de migração para Clean Architecture

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

Edite `server\LegacyBridge.Api\appsettings.json`:

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

-   Clique direito em `LegacyBridge.Api` → **Set as Startup Project**
-   Pressione `F5` para debug ou `Ctrl+F5` sem debug
-   Acesse: http://localhost:5000 ou https://localhost:5001

**Para executar apenas o MonitorService:**

-   Clique direito em `MonitorService` → **Set as Startup Project**
-   Pressione `F5` para debug ou `Ctrl+F5` sem debug

**Para executar ambos simultaneamente:**

-   Clique direito na Solution → **Configure Startup Projects**
-   Selecione **Multiple startup projects**
-   Defina `LegacyBridge.Api` e `MonitorService` como **Start**
-   Pressione `F5`

7. **Executar Testes**

```powershell
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test tests\LegacyBridge.UnitTests

# Com coverage
dotnet test /p:CollectCoverage=true
```

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
dotnet run --project server\LegacyBridge.Api

# Apenas MonitorService
dotnet run --project services\MonitorService

# Build de toda a solution
dotnet build LegacyBridge.sln

# Executar testes
dotnet test LegacyBridge.sln

# Ambos (em terminais separados)
Start-Process powershell -ArgumentList "dotnet run --project server\LegacyBridge.Api"
Start-Process powershell -ArgumentList "dotnet run --project services\MonitorService"
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
-   **PostgreSQL**: localhost:5432 (usuário: `seu_login`, senha: `sua_senha`, database: `nome_banco`)

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

## 📚 Documentação Adicional

-   **[Arquitetura](docs/arquitetura.md)** - Documentação detalhada da arquitetura do sistema
-   **[CI/CD](docs/ci-cd.md)** - Pipeline de integração e deploy contínuo
-   **[Runbook](docs/runbook.md)** - Guia operacional e troubleshooting

## 🤝 Contribuindo

Contribuições são bem-vindas! Para contribuir:

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

### Padrões de Código

-   Siga os princípios SOLID
-   Mantenha a separação de camadas Clean Architecture
-   Adicione testes unitários para novas funcionalidades
-   Use convenções de nomenclatura C# (.NET)
-   Documente APIs públicas com XML comments

### Commits

Use commits semânticos:

-   `feat:` nova funcionalidade
-   `fix:` correção de bug
-   `docs:` alterações na documentação
-   `refactor:` refatoração de código
-   `test:` adição ou modificação de testes
-   `chore:` tarefas de manutenção

## 📝 Roadmap

-   [ ] Implementar autenticação JWT
-   [ ] Adicionar testes de integração
-   [ ] Implementar CQRS com MediatR
-   [ ] Adicionar validação com FluentValidation
-   [ ] Implementar cache com Redis
-   [ ] Adicionar monitoramento com Application Insights
-   [ ] API Versioning
-   [ ] GraphQL endpoint
-   [ ] Suporte a múltiplos idiomas (i18n)
-   [ ] Migração completa do VB6 para .NET

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👥 Autores

-   **Carlos Silva** - _Desenvolvimento inicial_ - [@carlosecosmesilva](https://github.com/carlosecosmesilva)

## 🙏 Agradecimentos

-   Comunidade .NET por ferramentas e bibliotecas excelentes
-   Equipe do PostgreSQL pelo banco de dados robusto
-   Robert C. Martin pelos princípios de Clean Architecture
-   Todos os contribuidores que ajudaram a melhorar este projeto

---

**Feito usando .NET 8 e Clean Architecture**