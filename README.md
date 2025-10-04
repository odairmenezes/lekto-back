# CadPlus ERP - Módulo de Cadastro de Usuários

Sistema ERP desenvolvido para o mercado hospitalar, com foco na gestão completa de usuários e suas informações. Esta API REST foi desenvolvida em .NET 9 seguindo melhores práticas de desenvolvimento e arquitetura limpa.

## 📋 Visão Geral

O CadPlus ERP é comercializado por módulos independentes, sendo o módulo de cadastro de usuários fundamental para todas as versões do produto. Este projeto implementa uma API REST robusta e escalável que será consumida por uma interface Angular.

### 🎯 Objetivos do Projeto

- **Eficiência Operacional**: Automação completa do processo de cadastro de usuários
- **Conformidade**: Atendimento às regulamentações e normas hospitalares
- **Segurança**: Proteção de dados sensíveis com criptografia e auditoria completa
- **Escalabilidade**: Arquitetura preparada para crescimento e múltiplos módulos

## 🏆 Funcionalidades Principais

### 👤 Gestão Completa de Usuários

#### ✅ Validações Avançadas
- **CPF brasileiro**: Validação por algoritmo oficial + prevenção de duplicatas
- **Nome completo**: Mínimo 4 caracteres, apenas letras e espaços
- **E-mail corporativo**: Conformidade RFC + normalização automática
- **Telefone nacional**: Validação de DDD + formatação automática
- **Senha forte**: Múltiplos critérios de segurança + prevenção de breaches

#### ✅ Múltiplos Endereços por Usuário
- **Gestão flexível**: Usuários podem ter vários endereços cadastrados
- **Prevenção de duplicatas**: Sistema impede cadastro de endereços idênticos
- **Endereço principal**: Funcionalidade para definir endereço de referência
- **Validação geográfica**: Estados e CEPs brasileiros validados

#### ✅ Auditoria Completa
- **Rastreabilidade**: Log completo de todas as alterações realizadas
- **Histórico detalhado**: Registro "de → para" de cada campo modificado
- **Compliance**: Auditoria preparada para normas regulamentárias
- **Busca por CPF**: Filtro específico por CPF do usuário
- **Ordenação inteligente**: Por período decrescente e por ação
- **API dedicada**: Endpoint específico para consulta de logs
- **Paginação**: Navegação eficiente em grandes volumes de dados
- **Logs automáticos**: Criação automática de logs em todas as operações

## 🛠️ Stack Tecnológica

### Backend
- **[.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)** - Framework principal com alta performance
- **[ASP.NET Core Web API](https://docs.microsoft.com/pt-br/aspnet/core/web-api/)** - API REST moderna e robusta
- **[Entity Framework Core](https://docs.microsoft.com/pt-br/ef/core/)** - ORM avançado para acesso a dados
- **[SQLite](https://www.sqlite.org/index.html)** - Banco leve para desenvolvimento e testes
- **[SQL Server](https://www.microsoft.com/pt-br/sql-server/)** - Banco de produção empresarial

### Qualidade e Documentação
- **[Swagger/OpenAPI](https://swagger.io/)** - Documentação interativa da API
- **[FluentValidation](https://docs.fluentvalidation.net/)** - Validação robusta e declarativa
- **[AutoMapper](https://automapper.org/)** - Mapeamento automático de objetos
- **[Serilog](https://serilog.net/)** - Logging estruturado avançado

### Segurança
- **[BCrypt.Net](https://github.com/BcryptNet/bcrypt.net)** - Hash seguro de senhas
- **[JWT Bearer](https://jwt.io/)** - Autenticação stateless e segura

### Testes
- **[xUnit](https://xunit.net/)** - Framework moderno para testes unitários
- **[Moq](https://github.com/moq/moq4)** - Framework para mocking
- **[FluentAssertions](https://fluentassertions.com/)** - Assertions expressivas

## 🗂️ Arquitetura do Projeto

```
lelklto/
├── 🎮 Controllers/                 # Camada de apresentação
│   ├── AuthController.cs          # Autenticação e autorização
│   ├── UsersController.cs         # Gestão completa de usuários
│   ├── AddressesController.cs     # Gestão de endereços
│   └── StartupController.cs       # Operações de inicialização
├── 🧠 Models/                     # Entidades de domínio
│   └── User.cs                    # Entidade usuário com endereços
├── 📦 DTOs/                       # Objetos de transferência de dados
│   ├── UserDTOs.cs                # DTOs para operações de usuário
│   ├── AuthDTOs.cs                # DTOs de autenticação
│   └── AddTestUserDto.cs          # DTO para testes
├── ⚙️ Services/                    # Lógica de negócio
│   ├── Interfaces/                # Contratos dos serviços
│   │   ├── IUserService.cs
│   │   ├── IAuthService.cs
│   │   ├── IPasswordService.cs
│   │   └── ICpfValidationService.cs
│   └── Implementations/           # Implementações concretas
│       ├── UserService.cs
│       ├── AuthService.cs
│       ├── PasswordService.cs
│       └── CpfValidationService.cs
├── 🗃️ Data/                       # Camada de dados
│   └── CadPlusDbContext.cs        # Context do Entity Framework
├── 📝 Validators/                 # Validações de entrada
│   ├── UserValidator.cs           # Validação de usuários
│   ├── AddressValidators.cs       # Validação de endereços
│   ├── AuthValidators.cs          # Validação de autenticação
│   └── UserValidators.cs          # Validações adicionais
├── 🔧 Middleware/                 # Componentes de infraestrutura
│   ├── ExceptionHandlingMiddleware.cs  # Tratamento global de exceções
│   └── RequestLoggingMiddleware.cs     # Logging de requisições
├── 🔌 Extensions/                 # Extensões personalizadas
│   ├── StringExtensions.cs        # Extensões de string
│   └── UserExtensions.cs          # Conversores de usuário
├── 🗂️ Mappings/                   # Perfis do AutoMapper
│   └── UserProfile.cs             # Mapeamento de usuários
├── 🔄 Migrations/                 # Migrações do banco de dados
│   ├── 20251004160828_InitialMigration.cs
│   ├── 20251004160828_InitialMigration.Designer.cs
│   └── CadPlusDbContextModelSnapshot.cs
├── 🧪 Tests/                      # Testes automatizados
│   ├── Unit/                      # Testes unitários
│   │   ├── Services/              # Testes de serviços
│   │   └── TestUtilities/         # Utilitários para testes
│   └── Integration/               # Testes de integração
├── 📊 data/                       # Arquivo do banco de dados
│   ├── CadPlusDb_Dev.db          # Banco SQLite
│   ├── CadPlusDb_Dev.db-shm      # Cache compartilhado
│   └── CadPlusDb_Dev.db-wal      # Write-ahead logging
├── ⚙️ Properties/                 # Configurações de projeto
│   └── launchSettings.json       # Configurações de debug
├── 📋 appsettings.json           # Configurações da aplicação
├── 📋 appsettings.Development.json # Configurações de desenvolvimento
├── 🚀 Program.cs                 # Configuração e inicialização
├── 📁 lelklto.csproj             # Arquivo do projeto
└── 📖 README.md                  # Esta documentação
```

## 🚀 Instalação e Configuração

### 📋 Pré-requisitos

- **[.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** ou superior
- **[Visual Studio Code](https://code.visualstudio.com/)** ou **Visual Studio 2022**
- **Git** para controle de versão
- **SQLite** (incluído no .NET) para desenvolvimento

### 🔧 Configuração Passo a Passo

#### 1. **Clonar o Repositório**
```bash
git clone https://github.com/seu-usuario/lelklto.git
cd lelklto
```

#### 2. **Instalar Dependências**
```bash
dotnet restore
```

#### 3. **Configurar Variáveis de Ambiente**

Copie o arquivo de template e configure suas variáveis:

```bash
# Copiar template de configuração
cp .env.example .env

# Editar configurações (opcional)
nano .env
```

Principais configurações no arquivo `.env`:

```bash
# 🔧 Configurações da API
API_PORT=7001
API_URL=http://localhost:7001

# 🗄️ Configurações do Banco de Dados
DATABASE_CONNECTION_STRING=Data Source=./data/CadPlusDb_Dev.db
DATABASE_NAME=CadPlusDb_Dev

# 🔐 Configurações de Segurança
JWT_SECRET_KEY=CadPlus_Super_Secret_Key_Minimum_256_Bits_For_Security
JWT_ISSUER=CadPlusERP
JWT_AUDIENCE=CadPlusFrontend
```

#### 4. **Verificar Configurações**
```bash
# Verificação completa de todas as configurações
./run-api.sh --check

# Saída esperada: ✅ Todas as configurações estão corretas!
```

#### 5. **Inicializar o Banco de Dados**
```bash
# Executar migrações
dotnet ef database update

# Ou inicializar através da API (recomendado)
curl -X POST http://localhost:7001/api/startup/init
```

#### 6. **Executar a Aplicação**

**Método 1: Script automatizado (Recomendado)**
```bash
# Executar na porta padrão (7001)
./run-api.sh

# Executar em porta específica
./run-api.sh 8080

# Verificar configurações antes de executar
./run-api.sh --check
```

**Método 2: Execução direta**
```bash
# Execução padrão
dotnet run

# Com configuração de porta específica
export API_PORT=8080
dotnet run
```

**Método 3: Docker (Produção)**
```bash
# Construir e executar com Docker Compose
docker-compose up --build

# Executar em background
docker-compose up -d --build
```

#### 7. **Verificar se está Funcionando**
```bash
# Teste de conectividade
curl http://localhost:7001/api/startup/status
```

### 🌐 Acessos

A API estará disponível nas seguintes URLs (ajuste a porta conforme sua configuração):

- **🏥 Health Check**: `http://localhost:7001/health` - Status do sistema
- **📖 Swagger UI**: `http://localhost:7001/swagger` - Documentação interativa  
- **🔗 API Base**: `http://localhost:7001/api` - Endpoints REST
- **📋 OpenAPI Spec**: `http://localhost:7001/openapi/v1.json` - Especificação OpenAPI

**💡 Importante:** Se você alterou a porta no arquivo `.env` ou usando `./run-api.sh [porta]`, substitua `7001` pela porta configurada.

## ⚙️ Configuração Avançada

### 🔧 Gerenciamento de Variáveis de Ambiente

A API CadPlus ERP usa um sistema inteligente de configuração que respeita a seguinte ordem de prioridade:

1. **Variáveis de ambiente do sistema** (maior prioridade)
2. **Arquivo `.env`** (configuração padrão)
3. **Valores hardcoded** (fallback)

#### Configuração por Variáveis de Ambiente

```bash
# Definir porta via variável de ambiente
export API_PORT=8080
export API_URL=http://localhost:8080
dotnet run

# Usar script personalizado
./run-api.sh 8080
```

#### Configuração por Arquivo .env

O arquivo `.env` é carregado automaticamente e contém:

```bash
# Configurações da API
API_PORT=7001
API_URL=http://localhost:7001

# Configurações do banco
DATABASE_CONNECTION_STRING=Data Source=./data/CadPlusDb_Dev.db
DATABASE_NAME=CadPlusDb_Dev

# Configurações de segurança JWT
JWT_SECRET_KEY=CadPlus_Super_Secret_Key_Minimum_256_Bits_For_Security
JWT_ISSUER=CadPlusERP
JWT_AUDIENCE=CadPlusFrontend

# Outras configurações...
```

#### 💡 Debug de Configuração

A aplicação mostra automaticamente qual configuração está sendo usada:

```bash
# Quando usa .env
✓ Arquivo .env carregado com sucesso de: /path/to/.env
  📍 API_PORT: 7001
  🌐 API_URL: http://localhost:7001

# Quando usa variáveis de sistema (sobresscreve .env)
✓ Usando variáveis de ambiente do sistema (sobrescrevendo .env)
  📍 API_PORT: 8080
  🌐 API_URL: http://localhost:8080
```

### 🐳 Docker com Variáveis de Ambiente

```bash
# Usar arquivo .env com Docker
docker-compose up --build

# Definir variáveis específicas
API_PORT=9000 docker-compose up --build

# Usar arquivo .env diferente
env --file .env.production docker-compose up --build
```

### 🔄 Scripts Utilitários

#### run-api.sh - Execução Inteligente

```bash
# Verificar configurações
./run-api.sh --check

# Executar na porta padrão (.env)
./run-api.sh

# Executar em porta específica
./run-api.sh 8080

# Ver ajuda completa
./run-api.sh --help
```

#### Verificação de Porta Disponível

O script automaticamente verifica se a porta está disponível:

```bash
✓ Porta 7001 disponível - iniciando API
# OU
❌ Porta 7001 já está em uso
💡 Tente uma porta diferente: ./run-api.sh 8080
```

## 📚 Documentação da API

### 🔐 Autenticação

#### Login de Usuário
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@cadplus.com.br",
  "password": "Admin123!@#"
}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abc123def456...",
    "expiresAt": "2025-10-05T16:27:06Z",
    "user": {
      "id": "ac56650e-a1bf-4e22-bae0-4ce30dae84b2",
      "firstName": "Administrador",
      "lastName": "Sistema",
      "fullName": "Administrador Sistema",
      "cpf": "12345678901",
      "email": "admin@cadplus.com.br",
      "phone": "(11) 99999-9999",
      "isActive": true,
      "createdAt": "2025-10-04T16:26:35.799Z",
      "addresses": []
    }
  }
}
```

### 👤 Gestão de Usuários

#### Obter Usuário por ID
```http
GET /api/users/{userId}
Authorization: Bearer {token}
```

#### Atualizar Usuário
```http
PUT /api/users/{userId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "João Carlos",
  "lastName": "Silva Santos",
  "phone": "(11) 98765-4321"
}
```

### 🏠 Gestão de Endereços

#### Adicionar Endereço
```http
POST /api/addresses/users/{userId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "street": "Rua das Flores",
  "number": "123",
  "neighborhood": "Centro",
  "complement": "Apto 45",
  "city": "São Paulo",
  "state": "SP",
  "zipCode": "01234-567",
  "country": "Brasil",
  "isPrimary": true
}
```

#### Listar Endereços do Usuário
```http
GET /api/addresses/users/{userId}
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Endereços listados com sucesso",
  "data": [
    {
      "id": "f1f3e4fd-b6ed-4874-be9e-462f468c5228",
      "street": "Rua das Flores",
      "number": "123",
      "neighborhood": "Centro",
      "complement": "Apto 45",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01234-567",
      "country": "Brasil",
      "isPrimary": true
    }
  ]
}
```

### 📊 Auditoria e Logs

#### Buscar logs por CPF do usuário
```http
GET /api/audit/cpf/{cpf}?page=1&limit=20
Authorization: Bearer {token}
```

**Parâmetros:**
- `cpf` (obrigatório): CPF do usuário
- `page` (opcional): Página (padrão: 1)
- `limit` (opcional): Limite por página (padrão: 20)

**Funcionalidades:**
- ✅ Busca logs de auditoria por CPF do usuário
- ✅ Ordenação por período decrescente (mais recentes primeiro)
- ✅ Ordenação secundária por ação (campo alterado)
- ✅ Paginação para grandes volumes de dados
- ✅ Logs automáticos em todas as operações

**Exemplo de uso:**
```bash
# Buscar logs de um usuário específico por CPF
GET /api/audit/cpf/12345678901?page=1&limit=20
```

**Logs automáticos capturam:**
- **Usuários**: Alterações em FirstName, LastName, Email, Phone, Password
- **Endereços**: Criação, atualização, exclusão e definição como principal
- **Metadados**: IP, User Agent, timestamp e usuário responsável

**Exemplo de resposta:**
```json
{
  "success": true,
  "message": "Logs de auditoria recuperados com sucesso",
  "data": {
    "logs": [
      {
        "id": "0fa704df-1b11-4e7c-9749-cdf7544e2db9",
        "userId": "cb18c50e-4e6f-49cb-8dd2-35f8156b85ef",
        "entityType": "User",
        "entityId": "cb18c50e-4e6f-49cb-8dd2-35f8156b85ef",
        "fieldName": "FirstName",
        "oldValue": "Administrador",
        "newValue": "Administrador Atualizado",
        "changedAt": "2025-10-04T18:51:42.8661567",
        "changedBy": "cb18c50e-4e6f-49cb-8dd2-35f8156b85ef",
        "ipAddress": null,
        "userAgent": null
      }
    ],
    "totalCount": 3,
    "currentPage": 1,
    "totalPages": 1,
    "itemsPerPage": 20
  }
}
```

### 🔧 Endpoints de Sistema

#### Status do Sistema
```http
GET /api/startup/status
```

#### Inicialização do Banco
```http
POST /api/startup/init
```

### 🧪 Testes de Configuração

Antes de executar a aplicação, é recomendável verificar se todas as configurações estão corretas:

```bash
# Verificação completa de configurações
./run-api.sh --check
```

**Saída esperada:**
```
=================================
  CadPlus ERP - Sistema Hospitalar
=================================
🔍 Verificando configurações...
✓ Arquivo .env encontrado
✓ API_PORT configurado no .env
✓ DATABASE_CONNECTION_STRING configurado
✓ JWT_SECRET_KEY configurado
✓ .NET SDK instalado
Versão: 9.0.305
✅ Todas as configurações estão corretas!
```

### 🔬 Teste Rápido da API

```bash
# Testar API em porta específica
./run-api.sh 8888

# Em outro terminal, testar endpoints
curl http://localhost:8888/health
curl http://localhost:8888/api/startup/status
```

## 🧪 Testes

### Executar Testes Unitários
```bash
# Todos os testes
dotnet test

# Testes específicos com verbosidade
dotnet test --verbosity normal

# Testes com relatório detalhado
dotnet test --logger "console;verbosity=detailed"
```

### Cobertura de Código
```bash
# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 Segurança Implementada

### Validações Robustas
- **CPF**: Algoritmo oficial brasileiro + prevenção de duplicatas
- **E-mail**: Conformidade RFC + normalização + anti-duplicata  
- **Telefone**: Validação de DDD brasileiro + formatação automática
- **Senha**: Múltiplos critérios de segurança + prevenção de palavras comuns
- **Endereço**: Validação geográfica + prevenção de duplicatas por usuário

### Proteções de Segurança
- **Hash de Senhas**: BCrypt com salt automático
- **JWT**: Tokens seguros com expiração configurável
- **CORS**: Política de origem restrita
- **Rate Limiting**: Proteção contra ataques de força bruta
- **Validação de Entrada**: Sanitização completa de dados

## 📊 Monitoramento e Logs

### Logs Estruturados
- **Serilog**: Logging estruturado em JSON
- **Níveis configuráveis**: Debug, Information, Warning, Error
- **Request Tracing**: Rastreamento completo de requisições
- **Performance**: Métricas de tempo de resposta

### Auditoria Completa
- **Rastreabilidade**: Todas as alterações são registradas
- **Histórico detalhado**: Estados "antes → depois" 
- **Usuário responsável**: Identificação de who/when/what
- **Compliance**: Preparado para auditorias regulamentárias

## 🚀 Deployment

### Desenvolvimento (SQLite)
```bash
# Executar com logs em tempo real
dotnet run --urls "http://localhost:7001" --environment Development
```

### Produção (SQL Server)
```bash
# Com banco SQL Server
dotnet run --urls "http://localhost:5000" --environment Production
```

## 📈 Performance e Escalabilidade

### Otimizações Implementadas
- **Async/Await**: Operações não-bloqueantes em toda a aplicação
- **Paginação**: Listagens grandes com paginação automática
- **Índices**: Banco otimizado com índices estratégicos
- **Connection Pooling**: Pool inteligente de conexões de banco
- **Compressão**: Responses otimizadas para transferência

### Arquitetura Escalável
- **Stateless**: API preparada para load balancing
- **Microserviços**: Estrutura modular para futuras expansões
- **Clean Architecture**: Separação clara de responsabilidades

## 🎯 Roadmap Futuro

### ✅ Concluído (Fase 1)
- [x] Estrutura base da API REST
- [x] Sistema completo de autenticação JWT
- [x] Validações robustas (CPF, telefone, e-mail)
- [x] Gestão completa de usuários e endereços
- [x] Sistema de auditoria e logs
- [x] Testes unitários implementados
- [x] Documentação Swagger completa

### 🔄 Em Desenvolvimento (Fase 2)
- [ ] Testes de integração automatizados
- [ ] Performance testing e otimizações
- [ ] Metadados para monitoramento
- [ ] Validação de segurança avançada

### 🎨 Próximas Fases
- [ ] Interface Angular completa
- [ ] Testes end-to-end automatizados
- [ ] Pipeline CI/CD
- [ ] Deploy em containers (Docker/Kubernetes)
- [ ] Monitoramento e alertas em produção

## 🤝 Contribuindo

Este projeto foi desenvolvido seguindo as melhores práticas do mercado. Para contribuir:

1. **Fork** o projeto
2. **Crie** uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. **Commit** suas mudanças (`git commit -m 'feat: adicionar nova funcionalidade'`)
4. **Push** para a branch (`git push origin feature/nova-funcionalidade`)
5. **Abra** um Pull Request

### Padrões de Qualidade
- ✅ **SOLID**: Princípios orientação a objetos seguidos
- ✅ **Clean Code**: Código limpo e autodocumentado
- ✅ **Testes**: Cobertura mínima de 80% nos componentes críticos
- ✅ **Documentação**: README e comentários em código atualizados

## 📞 Suporte

Para questões técnicas ou suporte:

- 📧 **E-mail**: devteam@cadplus.com.br  
- 📖 **Wiki**: [Documentação completa do projeto]
- 🐛 **Issues**: [GitHub Issues](https://github.com/seu-usuario/lelklto/issues)
- 💬 **Discussões**: [GitHub Discussions](https://github.com/seu-usuario/lelklto/discussions)

## 📄 Licença

Este projeto está licenciado sob a **Licença MIT** - veja o arquivo `LICENSE` para detalhes completos.

---

## 🛠️ Troubleshooting de Configuração

### Problemas Comuns

#### ❌ "Arquivo .env não encontrado"
```bash
# Solução: Copiar o template
cp .env.example .env

# Verificar se foi criado
ls -la .env
```

#### ❌ "Porta já está em uso"
```bash
# Usar porta diferente
./run-api.sh 8080

# Ou matar processo usando a porta
lsof -ti:7001 | xargs -r kill -9
```

#### ❌ "Variáveis não estão sendo carregadas"

Verifique se o arquivo `.env` está na raiz do projeto:
```bash
# Verificar localização
pwd
ls -la .env

# Teste manual de carregamento
cat .env | grep API_PORT
```

#### ❌ "Configurações misturadas (.env + variáveis de sistema)"

A aplicação prioriza variáveis de sistema sobre `.env`. Para usar apenas `.env`:

```bash
# Limpar variáveis de ambiente
unset API_PORT API_URL

# Execute normalmente
dotnet run
```

#### ✅ Debug de Configuração Avançado

```bash
# Ver exatamente quais variáveis estão definidas
env | grep -E "(API_|DATABASE_|JWT_)"

# Testar com verbose
API_PORT=8888 dotnet run --verbosity detailed

# Verificar logs de debug
dotnet run 2>&1 | grep -E "(✓|⚠|📍|🌐)"
```

### 🔍 Status de Configuração

| Condição | Status | Ação |
|----------|--------|------|
| `.env` existe + sem variáveis de sistema | ✅ Usa `.env` | Execute `dotnet run` |
| `.env` existe + variáveis de sistema definidas | ⚡ Usa variáveis de sistema | Execute `export VAR=value && dotnet run` |
| Sem `.env` + sem variáveis | ❌ Erro | Execute `cp .env.example .env` |
| Verificação `--check` falha | 🔧 Problema de configuração | Corrija conforme output do check |

🏥 **Especialização hospitalar** - Validações específicas para ambientes de saúde  
🔒 **Segurança enterprise** - Criptografia, auditoria e conformidade regulatória  
⚡ **Performance otimizada** - Arquitetura escalável e performática  
🧪 **Qualidade garantida** - Testes automatizados e documentação completa  
🚀 **Pronto para produção** - Deployment simplificado e monitoramento integrado

**Desenvolvido com excelência técnica para atender as demandas críticas do ambiente hospitalar.**

---

*CadPlus ERP Solutions - Inovação em tecnologia para saúde*# lekto-back
