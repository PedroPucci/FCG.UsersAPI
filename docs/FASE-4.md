# Fase 4 - FCG.UsersAPI

## Objetivo

Preparar o microsservico de usuarios para integracao
continua, entrega continua e implantacao em Kubernetes
gerenciado.

## Tecnologias

- .NET 8
- Docker
- Kubernetes
- GitHub Actions
- AWS ECR e Amazon EKS (implantacao pendente)

## Funcionalidades implementadas

### Testes automatizados
16 testes aprovados localmente em configuracao Release.

### Docker
Build da imagem Docker validado localmente.

### CI/CD
Workflow GitHub Actions preparado para build, testes
e construcao da imagem Docker.

Jobs de publicacao no Amazon ECR e implantacao no
Amazon EKS preparados, mas desabilitados.

### Kubernetes Secrets
Arquivos de exemplo disponiveis:

- k8s/rabbitmq-secret.example.yaml
- k8s/secret.example.yaml
- k8s/users-sqlserver-secret.example.yaml

Os arquivos privados correspondentes devem ser
configurados localmente e nao enviados ao Git.

## Pendencias

- Validar pipeline no GitHub Actions.
- Revisar historico Git quanto a credenciais antigas.
- Configurar recursos AWS.
- Publicar imagem no Amazon ECR.
- Implantar no Amazon EKS.
- Demonstrar Rolling Update automatico.
- Produzir video de demonstracao.
