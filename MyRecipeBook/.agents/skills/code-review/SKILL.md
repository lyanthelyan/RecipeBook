
---
name: code-review
description: Realiza revisão técnica de código C# e .NET, identificando bugs, falhas de segurança, problemas de arquitetura, concorrência e lacunas de testes. Use quando o usuário solicitar code review, auditoria ou análise crítica de um UseCase, serviço ou endpoint. Não modifica código.
---

# Code Review — MyRecipeBook

## Objetivo
Realizar uma revisão técnica fundamentada no código existente, priorizando problemas reais, riscos e oportunidades relevantes de melhoria.

## Contexto
Projeto ASP.NET Core Web API em C#, com arquitetura em camadas:
- API
- Application
- Domain
- Infrastructure
- Communication
- Exceptions
- Token

Tecnologias utilizadas incluem EF Core, SQL Server, xUnit, Testcontainers, FluentValidation, Mapster e autenticação JWT.

Verifique as implementações reais antes de presumir contratos ou regras.

## Procedimento

1. Identifique o código solicitado e suas dependências diretas.
2. Consulte interfaces, implementações, entidades, validadores e testes relacionados.
3. Identifique as regras de negócio efetivamente implementadas.
4. Não presuma requisitos sem evidência.

## Delegação para subagentes

Para revisões completas ou complexas, solicite três subagentes independentes, executados em paralelo, quando disponíveis.

### Agente 1 — Lógica e segurança
Analise:
- Bugs e inconsistências lógicas.
- Validação de entradas.
- Tratamento de exceções.
- Autenticação e autorização.
- Transações e atomicidade.
- Concorrência, race conditions e idempotência.
- Estados inválidos e casos extremos.

### Agente 2 — Arquitetura
Analise:
- Respeito às responsabilidades das camadas.
- SOLID e Clean Architecture.
- Acoplamento desnecessário.
- Duplicação de código.
- Legibilidade e manutenção.
- Consistência com os padrões existentes.

Não recomende novas abstrações sem benefício concreto.

### Agente 3 — Testes
Analise:
- Cobertura dos comportamentos importantes.
- Cenários positivos e negativos.
- Qualidade das asserções.
- Testes que passam sem validar o comportamento correto.
- Casos extremos não cobertos.
- Possíveis testes instáveis ou dependentes da ordem de execução.

## Consolidação
O agente principal deve:
1. Aguardar os resultados disponíveis.
2. Eliminar duplicações.
3. Confrontar cada observação com o código.
4. Descartar falsos positivos.
5. Ordenar os problemas por gravidade.

Se os subagentes não estiverem disponíveis, executar a revisão sequencialmente.

## Formato de saída

Para cada problema:

- Severidade: Crítica / Alta / Média / Baixa.
- Arquivo e localização.
- Problema identificado.
- Evidência concreta no código.
- Impacto possível.
- Sugestão objetiva.

Ao final, apresentar:
- Resumo dos principais riscos.
- Lacunas de testes relevantes.
- Melhorias opcionais separadas dos defeitos.
- Pontos que não puderam ser verificados.

## Restrições
- NÃO modificar arquivos.
- NÃO implementar correções.
- NÃO criar contratos ou serviços.
- NÃO inventar regras de negócio.
- NÃO tratar preferências estilísticas como bugs.
- NÃO recomendar padrões apenas por preferência.
- NÃO afirmar que um bug existe sem evidência.
- NÃO executar comandos que alterem dados persistentes.

Priorize precisão e relevância em vez de quantidade de observações.
