
---
name: integration-tests
description: Analisa, implementa e corrige testes de integração em C# e ASP.NET Core usando xUnit, Testcontainers, SQL Server e Service Bus. Use para cobertura de endpoints, correção de testes falhando, análise de fixtures e criação de cenários. Preserve a estrutura existente.
---

# Integration Tests — MyRecipeBook

## Objetivo
Garantir que os testes de integração validem corretamente o comportamento esperado da aplicação, sem introduzir complexidade desnecessária.

## Contexto
O projeto utiliza:
- xUnit
- Shouldly
- Moq
- Bogus
- WebApplicationFactory
- Testcontainers para SQL Server e Service Bus
- EF Core

A estrutura conhecida possui:
- IntegrationTestCollection
- ServiceBusIntegrationTestCollection
- MyRecipeBookApplicationFactory
- ServiceBusApplicationFactory

Confirme os nomes e as implementações no repositório antes de utilizá-los.

## Procedimento

### Etapa 1 — Investigação
1. Localize o endpoint ou UseCase.
2. Analise seus contratos, validações e respostas.
3. Localize os testes equivalentes existentes.
4. Identifique fixtures, builders e helpers reutilizáveis.
5. Determine os cenários que realmente precisam ser testados.

### Etapa 2 — Análise
Verifique:
- Fluxo de sucesso.
- Entradas inválidas.
- Autenticação e autorização.
- Estados inexistentes ou expirados.
- Persistência e alterações no banco.
- Efeitos colaterais.
- Transações.
- Comportamento HTTP esperado.

Não invente contratos ou respostas.

### Etapa 3 — Implementação
Quando o usuário solicitar criação ou correção:
1. Reutilize as fixtures existentes.
2. Siga os padrões de nomenclatura do projeto.
3. Escreva testes independentes.
4. Utilize asserções que comprovem o comportamento.
5. Evite esperas arbitrárias e dependências temporais frágeis.
6. Preserve isolamento entre testes.

### Etapa 4 — Validação
- Execute primeiro os testes afetados, quando possível.
- Se necessário, amplie para a coleção relevante.
- Compare falhas antes e depois.
- Investigue a causa raiz antes de alterar asserções.
- Nunca altere o comportamento esperado apenas para fazer o teste passar.

## Delegação
Para tarefas complexas, use até dois subagentes:

Agente 1 — Investigar cenários e qualidade da cobertura.

Agente 2 — Investigar fixtures, infraestrutura e causas de falhas.

O agente principal deve consolidar as descobertas e realizar a implementação solicitada.

Evite edição simultânea dos mesmos arquivos.

## Restrições
- NÃO criar novas fixtures sem necessidade comprovada.
- NÃO duplicar infraestrutura existente.
- NÃO modificar código de produção sem autorização.
- NÃO alterar contratos da API.
- NÃO enfraquecer asserções para obter testes verdes.
- NÃO remover testes existentes sem justificativa e autorização.
- NÃO criar mocks desnecessários.
- NÃO executar indiscriminadamente toda a suíte se uma validação direcionada for suficiente.

## Formato de entrega
Apresente:
1. Cenários analisados.
2. Testes criados ou modificados.
3. Justificativa das alterações.
4. Resultado da execução.
5. Falhas restantes e suas causas conhecidas.

Se não foi possível executar os testes, informe explicitamente.
