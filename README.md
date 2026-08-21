# Korp - Sistema de Emissao de Notas Fiscais

Projeto tecnico com frontend em Angular e backend em C# (.NET 8), separado em dois microsservicos:

- EstoqueService: cadastro de produtos e controle de saldo
- FaturamentoService: criacao/impressao de notas fiscais

A aplicacao atende o fluxo do desafio: cadastrar produtos, criar nota com itens, imprimir nota (fechar status) e debitar estoque.

## Arquitetura

O projeto roda com 3 containers:

- estoque-service
- faturamento-service
- frontend

Obs.: os microsservicos de negocio sao Estoque e Faturamento. O frontend e a camada de interface.

## Tecnologias

- Frontend: Angular 17, RxJS, Angular Material
- Backend: ASP.NET Core Web API (.NET 8), Entity Framework Core, SQLite
- Resiliencia entre servicos: Polly (retry + circuit breaker)
- Documentacao de API: Swagger
- Orquestracao: Docker Compose

## Requisitos

Para rodar com Docker (recomendado):

- Docker Desktop
- Docker Compose

Para rodar sem Docker:

- .NET SDK 8
- Node.js 20+
- Angular CLI (opcional, pode usar npx)

## Como iniciar (Docker)

Na raiz do projeto:

```bash
docker compose up --build
```

Acesso:

- Frontend: http://localhost:4200
- Swagger Estoque: http://localhost:5001/swagger
- Swagger Faturamento: http://localhost:5002/swagger

Para parar:

```bash
docker compose down
```

Se quiser remover tambem os volumes de banco:

```bash
docker compose down -v
```

## Como iniciar (sem Docker)

### 1) Backend - EstoqueService

```bash
cd backend/EstoqueService
dotnet run
```

Sobe em: http://localhost:5001

### 2) Backend - FaturamentoService

Em outro terminal:

```bash
cd backend/FaturamentoService
dotnet run
```

Sobe em: http://localhost:5002

### 3) Frontend

Em outro terminal:

```bash
cd frontend
npm install
npm start
```

Sobe em: http://localhost:4200

## Fluxo rapido de teste

1. Acessar Produtos e cadastrar ao menos 2 produtos
2. Ir em Notas Fiscais e criar uma nova nota com itens
3. Abrir a nota criada e clicar em Imprimir nota fiscal
4. Confirmar que:
   - status muda de Aberta para Fechada
   - saldo dos produtos e atualizado

## Simulacao de falha (requisito do desafio)

Na tela de detalhe da nota existe um controle para simular indisponibilidade do EstoqueService.

Com a falha ativa:

- tentativa de impressao retorna erro amigavel
- nota permanece Aberta

Ao restaurar o servico:

- a impressao volta a funcionar
- nota fecha normalmente

## Endpoints principais

EstoqueService:

- GET /api/produtos
- POST /api/produtos
- POST /api/estoque/baixar
- POST /api/debug/falhar
- POST /api/debug/restaurar
- GET /api/debug/status

FaturamentoService:

- GET /api/notas-fiscais
- POST /api/notas-fiscais
- POST /api/notas-fiscais/{id}/imprimir

## Observacoes

- Os dados sao persistidos em SQLite.
- Em Docker, os bancos ficam nos volumes do compose.
- O frontend esta configurado para consumir:
  - http://localhost:5001/api
  - http://localhost:5002/api

Se abrir o frontend por outro host (ex.: 127.0.0.1 ou IP da rede), pode haver bloqueio de CORS conforme configuracao atual do backend.
