# Especificação do scaffold — API backend Valora

**Playbook:** `component_scaffold`  
**Etapa:** 1/2 — Component Specification & Interface Contract  
**Status:** contrato de implementação derivado dos documentos aprovados e das decisões confirmadas pelo usuário em 10/11/2026.

## Objetivo e limites

Criar o backend HTTP da V1 do Valora em ASP.NET Core / .NET 10 (`net10.0`) para uso local/privado. A API será responsável pelos contratos, regras financeiras, recorrências e persistência PostgreSQL. Não incluir SPA Angular, autenticação, usuários, integração bancária, deploy público nem infraestrutura distribuída.

## Referências e precedência

- `docs/pages/implementar-uma-api-net-10-para-ser-o-ba-spec.md` — rotas, modelos, erros e invariantes detalhados.
- `docs/pages/refine-valora-a-personal-finance-managem-implementation-plan.md` — decisões de produto, limites e desenho técnico.

Em caso de divergência, manter explícitas as questões abertas listadas abaixo e não criar comportamento de produto por suposição.

## Contrato HTTP a implementar

Prefixo `/api/v1`, JSON UTF-8 e competência civil `YYYY-MM` sem timezone no payload.

| Método e rota | Resultado esperado |
|---|---|
| `GET /months/{month}` | `200 MonthView` com lançamentos e resumo derivado |
| `GET /months/{month}/one-off` | `200 OneOffView`, filtrado da mesma coleção de ocorrências avulsas |
| `POST /months/{month}/open` | `200 MonthView`; gera recorrências aplicáveis de forma transacional e idempotente |
| `POST /months/{month}/entries` | `201 Entry`; cria lançamento avulso no mês editável |
| `PATCH /months/{month}/entries/{entryId}` | `200 Entry`; edita somente campos permitidos no mês editável |
| `POST /recurrence-rules` | `201 RecurrenceRule` |
| `PATCH /recurrence-rules/{ruleId}` | `200 RecurrenceRule`; nova configuração efetiva |
| `POST /recurrence-rules/{ruleId}/pause` | `200 RecurrenceRule` |
| `POST /recurrence-rules/{ruleId}/resume` | `200 RecurrenceRule` |
| `POST /recurrence-rules/{ruleId}/end` | `200 RecurrenceRule` |

As rotas de recorrência usam requests com `effectiveFromMonth`; pausa, retomada e encerramento devem registrar vigência efetiva. Respostas monetárias usam strings decimais canônicas com duas casas; armazenamento em decimal exato, nunca ponto flutuante binário.

## Invariantes que atravessam as camadas

1. Natureza é `income` ou `expense`; descrição não vazia; mês civil válido.
2. `pending_amount` requer `amount == null`; ocorrência completa exige valor válido.
3. Saldo = receitas preenchidas menos despesas preenchidas; pendências não entram no saldo.
4. Lançamentos avulsos são uma única ocorrência e aparecem também na consulta completa.
5. Ocorrência recorrente referencia regra e versão efetiva de origem.
6. Unicidade no banco por `(recurrence_rule_id, month)` protege geração repetida e concorrente.
7. Mês anterior ao corrente é imutável no backend; atualizações de regra não alteram meses passados.
8. Abrir o mês de novo não duplica nem sobrescreve valores editados.
9. Resumo deriva das ocorrências persistidas; não existe saldo persistido como segunda fonte da verdade.
10. Erros HTTP usam Problem Details com `extensions.code` estável e sem stack traces ou dados financeiros sensíveis.

## Decisões de produto confirmadas

- Pausar ou encerrar uma regra não apaga nem cancela ocorrências futuras já materializadas; apenas impede novas gerações a partir do mês efetivo.
- O mês corrente é determinado por uma zona configurável, com padrão `America/Sao_Paulo`.
- Valores negativos são inválidos. Não há limite comercial definido; aplicar o limite representável por `System.Decimal` sem escolher teto menor.
- Criar uma regra cria também sua ocorrência do mês efetivo inicial, na mesma transação. A abertura posterior do mês continua idempotente.

## Fronteiras internas

- **HTTP:** endpoints e DTOs versionados; sem regras financeiras nos handlers.
- **Aplicação/domínio:** validação de mês/valor, cálculo do resumo, mutabilidade, versões efetivas e geração de recorrências.
- **Persistência:** Entity Framework Core, PostgreSQL e migrations versionadas; constraints de unicidade e tipos decimais exatos.
- **Erros:** middleware de exceção e Problem Details; códigos sugeridos `invalid_request`, `entry_not_found`, `rule_not_found`, `month_read_only`, `recurrence_conflict` e `internal_error`.
- **Operação:** bind local/privado documentado; sem CORS amplo; logs sem descrição, nota ou valores financeiros.

## Testes e verificação

Usar xUnit. Testar domínio puro sem banco quando possível; testes unitários que exercitem `DbContext` usam EF Core InMemory. Cobrir também integração PostgreSQL para migrations, precisão numérica, constraints, transações e concorrência, pois InMemory não reproduz essas garantias. Verificar endpoints, Problem Details, cálculos, pendências, imutabilidade, idempotência e navegação fora de ordem.

## Dependências e decisões técnicas

- O projeto fixa `Microsoft.AspNetCore.OpenApi`, EF Core, EF Core Relational, EF Core Design e EF Core InMemory em `10.0.14`; provider `Npgsql.EntityFrameworkCore.PostgreSQL` em `10.0.3`; xUnit `2.9.3`. O SDK disponível no workspace é .NET `10.0.400`.
- A referência interativa usa `Scalar.AspNetCore` `2.20.0`, servida somente em Development em `/scalar`; o OpenAPI fica em `/openapi/v1.json`.
- Registros de dependência são separados em `AddValoraApi`, `AddValoraApplication`, `AddValoraDomain` e `AddValoraInfrastructure`. Mapeamento OpenAPI, middleware e endpoints ficam em extensões da camada Api; `Program.cs` é apenas o composition root.
- Valores são persistidos como `numeric(31,2)`, que permite o limite de `System.Decimal` com duas casas sem impor teto comercial menor. As migrations `InitialCreate` e `UseDecimalMaxPrecision` versionam o schema.
- A verificação em PostgreSQL real (constraint, migrations aplicadas, tipos, concorrência e transações) permanece necessária quando um servidor PostgreSQL estiver disponível; EF Core InMemory não a substitui.
- O plano registra gate `plan` como pré-requisito de prontidão; ele não está incluído na sessão atual de `component_scaffold` (que possui somente o gate `execution`). Verificar o estado/aprovação do plano antes de tratar esta implementação como autorizada por aquele gate.

## Documentação técnica consultada

O Context7 MCP não está disponível entre as ferramentas desta sessão. Como fallback permitido pelos documentos de referência, foram consultadas páginas oficiais de [Problem Details em ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0), [OpenAPI para Minimal APIs em ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0), [provider EF Core PostgreSQL do Npgsql](https://www.npgsql.org/efcore/), [notas da versão 10.0 do provider](https://www.npgsql.org/efcore/release-notes/10.0.html) e [integração Scalar para ASP.NET Core](https://scalar.com/products/api-references/integrations/aspnetcore/integration). As versões de pacotes e migrações foram registradas nesta implementação; a validação relacional final ainda depende de PostgreSQL real.
