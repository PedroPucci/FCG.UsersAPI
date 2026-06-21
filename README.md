# FCG Users API

# **Descrição do projeto**

O FCG Users API é o microsserviço utilizando .NET 8, seguindo os princípios de Clean Architecture, separação de responsabilidades e boas práticas de desenvolvimento. Esse microserviço é responsável pelo gerenciamento de usuários da plataforma FCG (FIAP Cloud Games).

Este serviço é responsável por:

* Cadastro de usuários
* Autenticação de usuários
* Geração de token JWT
* Autorização baseada em perfis
* Publicação de eventos de usuário criado
* Persistência de dados
* Validação de regras de negócio

# Responsabilidades do Microsserviço

### Cadastro de Usuários

Permite registrar novos usuários na plataforma.

Ao realizar um cadastro com sucesso, o serviço publica o evento:

```text
UserCreatedEvent
```

Esse evento será consumido pelo microsserviço de Notificações para envio de um e-mail de boas-vindas ao usuário cadastrado.

### Autenticação

Permite que usuários realizem login utilizando: e-mail e senha.

### Autorização

A API utiliza autorização baseada em roles e os perfis disponíveis são:

* Admin
* User

# **Regras de Negócio**

De acordo com os requisitos do desafio, o sistema implementa as seguintes regras:

### **Usuário**

* O usuário deve possuir: Nome, E-mail e Senha.

### **Validação de E-mail**

* O e-mail deve seguir um formato válido (ex: [usuario@email.com](mailto:usuario@email.com))

### **Validação de Senha**

* A senha deve conter:

  * No mínimo 8 caracteres
  * Pelo menos uma letra
  * Pelo menos um número
  * Pelo menos um caractere especial

### **Cadastro de Usuário**

* Não é permitido cadastrar usuários com dados inválidos.

---

## **Recursos do Projeto**

* **Serilog**: Para geração e gerenciamento de logs.
* **FluentValidation**: Para validação de dados e regras de negócios.
* **Entity Framework Core (ORM)**: Para mapeamento e interação com o banco de dados.
* **Unit of Work**: Padrão de design para gerenciar transações e persistência de dados de forma coesa.
* **Migrations**: Gerenciamento de alterações no banco de dados.
* **Xunit**: Para criação de testes unitários. 3A's (Arrange, Act, Assert).
* **FluentAssertions**: Melhor legibilidade nas validações.
* **ASP.NET Core Identity**: Implementação de autenticação e autorização baseada em identidade, com gerenciamento de usuários, roles e controle de acesso seguro à aplicação.
* **Swagger/OpenAPI**: Documentação automatizada dos endpoints da API.
* **Docker**: Containerização da aplicação e do banco de dados.

---

## **Variáveis de Ambiente**

A aplicação utiliza as seguintes variáveis de ambiente:

| Variável                             | Descrição                                                                 |
| ------------------------------------ | ------------------------------------------------------------------------- |
| ConnectionStrings__DefaultConnection | String de conexão com o SQL Server                                        |
| Jwt__Secret                          | Chave utilizada para geração e validação dos tokens JWT                   |
| Jwt__Issuer                          | Emissor do token JWT                                                      |
| Jwt__Audience                        | Público do token JWT                                                      |
| RunMigrations                        | Define se as migrations serão executadas automaticamente na inicialização |

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
3. Clique no botão **HTTPS** no menu superior para iniciar a aplicação.

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
* Executar as migrations automaticamente (quando configurado).

#### **Acessando a API**

Após a inicialização dos containers, a documentação Swagger estará disponível em:

```text
http://localhost:8080/swagger
```

#### **Parando os Containers**

```bash
docker compose down
```

---

### **4. Banco de Dados**

* **Centralização de Exceções:**
  Implementada a classe `ExceptionMiddleware` para unificar o tratamento de erros no sistema.

* **Alterações Realizadas:**
  Ajustadas as classes `Program` e `RepositoryUoW` para integrar o middleware.

* **Mensagens de Erro:**

  * Se o banco de dados não existir, os endpoints retornam:

```text
The database is currently unavailable. Please try again later.
```

* Para erros inesperados na criação do banco, é exibido:

```text
An unexpected error occurred. Please contact support if the problem persists.
```

---

### **5. Configuração do Log**

* O sistema gera logs diários com informações sobre os processos executados no projeto.

* O log será salvo no diretório:

```text
C://Users//User//Downloads//FCG-UserAPI-logs
```

**Nota:** É necessário criar a pasta manualmente nesse caminho ou alterar o diretório no código, caso deseje personalizá-lo.

**Formato do arquivo de log criado:**

* Arquivo diário com informações estruturadas.

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

* Após seguir as etapas anteriores, o sistema será iniciado e uma página com a interface **Swagger** será aberta automaticamente no navegador configurado.

* Essa página permitirá explorar e testar os endpoints disponibilizados pela API.

* Caso a aplicação esteja sendo executada via Docker, a documentação poderá ser acessada através do endereço:

```text
http://localhost:8080/swagger
```

---
