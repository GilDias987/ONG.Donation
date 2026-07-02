# Requisitos do Sistema de Gestão de Campanhas para ONG

## Visão Geral

O sistema tem como objetivo permitir o gerenciamento de campanhas de arrecadação por uma ONG, possibilitando o cadastro de doadores, autenticação de usuários, realização de doações e disponibilização de um painel público de transparência.

---

# Requisitos Funcionais

## 1. Autenticação e Autorização (RBAC)

### Autenticação

- O sistema deve utilizar autenticação baseada em **JWT (JSON Web Tokens)**.
- Após a autenticação, o usuário deve receber um token válido para acessar os endpoints protegidos.

### Perfis de Usuário (Roles)

Devem existir dois perfis distintos:

- **GestorONG**
- **Doador**

### Regras de Autorização

- Apenas usuários com a role **GestorONG** podem acessar endpoints administrativos.
- Usuários com a role **Doador** podem realizar doações.
- Endpoints públicos não exigem autenticação.

---

## 2. Gestão de Campanhas

**Acesso:** Apenas usuários com a role **GestorONG**.

### Cadastro e Edição

O sistema deve permitir criar e editar campanhas contendo obrigatoriamente:

| Campo | Tipo |
|--------|------|
| Título | String |
| Descrição | String |
| DataInicio | DateTime |
| DataFim | DateTime |
| MetaFinanceira | Decimal |
| Status | Enum |

### Status permitidos

- Ativa
- Concluída
- Cancelada

### Regras de Negócio

- A data de término não pode estar no passado.
- A meta financeira deve ser maior que zero.

---

## 3. Cadastro de Doador

**Acesso:** Público.

O sistema deve permitir o cadastro de novos doadores contendo:

| Campo | Validação |
|--------|-----------|
| Nome Completo | Obrigatório |
| Email | Obrigatório e único |
| CPF | Obrigatório e com formato válido |
| Senha | Obrigatória |

### Regras de Negócio

- O e-mail deve ser único no banco de dados.
- O CPF deve possuir formato válido.
- A senha deve ser armazenada utilizando hash seguro (ex.: BCrypt).

---

## 4. Painel de Transparência

**Acesso:** Público.

O sistema deve disponibilizar um endpoint para consulta pública das campanhas.

### Critérios

Devem ser retornadas apenas campanhas com status:

- Ativa

### Dados retornados

Para cada campanha, devem ser exibidos:

- Título
- Meta Financeira
- Valor Total Arrecadado

### Cálculo

O campo **Valor Total Arrecadado** deve ser calculado com base na soma das doações processadas para a campanha.

---

## 5. Processo de Doação

**Acesso:** Apenas usuários autenticados com a role **Doador**.

O doador deve poder registrar uma intenção de doação informando:

| Campo | Tipo |
|--------|------|
| IdCampanha | Inteiro |
| ValorDoacao | Decimal |

### Regras de Negócio

- Não é permitido realizar doações para campanhas:
  - Concluídas
  - Canceladas
- Apenas campanhas ativas podem receber doações.

---

# Requisitos Não Funcionais

## Segurança

- Utilização de JWT para autenticação.
- Armazenamento seguro das senhas utilizando BCrypt.
- Controle de acesso baseado em Roles (RBAC).

## Banco de Dados

- O e-mail do doador deve possuir restrição de unicidade.
- As campanhas e doações devem possuir relacionamento adequado para cálculo do valor arrecadado.

## API

A API deverá seguir boas práticas REST, retornando códigos HTTP apropriados, como:

- `200 OK`
- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

# Resumo dos Perfis

| Funcionalidade | Público | Doador | GestorONG |
|----------------|---------|---------|------------|
| Cadastro de doador | ✅ | ✅ | ✅ |
| Login | ✅ | ✅ | ✅ |
| Listar campanhas ativas | ✅ | ✅ | ✅ |
| Realizar doação | ❌ | ✅ | ❌ |
| Criar campanha | ❌ | ❌ | ✅ |
| Editar campanha | ❌ | ❌ | ✅ |

---

# Resumo das Regras de Negócio

- Utilizar autenticação via JWT.
- Implementar autorização baseada em Roles (RBAC).
- Apenas GestorONG pode gerenciar campanhas.
- Apenas Doador autenticado pode realizar doações.
- A campanha deve possuir meta financeira maior que zero.
- A data de término da campanha não pode estar no passado.
- Apenas campanhas ativas podem receber doações.
- O painel público deve listar apenas campanhas ativas.
- O valor arrecadado deve ser calculado pela soma das doações.
- O e-mail do doador deve ser único.
- O CPF deve possuir formato válido.
- A senha deve ser armazenada utilizando BCrypt.