# Especificação — API backend do Valora (.NET 10)

**Playbook:** `component_scaffold`  
**Etapa:** 1/2 — Component Specification & Interface Contract  
**Objetivo:** Implementar uma API .NET 10 para ser o backend  
**Estado:** contrato inicial para implementação; escolhas de persistência/dependências ainda precisam ser fechadas.

## 1. Contexto e limites

Valora é uma aplicação web pessoal para controlar receitas e despesas em BRL. A V1 é de uso individual, sem autenticação, integração bancária ou operação distribuída. A API é a fronteira de negócio e persistência para a SPA: valida os meses, valores e regras de recorrência; calcula o resumo; e protege o histórico contra alterações indevidas.

O escopo desta entrega é o backend e seus contratos. Não implica criar a SPA, autenticação, usuários, OAuth, integrações, hospedagem pública nem infraestrutura de alta disponibilidade. A implantação inicial deve permanecer local/privada e documentar a configuração de bind para não expor a API por acidente. O desenho separa HTTP, aplicação/domínio e persistência para manter baixo acoplamento e permitir evolução sem introduzir serviços distribuídos sem requisito.

## 2. Requisitos cobertos

- Consultar um mês e obter suas receitas, despesas, pendências e saldo.
- Consultar somente lançamentos não recorrentes do mês, sem duplicar dados.
- Criar e editar lançamentos manuais no mês corrente.
- Criar e manter regras de recorrência mensal e gerar ocorrências idempotentemente.
- Consultar meses a partir de novembro de 2026; meses anteriores ao corrente são somente leitura.
- Representar valor monetário com precisão decimal de duas casas, nunca ponto flutuante binário.
- Não incluir valor pendente no saldo; retornar contagem de pendências.
- Retornar erros estáveis, sem stack traces, dados sensíveis ou detalhes internos.
- Usar Entity Framework Core como ORM para persistência PostgreSQL e xUnit como framework de testes .NET.
- Usar o provider Entity Framework Core InMemory nos testes unitários que exercitam um `DbContext`.

## 3. Contrato HTTP proposto

Prefixo versionado: `/api/v1`. JSON em UTF-8. Datas de competência são meses civis `YYYY-MM`, sem horário ou conversão de timezone.

| Método e rota | Uso | Resposta esperada |
|---|---|---|
| `GET /api/v1/months/{month}` | Consulta todas as ocorrências do mês e resumo | `200 MonthView` |
| `GET /api/v1/months/{month}/one-off` | Consulta somente ocorrências sem regra recorrente | `200 OneOffView` |
| `POST /api/v1/months/{month}/open` | Inicializa mês e gera recorrências aplicáveis | `200 MonthView`; repetição não duplica nem sobrescreve |
| `POST /api/v1/months/{month}/entries` | Cria receita ou despesa avulsa no mês corrente | `201 Entry` |
| `PATCH /api/v1/months/{month}/entries/{entryId}` | Atualiza valor/descrição/nota de ocorrência editável | `200 Entry` |
| `POST /api/v1/recurrence-rules` | Cria regra de recorrência com mês efetivo inicial | `201 RecurrenceRule` |
| `PATCH /api/v1/recurrence-rules/{ruleId}` | Cria nova configuração efetiva para mês corrente/futuro | `200 RecurrenceRule` |
| `POST /api/v1/recurrence-rules/{ruleId}/pause` | Pausa regra a partir de um mês efetivo | `200 RecurrenceRule` |
| `POST /api/v1/recurrence-rules/{ruleId}/resume` | Retoma regra a partir de um mês efetivo | `200 RecurrenceRule` |
| `POST /api/v1/recurrence-rules/{ruleId}/end` | Encerra regra a partir de um mês efetivo | `200 RecurrenceRule` |

As rotas são um contrato conceitual inicial. Antes de implementar a T1, resolver as decisões da seção 8, sobretudo se a criação recorrente cria também a ocorrência do mês selecionado ou se ela exige uma chamada separada. A operação de abrir mês deve ser transacional e segura para repetição e chamadas simultâneas. Uma restrição única por regra/mês é a última linha de defesa contra duplicatas.

### 3.1 Modelos de resposta

`MonthView`:

```json
{
  "month": "2026-11",
  "isReadOnly": false,
  "entries": [
    {
      "id": "uuid",
      "kind": "income",
      "description": "Salário",
      "amount": "5000.00",
      "status": "complete",
      "isRecurring": true,
      "note": null
    }
  ],
  "summary": {
    "incomeTotal": "5000.00",
    "expenseTotal": "120.00",
    "balance": "4880.00",
    "pendingAmountCount": 1
  }
}
```

Valores monetários são strings decimais canônicas (`"1234.56"`) nos contratos JSON para evitar perda de precisão em clientes JavaScript. O servidor valida até duas casas e armazena em decimal exato. Confirmação do formato de transporte e precisão numérica do banco é decisão pendente antes da migração.

`Entry` inclui `id`, `kind` (`income`/`expense`), `description`, `amount` opcional/null apenas para ocorrência recorrente pendente, `status` (`complete`/`pending_amount`), `month`, `recurrenceRuleId` opcional, `note` opcional e `isRecurring`. `OneOffView` aplica `recurrenceRuleId == null` sobre as mesmas ocorrências e não mantém cópia.

`RecurrenceRule` inclui identificador estável, natureza, descrição, mês inicial, mês final opcional, estado (`active`/`paused`/`ended`) e configuração efetiva de valor: `copy_default` com valor decimal ou `leave_pending` sem valor padrão. A estrutura deve preservar versões efetivas de configuração para que abertura fora de ordem não aplique retroativamente o estado corrente.

### 3.2 Requests essenciais

- Criar lançamento: `{ "kind": "expense", "description": "Conta de luz", "amount": "120.00", "note": null }`.
- Criar/atualizar recorrência: `{ "kind": "expense", "description": "Aluguel", "effectiveFromMonth": "2026-11", "valueMode": "copy_default", "defaultAmount": "1800.00" }`.
- Para `leave_pending`, `defaultAmount` deve ser omitido/null. Valores negativos, arredondamento automático e mais de duas casas são rejeitados até decisão explícita de produto.

### 3.3 Erros

Usar `application/problem+json` com Problem Details compatível com ASP.NET Core e incluir um código de erro estável em `extensions.code`. Não incluir mensagem de exceção ou valores de transações em logs de erro.

| HTTP | Código sugerido | Significado |
|---|---|---|
| `400` | `invalid_request` | Formato de mês, valor ou campo inválido |
| `404` | `entry_not_found` / `rule_not_found` | Identificador inexistente |
| `409` | `month_read_only` | Tentativa de mutar mês fechado ou anterior ao mês corrente |
| `409` | `recurrence_conflict` | Conflito de transição/configuração ou idempotência |
| `500` | `internal_error` | Falha não prevista; sem detalhes internos |

O cliente deve conseguir distinguir validação, bloqueio de mês e conflito sem interpretar texto livre.

## 4. Invariantes do domínio

1. Uma ocorrência tem natureza `income` ou `expense`, descrição não vazia e mês civil válido.
2. `pending_amount` implica `amount == null`; ocorrência completa exige valor válido. Somente pendentes podem não ter valor.
3. O saldo é a soma de valores preenchidos de receitas menos a soma de valores preenchidos de despesas no mês consultado.
4. Avulsos aparecem na visão completa e na visão avulsa, mas são uma única ocorrência e entram uma vez no saldo.
5. Uma ocorrência recorrente mantém referência à regra e à versão efetiva que a originou.
6. Deve haver no máximo uma ocorrência gerada por regra e mês (`UNIQUE (recurrence_rule_id, month)`).
7. Edição direta de ocorrência e mudança de regra não alteram meses anteriores ao mês corrente. A restrição é aplicada no backend, não apenas na SPA.
8. Repetir abertura do mesmo mês mantém os registros existentes; nunca sobrescreve valores editados pelo usuário.
9. Datas de competência não usam horário UTC. A fonte/fuso que determina o mês corrente precisa ser definida antes da implementação.
10. Resumo é derivado das ocorrências, sem saldo armazenado como segunda fonte de verdade.

## 5. Requisitos técnicos e fronteiras

- Runtime e target framework: .NET 10 / `net10.0`; ASP.NET Core Web API.
- Separar endpoints/contratos HTTP, casos de uso/regras de domínio e adaptador de persistência. Evitar lógica financeira em handlers HTTP e dependência do domínio em framework/banco.
- Documentar OpenAPI para os endpoints e resultados de erro; manter versionamento explícito em `/api/v1`.
- Usar middleware de tratamento de exceções e respostas Problem Details. A documentação oficial ASP.NET Core 10 descreve `AddProblemDetails`, `UseExceptionHandler` e `UseStatusCodePages` para respostas consistentes: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0
- OpenAPI é gerável para Minimal APIs ASP.NET Core; anotar respostas de erro e validação nos metadados do endpoint: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi?view=aspnetcore-10.0
- Persistência alvo: PostgreSQL acessado por Entity Framework Core, usando um provider PostgreSQL compatível com EF Core 10. Usar migrations do EF Core para versionar o schema. O provider concreto e as versões exatas dos pacotes ainda devem ser documentados antes da implementação.
- Testes backend: xUnit. Testes unitários que precisem de `DbContext` usam `Microsoft.EntityFrameworkCore.InMemory`; privilegiar testes de domínio puros quando não for necessário persistir.
- Limite de teste: EF Core InMemory não emula o comportamento relacional do PostgreSQL. Migrations, tipos/precisão `numeric`, constraints, transações e concorrência devem ser verificados em testes de integração com PostgreSQL.
- Não habilitar CORS amplo. Sem login na V1, não criar identificadores de usuário falsos. Preparar organização para plugar autorização futura, sem afirmar que autenticador estará ativo.
- Logs podem conter trace/correlation ID, rota e resultado operacional; evitar descrição, nota e valores financeiros.
- Mudanças de schema preferencialmente aditivas e versionadas. A documentação operacional de backup/restore fica no plano do produto; não implementar backup dentro da API.

## 6. Resiliência e recuperação

- Criar regra/ocorrências e gerar ocorrências mensais dentro de transação para evitar estado parcial.
- Usar unicidade no banco junto da idempotência da operação para absorver repetição ou concorrência; tratar conflito conhecido sem falha opaca.
- Resumos podem ser recalculados a partir dos dados persistidos.
- Preservar ocorrências já geradas em futuras alterações/pausas até que a política pendente seja decidida; não apagar dados automaticamente.
- Usar migrações reversíveis quando seguras; para mudança destrutiva, exigir backup operacional antes da aplicação. Sem RPO/RTO definidos, não prometer recuperação quantitativa.

## 7. Critérios de aceite da especificação

- Contrato lista rotas de leitura, escrita, recorrência e abertura de mês, com requests/responses e códigos de erro.
- O desenho representa dinheiro exato, ocorrência pendente, resumo mensal derivado e filtro avulso sem cópia.
- O backend impõe imutabilidade de meses passados e unicidade de recorrência/mês.
- Inicialização de mês é atômica, idempotente e não sobrescreve ajustes do usuário.
- Escopo V1 não presume autenticação, multiusuário ou distribuição.
- Questões não decididas estão marcadas e não são transformadas em fatos inventados.

## 8. Decisões ainda abertas

Os documentos existentes registram explicitamente que estas decisões precisam ser confirmadas antes da execução completa:

1. Se pausar/encerrar uma regra remove, mantém ou cancela ocorrências futuras que já foram materializadas. A proposta conservadora deste contrato é mantê-las; comportamento automático de remoção está proibido até decisão.
2. Qual fuso define o mês corrente. O produto é brasileiro, mas não está declarado se a regra é `America/Sao_Paulo` ou o timezone local do host.
3. Se valores negativos são proibidos e qual máximo comercial suportado.
4. Precisão total de `numeric(p,2)` compatível com `System.Decimal` e valor máximo aceito. Não escolher `p` sem verificar o mapeamento real do provider/ORM.
5. Se criação de regra recorrente também cria ocorrência no mês inicial e qual deve ser o fluxo de criação da ocorrência inicial.
6. Selecionar e documentar o provider PostgreSQL compatível com EF Core 10 e versões exatas dos pacotes, além da estratégia de migrations. A escolha do ORM foi definida: Entity Framework Core. Context7 não estava acessível nesta sessão; validar versões/APIs concretas na documentação oficial compatível antes de implementar dependências.

## 9. Evidência documental

Esta especificação deriva do PRD V1 e dos documentos de feasibility/implementation plan em `docs/pages/`. A documentação ASP.NET Core 10 foi consultada como fallback porque o servidor Context7 obrigatório pelo padrão do playbook não aparece entre as ferramentas disponíveis nesta sessão. As referências oficiais sustentam Problem Details e OpenAPI; elas não resolvem decisões do produto nem a seleção de persistência.
