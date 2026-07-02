# Arquitetura e Diretrizes Técnicas

## Visão Geral

A aplicação será desenvolvida utilizando **.NET 10**, seguindo princípios de arquitetura limpa (Clean Architecture), SOLID e boas práticas de desenvolvimento, visando escalabilidade, desacoplamento, testabilidade e fácil manutenção.

---

# Stack Tecnológica

| Tecnologia | Descrição |
|------------|-----------|
| .NET 10 | Framework principal da aplicação |
| ASP.NET Core Minimal API | Exposição dos endpoints da API |
| Entity Framework Core | ORM para acesso ao banco de dados |
| SQL Server | Banco de dados relacional |
| FluentValidation | Validação das requisições |
| RabbitMQ | Comunicação assíncrona entre serviços |
| Grafana + Loki | Observabilidade e centralização de logs |
| JWT | Autenticação |
| BCrypt | Hash de senhas |

---

# Arquitetura da Solução

A solução deverá seguir uma arquitetura em camadas.

```
ONG.Donation.sln
│
├── ONG.Donation.WebAPI
├── ONG.Donation.Application
├── ONG.Donation.Domain
├── ONG.Donation.Infrastructure
└── ONG.Donation.Worker
```

---

# Estrutura dos Projetos

## ONG.Donation.WebAPI

Responsável pela exposição dos endpoints da aplicação.

### Responsabilidades

- Configuração da aplicação
- Minimal APIs
- Configuração de DI
- Middleware
- Autenticação JWT
- Autorização
- Swagger
- Configuração do FluentValidation
- Configuração dos Logs
- Configuração do RabbitMQ

**Não deve conter regra de negócio.**

---

## ONG.Donation.Application

Responsável pelos casos de uso da aplicação.

### Responsabilidades

- Commands
- Queries
- DTOs
- Interfaces
- Services
- Validators
- Mapeamentos
- Casos de uso

Toda a lógica de aplicação deverá estar concentrada nesta camada.

---

## ONG.Donation.Domain

Responsável pelas regras de negócio.

### Responsabilidades

- Entidades
- Value Objects
- Enums
- Interfaces de domínio
- Regras de negócio
- Exceptions de domínio

Esta camada **não deve possuir dependência de nenhuma outra camada**.

---

## ONG.Donation.Infrastructure

Responsável pela implementação das dependências externas.

### Responsabilidades

- Entity Framework Core
- DbContext
- Entity Configurations
- Repositórios
- RabbitMQ
- Serviços externos
- Persistência
- Autenticação
- Logs

---

## ONG.Donation.Worker

Aplicação responsável pelo processamento de pagamentos.

### Responsabilidades

- Consumir mensagens do RabbitMQ
- Processar pagamentos
- Atualizar status das doações
- Registrar logs
- Publicar eventos quando necessário

---

# Banco de Dados

Será utilizado:

- SQL Server
- Entity Framework Core

## Configuração

O mapeamento das entidades deverá utilizar exclusivamente:

- **EntityTypeConfiguration**

Exemplo:

```csharp
public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        // configuração
    }
}
```

**Não utilizar Data Annotations para configuração do banco.**

---

# Minimal APIs

Toda exposição da API deverá utilizar **Minimal APIs**.

Exemplo da organização:

```
Endpoints
│
├── CampaignEndpoints.cs
├── DonationEndpoints.cs
├── AuthEndpoints.cs
└── DonorEndpoints.cs
```

Cada endpoint deverá possuir sua própria extensão.

Exemplo:

```csharp
app.MapCampaignEndpoints();
```

---

# FluentValidation

Todas as validações deverão utilizar **FluentValidation**.

Não utilizar validações diretamente nos endpoints.

Exemplo:

```
Validators
│
├── CreateCampaignValidator
├── CreateDonationValidator
└── RegisterDonorValidator
```

---

# Entity Framework

Utilizar:

- Migrations
- IEntityTypeConfiguration
- Repository Pattern (quando necessário)
- Unit of Work (opcional)

Evitar consultas desnecessárias e utilizar consultas assíncronas.

---

# RabbitMQ

A comunicação entre os módulos deverá ocorrer de forma assíncrona.

## Fluxo

```
WebAPI
    │
    │ Publica Evento
    ▼
RabbitMQ
    │
    ▼
Worker
    │
    ▼
Processamento do pagamento
    │
    ▼
Atualização da Doação
```

### Eventos sugeridos

- DonationCreatedEvent
- DonationPaymentProcessedEvent
- DonationPaymentFailedEvent

---

# Logs

Toda a aplicação deverá possuir logs estruturados.

## Stack

- Serilog
- Loki
- Grafana

Os logs deverão registrar:

- Requisições
- Erros
- Exceções
- Processamento de pagamentos
- Publicação de eventos
- Consumo de eventos

Os logs deverão conter informações como:

- CorrelationId
- RequestId
- Usuário
- Endpoint
- Data/Hora
- Tempo de execução

---

# Princípios SOLID

Toda implementação deverá seguir os princípios SOLID.

## Single Responsibility Principle (SRP)

Cada classe deverá possuir apenas uma responsabilidade.

## Open Closed Principle (OCP)

As classes deverão estar abertas para extensão e fechadas para modificação.

## Liskov Substitution Principle (LSP)

As abstrações deverão permitir substituição por suas implementações.

## Interface Segregation Principle (ISP)

Interfaces pequenas e específicas.

## Dependency Inversion Principle (DIP)

Toda dependência deverá ocorrer por abstrações.

---

# Injeção de Dependência

Utilizar o container nativo do .NET.

Toda dependência deverá ser registrada através de métodos de extensão.

Exemplo:

```
builder.Services
    .AddApplication()
    .AddInfrastructure(configuration)
    .AddAuthentication()
    .AddRabbitMq();
```

---

# Segurança

A autenticação deverá utilizar:

- JWT Bearer Authentication

As senhas deverão ser armazenadas utilizando:

- BCrypt

O acesso aos endpoints deverá utilizar RBAC.

Perfis:

- GestorONG
- Doador

---

# Convenções

## Namespace

```
ONG.Donation.*
```

## Idioma

- Código em inglês.
- Comentários apenas quando estritamente necessários.

## Classes

Utilizar nomes claros.

Exemplo:

- Campaign
- Donation
- Donor
- User
- PaymentProcessor

---

# Estrutura sugerida

```
ONG.Donation.Application
│
├── Commands
├── Queries
├── DTOs
├── Interfaces
├── Services
├── Validators
├── Behaviors
├── Mappings
└── DependencyInjection

ONG.Donation.Domain
│
├── Entities
├── Enums
├── ValueObjects
├── Events
├── Interfaces
├── Exceptions
└── Common

ONG.Donation.Infrastructure
│
├── Persistence
│   ├── Context
│   ├── Configurations
│   ├── Migrations
│   └── Repositories
├── Authentication
├── RabbitMQ
├── Logging
├── Services
└── DependencyInjection

ONG.Donation.WebAPI
│
├── Endpoints
├── Extensions
├── Middlewares
├── Filters
├── Authentication
└── Program.cs

ONG.Donation.Worker
│
├── Consumers
├── Services
├── Workers
├── Extensions
└── Program.cs
```

---

# Objetivos da Arquitetura

- Código limpo e organizado.
- Baixo acoplamento.
- Alta coesão.
- Facilidade de manutenção.
- Escalabilidade.
- Facilidade para criação de testes.
- Separação clara das responsabilidades.
- Comunicação assíncrona entre os módulos utilizando RabbitMQ.
- Observabilidade completa através de logs centralizados no Grafana/Loki.
- Utilização das melhores práticas do ecossistema .NET 10.