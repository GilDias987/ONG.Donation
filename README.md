# 🫂 Conexão Solidária - ONG.Donation

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Azure Pipelines](https://img.shields.io/badge/CI%2FCD-Azure%20Pipelines-blue)](azure-pipelines.yml)

**Sistema de gestão de doações para a ONG Esperança Solidária** — plataforma que conecta doadores a campanhas com processamento assíncrono de pagamentos, autenticação JWT e infraestrutura cloud-native.

Desenvolvido como projeto de hackathon (Fase 5) pelo **Grupo 55**.

---

## Funcionalidades

- **Gestão de Campanhas** — CRUD completo de campanhas (restrito a gestores)
- **Cadastro de Doadores** — Registro público com validação de CPF e email único
- **Autenticação JWT** — Login com roles `GestorONG` e `Doador`
- **Doações Assíncronas** — Criação de doação → publicação em fila → processamento em background → retorno do resultado
- **Painel de Transparência** — Consulta pública de campanhas ativas com valores arrecadados
- **Monitoramento** — Logs centralizados via Serilog + Grafana Loki
- **Health Checks** — Endpoints `/health/live` e `/health/ready`

---

## Stack Tecnológica

| Categoria | Tecnologia |
|---|---|
| **Linguagem** | C# 13 (.NET 10) |
| **Framework** | ASP.NET Core 10 (Minimal APIs) |
| **ORM** | Entity Framework Core 10 |
| **Banco de Dados** | SQL Server 2022 |
| **Validação** | FluentValidation 12 |
| **Autenticação** | JWT Bearer |
| **Hashing** | BCrypt.Net-Next |
| **Mensageria** | Azure Service Bus / Emulator |
| **Logging** | Serilog + Grafana Loki |
| **Monitoramento** | Grafana |
| **Container** | Docker, Docker Compose |
| **Orquestração** | Kubernetes (AKS) |
| **CI/CD** | Azure Pipelines |
| **IaC** | Terraform |

---

## Arquitetura

O projeto segue os princípios de **Clean Architecture** com **Domain-Driven Design (DDD)**:

```
┌─────────────────────────────────────────────────┐
│           ONG.Donation.WebAPI (API)             │
│           ONG.Donation.Worker (Worker)          │
├─────────────────────────────────────────────────┤
│           ONG.Donation.Application              │
│  (Commands, Queries, Services, DTOs, Validação) │
├─────────────────────────────────────────────────┤
│         ONG.Donation.Infrastructure             │
│   (EF Core, Service Bus, JWT, Migrations)       │
├─────────────────────────────────────────────────┤
│           ONG.Donation.Domain                   │
│   (Entities, Enums, Events, Exceptions)         │
└─────────────────────────────────────────────────┘
```

### Fluxo de uma Doação

```
Doador → POST /donations
              │
              ▼
   WebAPI cria Donation (status: Pendente)
              │
              ▼
   Publica DonationCreatedEvent → Azure Service Bus (donation.payment)
                                   │
                                   ▼
                         Worker consome a fila
                                   │
                                   ▼
                    ProcessPaymentAsync (simulado)
                                   │
                    ┌──────────────┴──────────────┐
                    ▼                              ▼
          Sucesso → Processada            Falha → Falhou
                    │                              │
                    ▼                              ▼
         Publica DonationPayment        Publica DonationPayment
         ProcessedEvent                 FailedEvent
                    │                              │
                    ▼                              ▼
         Service Bus (donation.payment.result)
                    │
                    ▼
         WebAPI (ServiceBusPaymentEventConsumer)
                    │
                    ▼
         Atualiza status da Donation no banco
```

---

## Começando

### Pré-requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Azure Service Bus Emulator (iniciado via Docker Compose)

### Configuração

1. Clone o repositório:

```bash
git clone https://github.com/seu-usuario/ONG.Donation.git
cd ONG.Donation
```

2. Execute a infraestrutura local (SQL Server, Service Bus Emulator, Loki, Grafana):

```bash
docker-compose up -d
```

3. Execute as migrações do banco:

```bash
dotnet ef database update --project ONG.Donation.Infrastructure --startup-project ONG.Donation.WebAPI
```

4. Inicie a aplicação:

```bash
dotnet run --project ONG.Donation.WebAPI
```

5. (Opcional) Inicie o Worker:

```bash
dotnet run --project ONG.Donation.Worker
```

### Credenciais Padrão (Admin)

- **Email:** admin@gmail.com
- **Senha:** Carlos@987

---

## Endpoints da API

### Autenticação

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/auth/login` | Login e obtenção de token JWT |

### Doadores

| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| `POST` | `/donors/register` | — | Cadastro público |
| `PUT` | `/donors/{id}` | JWT | Atualizar perfil |
| `DELETE` | `/donors/{id}` | JWT | Excluir conta |

### Campanhas (requer role `GestorONG`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/campaigns` | Listar todas |
| `GET` | `/campaigns/{id}` | Obter por ID |
| `POST` | `/campaigns` | Criar |
| `PUT` | `/campaigns/{id}` | Atualizar |
| `PATCH` | `/campaigns/{id}/updateAmount` | Atualizar meta financeira |
| `PATCH` | `/campaigns/{id}/status` | Ativar/desativar |

### Doações (requer role `Doador`)

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/donations` | Criar doação |
| `GET` | `/donations/campaign/{campaignId}` | Listar por campanha (GestorONG) |

### Transparência (público)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/transparency/transparency/campaigns` | Campanhas ativas com total arrecadado |

### Health Checks

| Método | Rota |
|---|---|
| `GET` | `/health/live` |
| `GET` | `/health/ready` |

---

## Estrutura do Projeto

```
ONG.Donation/
├── ONG.Donation.Domain/           # Entidades, enums, eventos, exceções
├── ONG.Donation.Application/      # Services, commands, DTOs, validators
├── ONG.Donation.Infrastructure/   # EF Core, Service Bus, JWT, migrations
├── ONG.Donation.WebAPI/           # Minimal API, endpoints, middleware
├── ONG.Donation.Worker/           # Background consumer de filas
├── k8s/                           # Manifestos Kubernetes (AKS)
├── grafana/                       # Provisionamento Grafana Loki
├── docker-compose.yml             # Infraestrutura local
├── azure-pipelines.yml            # CI/CD Azure Pipelines
└── ONG.Donation.slnx              # Solution file
```

---

## CI/CD

O pipeline do Azure Pipelines (`.azure-pipelines.yml`) é acionado ao fazer push para `main`/`master` com mudanças nos projetos. O fluxo inclui:

1. **Build** — `dotnet restore`, `build`, `test`
2. **Docker** — Build e push da imagem para Azure Container Registry
3. **Deploy** — Aplica manifestos Kubernetes no AKS com substituição da tag da imagem
4. **Rollback automático** em caso de falha no rollout

---

## Deploy (Kubernetes)

Os manifestos em `k8s/` configuram:

- **Namespace:** `ong-donation`
- **Deployment:** 2 réplicas com rolling update
- **Horizontal Pod Autoscaler:** min 1, max 2 pods (CPU 70%, memória 80%)
- **Secrets:** Azure Key Vault via CSI Driver
- **Ingress:** NGINX
- **Probes:** Liveness, Readiness, Startup
- **Recursos:** 128Mi/100m request, 512Mi/500m limit (por pod)

### Infraestrutura como Código

O provisionamento dos recursos Azure (AKS, ACR, Key Vault, SQL Server, Service Bus) é gerenciado via **Terraform** em repositório separado (`fcg-infra/terraform/ong-donation/`).

---

## Monitoramento

- **Logs estruturados** com Serilog, enviados para:
  - Console
  - Grafana Loki (`http://localhost:3100`)
- **Labels:** `app` (ong-donation-webapi / ong-donation-worker), `env`
- **Grafana** disponível em `http://localhost:3000` (sem autenticação no ambiente local)
- **Request Logging Middleware** captura: método, path, correlation ID, usuário, status code, duração
- **Prometheus** annotations nos pods para coleta de métricas

---

## Equipe — Grupo 55

| RM       | Membro                                         |
|----------|------------------------------------------------|
| RM367747 | — Alexandre Araújo da Silva  / AlexandreAraujo |
| RM367560 | — Josegil Dias Frota Figueira / gildiasfrota   |
| RM367985 | — Miguel de Oliveira Gonçalves / miguel084     |

---

## Licença

Distribuído sob licença MIT. Veja [LICENSE](LICENSE) para mais informações.
