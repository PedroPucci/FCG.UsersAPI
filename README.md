# FCG Users API

# **Descrição do projeto**

O FCG Users API é o microsserviço desenvolvido em .NET 8 responsável pelo gerenciamento de usuários da plataforma FCG (FIAP Cloud Games). A aplicação segue os princípios de Clean Architecture, separação de responsabilidades e boas práticas de desenvolvimento backend.

Este serviço é responsável por:

* Cadastro de usuários
* Autenticação de usuários
* Geração de token JWT
* Autorização baseada em perfis
* Gerenciamento de usuários
* Publicação de eventos de integração
* Persistência de dados
* Validação de regras de negócio

---

# Responsabilidades do Microsserviço

### Cadastro de Usuários

Permite registrar novos usuários na plataforma.

Ao realizar um cadastro com sucesso, o serviço publica o evento:

```text
UserCreatedEvent
```

Esse evento será consumido pelo microsserviço de Notificações para envio de um e-mail de boas-vindas ao usuário cadastrado.

### Autenticação

Permite que usuários realizem login utilizando e-mail e senha.

Após autenticação bem-sucedida, é gerado um token JWT utilizado pelos demais microsserviços da plataforma.

### Autorização

A API utiliza autorização baseada em roles e os perfis disponíveis são:

* Admin
* User

### Integração entre Microsserviços

O UsersAPI participa da arquitetura orientada a eventos da plataforma, publicando eventos para integração assíncrona entre os serviços.

---

# Eventos Publicados

### UserCreatedEvent

Evento publicado após o cadastro de um novo usuário.

Exemplo:

```json
{
  "userId": "guid",
  "name": "Pedro",
  "email": "pedro@email.com"
}
```

Consumidor:

```text
NotificationsAPI
```

Objetivo:

```text
Enviar e-mail de boas-vindas ao usuário cadastrado.
```

---

# Fluxo de Integração

```text
UsersAPI
    ↓
UserCreatedEvent
    ↓
NotificationsAPI
    ↓
Envio de e-mail de boas-vindas
```

---

# **Regras de Negócio**

De acordo com os requisitos do desafio, o sistema implementa as seguintes regras:

### **Usuário**

* O usuário deve possuir:

  * Nome
  * E-mail
  * Senha

### **Validação de E-mail**

* O e-mail deve seguir um formato válido.

Exemplo:

```text
usuario@email.com
```

### **Validação de Senha**

* A senha deve conter:

  * No mínimo 8 caracteres
  * Pelo menos uma letra
  * Pelo menos um número
  * Pelo menos um caractere especial

### **Cadastro de Usuário**

* Não é permitido cadastrar usuários com dados inválidos.
* Não é permitido cadastrar usuários com e-mail duplicado.

---

## **Recursos do Projeto**

* **ASP.NET Core 8**
* **Entity Framework Core**
* **SQL Server**
* **ASP.NET Core Identity**
* **JWT Authentication**
* **FluentValidation**
* **Serilog**
* **Swagger/OpenAPI**
* **Unit of Work**
* **Repository Pattern**
* **Docker**
* **Xunit**
* **FluentAssertions**
* **Migrations**

---

## **Variáveis de Ambiente**

A aplicação utiliza as seguintes variáveis de ambiente:

| Variável                             | Descrição                                                |
| ------------------------------------ | -------------------------------------------------------- |
| ConnectionStrings__DefaultConnection | String de conexão com o SQL Server                       |
| Jwt__Secret                          | Chave utilizada para geração e validação dos tokens JWT  |
| Jwt__Issuer                          | Emissor do token JWT                                     |
| Jwt__Audience                        | Público do token JWT                                     |
| RunMigrations                        | Define se as migrations serão executadas automaticamente |

---

# Endpoints Disponíveis

### Usuários

```http
POST /api/users
```

Cadastrar usuário.

```http
GET /api/users/{id}
```

Consultar usuário por identificador.

```http
PUT /api/users/{id}
```

Atualizar usuário.

```http
DELETE /api/users/{id}
```

Remover usuário.

### Autenticação

```http
POST /api/auth/login
```

Realizar autenticação e gerar token JWT.

---

## **Como Executar o Projeto**

### **1. Configuração Inicial do Banco de Dados**

1. Faça o clone do projeto.
2. Configure a string de conexão do banco de dados.
3. Execute a aplicação.

As migrations serão aplicadas automaticamente durante a inicialização do sistema quando a configuração `RunMigrations` estiver habilitada.

---

### **2. Executando o Projeto**

1. Abra o projeto no Visual Studio 2022 ou em outro IDE de sua escolha.

2. Configure o projeto principal para execução:

   * Clique com o botão direito no projeto **FCG.UsersAPI** e selecione `Set as Startup Project`.

3. Clique no botão **HTTPS** para iniciar a aplicação.

---

### **3. Execução com Docker**

#### **Pré-requisitos**

* Docker Desktop instalado.

#### **Executando a Aplicação**

Na raiz do projeto execute:

```bash
docker compose up --build
```

Esse comando irá:

* Construir a imagem da API.
* Criar o container da aplicação.
* Criar o container do SQL Server.
* Executar as migrations automaticamente.

#### **Acessando a API**

Após a inicialização dos containers:

```text
http://localhost:8080/swagger
```

#### **Parando os Containers**

```bash
docker compose down
```

---

### **4. Banco de Dados**

#### Centralização de Exceções

Implementada a classe:

```text
ExceptionMiddleware
```

Responsável por centralizar o tratamento de exceções da aplicação.

#### Mensagens de Erro

Quando o banco estiver indisponível:

```text
The database is currently unavailable. Please try again later.
```

Para erros inesperados:

```text
An unexpected error occurred. Please contact support if the problem persists.
```

---

### **5. Configuração do Log**

* O sistema gera logs estruturados diariamente.
* Os logs registram eventos de negócio, erros e operações da aplicação.

Formato:

```text
Arquivo diário contendo informações estruturadas.
```

---

### **6. Estrutura da Solução**

```text
FCG.UsersAPI
│
├── docs
├── src
│   ├── FCG.UsersAPI
│   ├── FCG.UsersAPI.Application
│   ├── FCG.UsersAPI.Domain
│   ├── FCG.UsersAPI.Infrastructure
│   └── FCG.UsersAPI.Shared
│
├── tests
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

### **7. Finalização**

Após seguir as etapas anteriores, a aplicação estará disponível para utilização através do Swagger.

```text
http://localhost:8080/swagger
```

A interface permitirá explorar, autenticar e testar todos os endpoints disponibilizados pelo microsserviço.

---
