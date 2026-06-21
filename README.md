# FCG Users API
# **Descrição do projeto**
O FCG Users API é o microsserviço utilizando .NET 8, seguindo os princípios de Clean Architecture, separação de responsabilidades e boas práticas de desenvolvimento. Esse microserviço é responsável pelo gerenciamento de usuários da plataforma FCG (FIAP Cloud Games).
Este serviço é responsável por:
- Cadastro de usuários
- Autenticação de usuários
- Geração de token JWT
- Autorização baseada em perfis
- Publicação de eventos de usuário criado
- Persistência de dados
- Validação de regras de negócio

# Responsabilidades do Microsserviço
### Cadastro de Usuários
Permite registrar novos usuários na plataforma.
Ao realizar um cadastro com sucesso, o serviço publica o evento:

```text
UserCreatedEvent
```

### Autenticação
Permite que usuários realizem login utilizando: email e senha.

### Autorização
A API utiliza autorização baseada em roles: Perfis disponíveis: Admin e User

# **Regras de Negócio**
De acordo com os requisitos do desafio, o sistema implementa as seguintes regras:

### **Usuário**
- O usuário deve possuir: Nome, E-mail e Senha.

### **Validação de E-mail**
- O e-mail deve seguir um formato válido (ex: usuario@email.com)

### **Validação de Senha**
- A senha deve conter:
  - No mínimo 8 caracteres
  - Pelo menos uma letra
  - Pelo menos um número
  - Pelo menos um caractere especial

### **Cadastro de Usuário**
- Não é permitido cadastrar usuários com dados inválidos
---
## **Tecnologias**
- .NET 8.0
- Entity Framework Core
- SQL Server

## **Ferramentas**
- Visual Studio 2022
- SQL Server Management Studio (SSMS)
- Git / Git Bash
---
## **Recursos do Projeto**
- **Serilog**: Para geração e gerenciamento de logs.
- **FluentValidator**: Para validação de dados e regras de negócios.
- **Entity Framework (ORM)**: Para mapeamento e interação com o banco de dados.
- **Unit of Work**: Padrão de design para gerenciar transações e persistência de dados de forma coesa.
- **Migrations**: Gerenciamento de alterações no banco de dados.
- **Xunit**: Para criação de testes unitários. 3A's (Arrange, Act, Assert).
- **FluentAssertions**: Melhor legibilidade nas validações.
- **ASP.NET Core Identity**: Implementação de autenticação e autorização baseada em identidade, com gerenciamento de usuários, roles e controle de acesso seguro à aplicação.
---
## **Como Executar o Projeto**
### **1. Configuração Inicial do Banco de Dados**
1. Faça o clone do projeto.
2. Verifique se a pasta `Migrations` no projeto está vazia. Caso contrário, delete todos os arquivos dessa pasta.   
3. Execute os seguintes comandos no **Package Manager Console**:
   - Certifique-se de selecionar o projeto relacionado ao banco de dados no menu "Default project".
   - Execute:
     ```bash
     add-migration PrimeiraMigracao
     update-database
     ```
   - Isso criará e configurará o banco de dados no Microsoft SQL Server.
---
### **2. Executando o Projeto**
1. Abra o projeto no Visual Studio 2022.
2. Configure o projeto principal para execução:
   - Clique com o botão direito no projeto **FCG.Platform** e selecione `Set as Startup Project`.
3. Clique no botão **HTTPS** no menu superior para iniciar a aplicação.

### **3. Banco de Dados**
- **Centralização de Exceções:**  
  Implementada a classe `ExceptionMiddleware` para unificar o tratamento de erros no sistema.
- **Alterações Realizadas:**  
  Ajustadas as classes `Program` e `RepositoryUoW` para integrar o middleware.
- **Mensagens de Erro:**  
  - Se o banco de dados não existir, os endpoints retornam:  
    ```text
    The database is currently unavailable. Please try again later.
    ```
  - Para erros inesperados na criação do banco, é exibido:  
    ```text
    An unexpected error occurred. Please contact support if the problem persists.
    ```
---
### **4. Configuração do Log**
- O sistema gera logs diários com informações sobre os processos executados no projeto.
- O log será salvo no diretório:  
  `C://Users//User//Downloads//FCG-Platform-logs`.  
  **Nota**: É necessário criar a pasta manualmente nesse caminho ou alterar o diretório no código, caso deseje personalizá-lo.

  **Formato do arquivo de log criado**:
- Arquivo diário com informações estruturadas.
---
### **5. Finalização**
- Após seguir as etapas anteriores, o sistema será iniciado, e uma página com a interface **Swagger** será aberta automaticamente no navegador configurado no Visual Studio. Essa página permitirá explorar e testar os endpoints da API.
---
