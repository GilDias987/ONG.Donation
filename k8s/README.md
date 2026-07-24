# ONG Donation WebAPI - Kubernetes Manifests

## Overview

Manifests Kubernetes para o deployment do ONG.Donation.WebAPI no Azure Kubernetes Service (AKS).

## Architecture

```
┌─────────────────────────────────────────────────┐
│             Ingress (nginx)                     │
│    TLS/HTTPS - Let's Encrypt Certificate       │
└──────────┬──────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│          ONG Donation WebAPI Service            │
│          (ClusterIP - Internal Load Balance)    │
└──────────┬──────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│    ONG Donation WebAPI Deployment (HPA)         │
│    - Replicas: 2-10 (Auto-scaled)              │
│    - CPU/Memory monitoring                     │
│    - Rolling updates                           │
│    - Pod disruption budgets                    │
└──────────┬──────────────────────────────────────┘
           │
        ┌──┴──────┬──────────┬──────────┐
        ▼         ▼          ▼          ▼
    [Pod 1]  [Pod 2]   [Pod N]    [Sidecars]
    ├─Logs                          
    ├─Secrets (Key Vault CSI)
    ├─Config (ConfigMap)
    └─Liveness/Readiness Probes
```

## Files

### 1. namespace.yaml
Define um namespace isolado para todos os recursos do ONG Donation.

**Recursos:**
- `Namespace`: ong-donation

### 2. configmap.yaml
Armazena configurações de aplicação em variáveis de ambiente.

**Recursos:**
- `ConfigMap`: ong-donation-config (todas as configurações não-sensíveis)
- `ConfigMap`: ong-donation-logging-config (configuração de logging)

**Variáveis incluídas:**
- ASPNETCORE_ENVIRONMENT
- Database configuration
- JWT configuration
- RabbitMQ configuration
- Loki configuration
- Feature flags

### 3. secretproviderclass.yaml
Integra com Azure Key Vault via CSI Driver (Secrets Store).

**Recursos:**
- `ServiceAccount`: ong-donation-webapi
- `AzureIdentity`: Workload Identity para autenticação sem credenciais
- `AzureIdentityBinding`: Liga o Workload Identity ao Pod
- `SecretProviderClass`: Define quais secrets serão injetados do Key Vault

**Secrets injetados:**
- db-connection-string
- jwt-key
- rabbitmq-connection-string
- service-bus-connection-string
- appinsights-connection-string

### 4. deployment.yaml
Deploy da aplicação WebAPI com configurações avançadas.

**Recursos:**
- `Deployment`: ong-donation-webapi
- `PodDisruptionBudget`: Garante mínimo de replicas disponíveis

**Características:**
- 2 replicas iniciais
- Rolling updates (maxSurge: 1, maxUnavailable: 0)
- Security context (non-root, read-only filesystem)
- Resource limits (256Mi-512Mi memory, 250m-500m CPU)
- Liveness, Readiness, Startup probes
- Pod anti-affinity (spread entre nodes)
- Graceful shutdown (15s sleep)
- Environment variables dinâmicas (POD_NAME, NODE_NAME, etc.)

### 5. service.yaml
Expõe a aplicação internamente no cluster.

**Recursos:**
- `Service`: ong-donation-webapi (ClusterIP - load balancer interno)
- `Service`: ong-donation-webapi-headless (para service discovery)

**Porta:** 80/TCP

### 6. ingress.yaml
Expõe a aplicação externamente com TLS.

**Recursos:**
- `NetworkPolicy`: Controla tráfego de rede (whitelist)
- `Ingress`: nginx-based com TLS
- `Certificate`: Gerenciado pelo cert-manager (Let's Encrypt)

**Hosts:**
- api.donation.ong.com (API completa)
- donation.ong.com (Endpoints públicos)
- www.donation.ong.com (Alias)

**Features:**
- TLS/HTTPS obrigatório
- CORS habilitado
- Rate limiting (100 req/min)
- WAF (ModSecurity) habilitado
- Proxy timeouts configurados

### 7. hpa.yaml
Horizontal Pod Autoscaler para escalar baseado em métricas.

**Configuração:**
- Min replicas: 2
- Max replicas: 10
- Métricas:
  - CPU: 70% de utilização
  - Memory: 80% de utilização
- Scale up: 100% em 30s
- Scale down: 50% em 60s com estabilização de 5min

## Deployment Steps

### Prerequisites
```bash
# Instalar cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Instalar nginx-ingress
helm install nginx-ingress ingress-nginx/ingress-nginx -n ingress-nginx --create-namespace

# Instalar Secrets Store CSI Driver
kubectl apply -f https://raw.githubusercontent.com/Azure/secrets-store-csi-driver-provider-azure/master/deployment/provider-azure-installer.yaml

# Instalar AAD Pod Identity
helm install aad-pod-identity aad-pod-identity/aad-pod-identity -n aad-pod-identity --create-namespace
```

### Deploy
```bash
# 1. Create namespace
kubectl apply -f namespace.yaml

# 2. Create ConfigMap
kubectl apply -f configmap.yaml

# 3. Setup Workload Identity and Secrets
# - Atualizar os placeholders em secretproviderclass.yaml:
#   - {SUBSCRIPTION_ID}
#   - {RESOURCE_GROUP}
#   - {CLIENT_ID}
#   - {TENANT_ID}
kubectl apply -f secretproviderclass.yaml

# 4. Create Ingress (requires ClusterIssuer)
# - Primeiro criar ClusterIssuer para Let's Encrypt
kubectl apply -f ingress.yaml

# 5. Deploy application
kubectl apply -f deployment.yaml

# 6. Expose service
kubectl apply -f service.yaml

# 7. Setup autoscaling
kubectl apply -f hpa.yaml
```

### ClusterIssuer (Let's Encrypt)
```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@ong.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

## Monitoramento

### Health Checks
```bash
# Liveness
curl http://localhost/health

# Readiness
curl http://localhost/health/ready

# Startup
curl http://localhost/health/live
```

### Logs
```bash
# Real-time logs
kubectl logs -f deployment/ong-donation-webapi -n ong-donation

# Previous logs (crashes)
kubectl logs --previous deployment/ong-donation-webapi -n ong-donation

# All containers
kubectl logs -f --all-containers=true deployment/ong-donation-webapi -n ong-donation
```

### Metrics
```bash
# Pod status
kubectl get pods -n ong-donation -o wide

# Deployment status
kubectl describe deployment ong-donation-webapi -n ong-donation

# HPA status
kubectl get hpa ong-donation-webapi-hpa -n ong-donation -w

# Resource usage
kubectl top pods -n ong-donation
kubectl top nodes
```

## Troubleshooting

### Pod não está iniciando
```bash
# Verificar logs
kubectl logs deployment/ong-donation-webapi -n ong-donation

# Verificar eventos
kubectl describe pod <pod-name> -n ong-donation

# Verificar secrets
kubectl get secret -n ong-donation
kubectl describe secret <secret-name> -n ong-donation
```

### Acesso ao Key Vault falhando
```bash
# Verificar Workload Identity binding
kubectl get aadidentity -n ong-donation
kubectl get aadidentitybinding -n ong-donation

# Verificar CSI Driver
kubectl get crd secretproviderclasses.secrets-store.csi.x-k8s.io

# Verificar logs do pod de secrets
kubectl logs -n aad-pod-identity deployment/aad-pod-identity-nmi
```

### Conexão do banco de dados falhando
```bash
# Verificar connectivity
kubectl run -it --rm debug --image=mcr.microsoft.com/powershell --restart=Never -- pwsh
# Dentro do pod: Test-NetConnection -ComputerName <sql-server> -Port 1433

# Verificar NSG rules
az network nsg rule list -g <resource-group> -n <nsg-name>
```

## Variables a configurar

No arquivo `secretproviderclass.yaml`, substituir os placeholders:
- `{SUBSCRIPTION_ID}`: Azure Subscription ID
- `{RESOURCE_GROUP}`: Nome do Resource Group
- `{CLIENT_ID}`: Client ID da User Managed Identity do AKS
- `{TENANT_ID}`: Azure Tenant ID
- `{ACR_LOGIN_SERVER}`: Login server do Azure Container Registry (deployment.yaml)

No arquivo `ingress.yaml`:
- Atualizar os nomes de domínio conforme necessário
- Criar ClusterIssuer antes de fazer deploy

## Best Practices aplicadas

1. **Security**
   - Non-root user (1000)
   - Read-only filesystem
   - Security policies
   - Network policies
   - Secret management via Key Vault

2. **Reliability**
   - Multiple replicas
   - Pod Disruption Budgets
   - Health checks (liveness, readiness, startup)
   - Graceful shutdown

3. **Performance**
   - Resource requests/limits
   - Pod anti-affinity
   - HPA com múltiplas métricas
   - Connection pooling

4. **Observability**
   - Structured logging
   - Application Insights
   - Prometheus metrics
   - Distributed tracing

5. **Scalability**
   - Horizontal Pod Autoscaler
   - Service mesh ready (Istio compatible)
   - Multi-zone deployment
   - Load balancing

## Próximas steps

1. Configurar monitoring e alerting
2. Implementar service mesh (Istio/Linkerd)
3. Setup CI/CD pipeline para build e push de images
4. Configurar backup e disaster recovery
5. Setup cost optimization policies
