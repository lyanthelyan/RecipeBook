
---
name: refactoring
description: Refatora código C# e .NET para melhorar legibilidade, organização e manutenção sem alterar comportamento. Use quando o usuário solicitar refatoração, extração de métodos, remoção de duplicação ou simplificação de UseCases e serviços. Respeite a arquitetura existente.
---

# Refactoring — MyRecipeBook

## Objetivo
Melhorar a qualidade interna do código preservando integralmente seu comportamento observável.

A refatoração deve ser mínima, objetiva e compatível com os padrões existentes.

## Princípios
- Simplicidade antes de abstração.
- Legibilidade antes de concisão.
- Consistência antes de preferência pessoal.
- Mudanças pequenas e verificáveis.
- Preservação de comportamento.

## Procedimento

### Etapa 1 — Compreensão
1. Leia o código selecionado.
2. Identifique responsabilidades e dependências.
3. Consulte métodos semelhantes no projeto.
4. Identifique comportamentos que precisam permanecer iguais.
5. Examine os testes existentes.

### Etapa 2 — Planejamento
Identifique:
- Código duplicado.
- Condicionais desnecessariamente complexas.
- Métodos com múltiplas responsabilidades.
- Nomes pouco expressivos.
- Oportunidades de extração de métodos privados.
- Fluxos difíceis de compreender.

Não trate todos esses pontos como problemas obrigatórios.

### Etapa 3 — Implementação
Quando autorizada:
- Prefira alterações locais.
- Reutilize estruturas já existentes.
- Extraia métodos privados quando isso melhorar a leitura.
- Utilize nomes que expressem intenção.
- Mantenha assinaturas públicas.
- Preserve exceções e seus tipos.
- Preserve respostas e códigos HTTP.
- Preserve validações e regras de negócio.
- Preserve comportamento assíncrono.
- Preserve limites transacionais e ordem de efeitos colaterais.

Evite introduzir mudanças não relacionadas.

### Etapa 4 — Verificação
1. Inspecione o diff.
2. Verifique alterações acidentais.
3. Execute testes relacionados, quando possível.
4. Confirme que contratos e comportamentos permanecem iguais.
5. Informe limitações da validação.

## Delegação
Quando o código for complexo, utilize dois subagentes de leitura:

Agente 1 — Identificar oportunidades reais de simplificação.

Agente 2 — Mapear riscos de alteração comportamental e testes relevantes.

O agente principal é responsável pela implementação e validação.

Para refatorações pequenas, trabalhe sem subagentes.

## Restrições obrigatórias
- NÃO criar serviços novos sem autorização.
- NÃO criar interfaces ou contratos novos sem autorização.
- NÃO introduzir padrões arquiteturais novos.
- NÃO adicionar dependências externas.
- NÃO alterar o comportamento existente.
- NÃO modificar regras de negócio.
- NÃO realizar otimizações prematuras.
- NÃO modificar arquivos fora do escopo sem necessidade demonstrada.
- NÃO refatorar testes apenas para esconder alterações de comportamento.

Se uma melhoria exigir quebrar alguma dessas restrições, explique a necessidade e solicite aprovação antes de implementá-la.

## Formato de entrega
Apresente:
1. Problemas de legibilidade encontrados.
2. Alterações realizadas.
3. Justificativa das decisões.
4. Comportamentos preservados.
5. Testes executados e resultados.

Prefira explicações curtas e técnicas.
