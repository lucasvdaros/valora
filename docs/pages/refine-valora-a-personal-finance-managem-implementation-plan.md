```json
{
  "title": "Valora: plano técnico de implementação",
  "status": "ACCEPTED",
  "context": "O PRD define uma aplicação pessoal de finanças. O usuário confirmou execução local, stack .NET, Angular e PostgreSQL, recorrência mensal e imutabilidade dos meses passados.",
  "decision": "Construir a V1 como SPA Angular com API C#/.NET e PostgreSQL local, sem autenticação, gerando ocorrências idempotentemente ao abrir meses e preservando versões efetivas de regras para suportar navegação fora de ordem e histórico imutável.",
  "consequences": {
    "positive": [
      "Stack e fronteiras de componentes estão definidas para iniciar a implementação.",
      "Versionamento e geração idempotente preservam o histórico em aberturas fora de ordem.",
      "Execução local reduz a operação inicial e deixa autenticação para uma evolução futura."
    ],
    "negative": [
      "Manter versões efetivas de recorrência aumenta a complexidade do modelo e das migrações.",
      "Sem autenticação, a aplicação depende de execução local/privada corretamente configurada."
    ],
    "neutral": [
      "Pausar ou encerrar recorrências ainda exige definir o tratamento de ocorrências futuras já criadas."
    ]
  },
  "rollback_strategy": "Como esta etapa é um plano, rever decisões antes de implementar não exige migração. Após a implementação, usar migrações PostgreSQL versionadas e backup local testado; reverter código e esquema em conjunto ou restaurar o último backup consistente."
}
```
---
title: "Valora: plano técnico de implementação"
status: ACCEPTED
context: >-
  O PRD define uma aplicação pessoal de finanças, e o usuário confirmou execução local,
  stack .NET, Angular e PostgreSQL, recorrência mensal e imutabilidade de meses passados.
decision: >-
  Construir a V1 como SPA Angular com API C#/.NET e PostgreSQL local, sem autenticação,
  gerando ocorrências idempotentemente ao abrir meses e preservando versões efetivas
  das regras para suportar navegação fora de ordem e histórico imutável.
consequences:
  positive:
    - "Stack e fronteiras de componentes estão definidas para iniciar a implementação."
    - "Versionamento e geração idempotente preservam o histórico em aberturas fora de ordem."
    - "Execução local reduz a operação inicial e deixa autenticação para uma evolução futura."
  negative:
    - "Manter versões efetivas de recorrência aumenta a complexidade do modelo e das migrações."
    - "Sem autenticação, a aplicação depende de execução local/privada corretamente configurada."
  neutral:
    - "Pausar ou encerrar recorrências ainda exige definir o tratamento de ocorrências futuras já criadas."
rollback_strategy: >-
  Como esta etapa é um plano, rever decisões antes de implementar não exige migração.
  Após a implementação, usar migrações PostgreSQL versionadas e backup local testado;
  reverter código e esquema em conjunto ou restaurar o último backup consistente.
---
# Valora — plano técnico de implementação

**Playbook:** `technical_refinement`  
**Etapa:** `implementation_plan`  
**PRD de origem:** `docs/pages/valora-a-personal-finance-management-too-prd.md`  
**Viabilidade:** `docs/pages/refine-valora-a-personal-finance-managem-feasibility.md`  
**Data:** 8 de novembro de 2026  
**Status:** Revisado com as decisões fornecidas pelo usuário; aguardando revisão e aprovação do gate `plan`.

## 1. Objetivo e escopo da V1

Construir uma aplicação web pessoal para registrar receitas e despesas em BRL, consultar lançamentos por mês, visualizar saldo e pendências, separar a consulta de itens não recorrentes e gerar ocorrências recorrentes ao abrir cada mês.

A solução será executada localmente/privadamente e não será publicada na internet nesta fase. A stack está definida: backend em C#/.NET, frontend SPA em Angular e banco PostgreSQL. Não haverá autenticação na V1. O desenho deve permitir acrescentar autenticação no futuro, sem implementá-la agora.

## 2. Decisões de produto registradas

As seguintes respostas foram fornecidas pelo usuário e passam a ser requisitos do plano:

- Navegação mensal pode ir a períodos anteriores a novembro de 2026.
- A geração das ocorrências recorrentes acontece ao abrir um mês.
- Meses podem ser abertos fora de ordem.
- Recorrências são sempre mensais e podem ter início, fim, pausa e encerramento.
- Alterar o valor padrão da recorrência atualiza o mês corrente e os meses seguintes.
- Meses anteriores ao mês corrente são imutáveis: valores já usados nesses meses não podem ser alterados.
- A execução será local/privada, sem autenticação na V1; autenticação poderá ser adicionada no futuro.
- Não foi definido limite de quantidade de lançamentos. Valores monetários têm duas casas decimais.

### Semântica operacional proposta para manter essas regras consistentes

- O mês corrente é determinado pela data local da máquina que executa a aplicação. Mês corrente e meses futuros permanecem editáveis; ao virar o mês, o anterior torna-se imutável. O fuso/localidade operacional deve ser configurável ou documentado para evitar divergência na virada do mês.
- Abrir qualquer mês chama um gerador idempotente para aquele mês. Uma recorrência só gera ocorrência se o mês estiver dentro do início/fim e ativo segundo a sua configuração aplicável àquele período. A operação não pode duplicar ocorrências e deve respeitar os valores efetivos da regra para o mês solicitado, mesmo quando meses forem abertos fora de ordem.
- Para reconciliar geração fora de ordem com imutabilidade, as mudanças de configuração da recorrência precisam ser versionadas por mês efetivo. Ao abrir um mês passado ainda não materializado, o sistema usa a versão efetiva naquele mês; cria a ocorrência uma vez e a trata como fechada/imutável. Essa regra preserva o histórico sem usar por engano o valor atual para períodos antigos.
- Alterar o valor padrão no mês corrente atualiza as ocorrências do mês corrente e de meses futuros já materializados, e define o valor para os meses futuros que ainda serão abertos. Ocorrências de meses anteriores nunca são atualizadas.
- Pausar ou encerrar uma recorrência vale a partir do mês corrente ou de um mês futuro escolhido; não apaga nem modifica ocorrências de períodos anteriores. Definir o que acontece com uma ocorrência já gerada para um mês futuro ao pausar/encerrar: a proposta é removê-la/cancelá-la ou mantê-la conforme escolha explícita do usuário, nunca silenciosamente.

A política de ocorrências futuras já criadas ao pausar/encerrar e o mecanismo exato para selecionar a data local do fechamento devem ser validados como comportamento de produto antes da implementação da UI. O plano usa as propostas acima como contrato inicial.

## 3. Arquitetura e stack

- **Backend:** C#/.NET 10, aplicação web API em ASP.NET Core. Usar Entity Framework Core como ORM e o provider PostgreSQL do EF Core para acesso ao banco. Manter regras financeiras e de recorrência em serviços de domínio/aplicação, independentes dos controllers e do acesso a dados.
- **Frontend:** Angular SPA, consumindo a API por contratos HTTP versionáveis. A UI não calcula regras de recorrência; saldos e validações de domínio vêm do backend.
- **Banco:** PostgreSQL como fonte de verdade transacional, acessado por Entity Framework Core. Usar migrações versionadas do EF Core e constraints no banco para invariantes críticas.
- **Estrutura:** um deploy local, composto por SPA, API e PostgreSQL, com módulos internos claros. Evitar microsserviços, filas e cache distribuído para o uso individual atual.
- **Execução local:** documentar configuração e inicialização, armazenamento persistente do banco e procedimento de backup. Manter configurações e caminhos específicos fora das regras de domínio.
- **Autenticação futura:** deixar pontos de extensão na API para autorização/autenticação via middleware e políticas, sem criar login, usuários ou fluxo OAuth/SAML agora. Não armazenar dados de autenticação nesta V1.
- **Testes backend:** xUnit como framework de testes. Os testes unitários que exercitam persistência/contexto de dados devem usar o provider Entity Framework Core InMemory. Manter testes de integração separados contra PostgreSQL real para validar provider, migrations, constraints, transações e concorrência; o provider InMemory não comprova comportamento relacional nem substitui esses testes.
- **Documentação de dependências:** Context7 não estava acessível nesta sessão. Antes de implementar contra APIs concretas do .NET 10, Angular ou provider PostgreSQL do EF Core, consultar documentação oficial compatível com as versões selecionadas. A seleção e as versões dos pacotes devem ser documentadas na T1.

Não há necessidade atual de escalabilidade horizontal ou arquitetura distribuída. Separar contratos e módulos internos reduz acoplamento e deixa a futura autenticação possível sem afirmar que será necessária uma infraestrutura de internet.

## 4. Modelo e contratos de dados propostos

### RecurrenceRule e versões efetivas

Manter uma identidade estável para a recorrência e histórico de configuração por mês efetivo:

- `id`, `kind` (`income`/`expense`), descrição e estado;
- mês inicial, mês final opcional e política mensal;
- versão de configuração com `effective_from_month`, modo de valor (`copy_default`/`leave_pending`) e valor padrão;
- estado ativo/pausado/encerrado com mês efetivo da transição;
- metadados de criação e atualização.

Versões efetivas possibilitam abrir meses fora de ordem sem aplicar retroativamente a configuração corrente. Alterações feitas no mês atual passam a valer nesse mês e nos seguintes, mantendo intactas as versões/valores já fechados de meses passados.

### TransactionOccurrence

- `id`, `kind`, descrição;
- valor em decimal exato com duas casas (`amount`, anulável somente quando pendente);
- status `complete` ou `pending_amount`;
- mês civil de competência, sem horário;
- `recurrence_rule_id` anulável para itens avulsos;
- nota complementar opcional;
- timestamps e, se necessário, origem/manual ou referência à versão da recorrência usada na geração.

### Dinheiro e precisão

Usar `decimal` do .NET e tipo `numeric` do PostgreSQL com escala de duas casas, nunca `float`/`double` para valores financeiros. Validar entrada com até duas casas decimais e persistir sem arredondamento implícito; se o usuário inserir mais de duas casas, rejeitar com mensagem clara. A quantidade de lançamentos não terá limite de produto.

O tipo `numeric` do PostgreSQL e `decimal` do .NET têm limites técnicos de representação; não foi definido um teto de valor comercial. A implementação deve escolher e documentar uma precisão de coluna compatível com a faixa suportada por .NET e rejeitar overflow explicitamente, sem truncamento. Confirmar essa faixa técnica antes da migração inicial.

### Resumo mensal

Consulta derivada do mês solicitado: totais de receitas preenchidas, despesas preenchidas, saldo (receitas menos despesas) e quantidade de ocorrências pendentes. A visão de itens não recorrentes filtra as mesmas ocorrências de `recurrence_rule_id IS NULL`; não replica dados.

### Invariantes

- `pending_amount` se e somente se o valor estiver ausente.
- Valores preenchidos têm escala de duas casas decimais e natureza não negativa; confirmar política de valores negativos se desejado, pois não foi respondido explicitamente.
- Uma ocorrência gerada pertence à natureza da regra que a originou.
- No máximo uma ocorrência por regra e mês (`UNIQUE (recurrence_rule_id, month)`).
- Um mês anterior ao mês local corrente não aceita edição manual nem atualização por mudança de regra.
- O saldo soma somente os valores preenchidos do mês consultado.
- Regerar mês já aberto não altera ocorrências existentes nem duplica linhas.

## 5. Contratos API e fronteiras

A API ASP.NET Core deve oferecer operações conceituais para:

- listar/resumir um mês (`month=YYYY-MM`), incluindo pendências;
- listar somente itens não recorrentes do mês;
- criar receita e criar despesa para um mês ainda editável;
- editar ocorrência em mês ainda editável;
- criar, editar, pausar, retomar e encerrar uma regra recorrente com mês efetivo;
- abrir/inicializar mês, gerando ocorrências idempotentemente.

O backend deve validar que edição de ocorrência e configuração efetiva não alteram mês fechado. Respostas de erro devem distinguir validação, mês fechado, conflito de unicidade e falha interna sem expor stack trace ou dados financeiros. A SPA recebe o estado do mês e reflete o bloqueio de edição, mas a regra de imutabilidade é sempre aplicada também no backend.

## 6. Plano de trabalho ordenado

| ID | Entrega | Trabalho | Critério de conclusão |
|---|---|---|---|
| T0 | Contratos finais | Confirmar comportamento ao pausar/encerrar diante de meses futuros já gerados, fuso local e valores negativos; fechar precisão técnica de `numeric` | Regras determinísticas documentadas e refletidas nos critérios de aceite |
| T1 | Fundação .NET/Angular/PostgreSQL | Criar solução .NET 10 API com Entity Framework Core e provider PostgreSQL, SPA Angular, configuração local PostgreSQL, projeto de testes xUnit com EF Core InMemory, contratos de ambiente e instruções de execução | API e SPA iniciam localmente; banco persiste dados; testes xUnit executam; versões e comandos estão documentados |
| T2 | Persistência e migrações | Modelar regras, versões efetivas e ocorrências com EF Core; criar índices, constraints e migrations | Banco PostgreSQL pode ser criado do zero; unicidade e estados inválidos são bloqueados pelo banco |
| T3 | Domínio financeiro | Implementar valor decimal de duas casas, mês civil, regra de mês fechado, resumo e pendência | Domínio independente de Angular/ASP.NET; casos de domínio cobertos por testes xUnit determinísticos, usando EF Core InMemory quando o teste depender do contexto de dados |
| T4 | API de cadastro e recorrência | Implementar endpoints e transações para lançamentos e regras; autorização futura preparada via middleware sem login nesta fase | API rejeita mutações de meses fechados e cria dados consistentemente |
| T5 | Geração ao abrir mês | Gerar em ordem de mês solicitado, respeitando versão efetiva, intervalo, pausa/fim e chave idempotente | Meses abertos fora de ordem recebem a configuração aplicável; repetição e concorrência não duplicam |
| T6 | Angular SPA | Criar visões mensal completa e avulsos, navegação sem limite inferior em novembro/2026, formulários separados e estado de mês fechado | Fluxos de cadastro/consulta operam via API; períodos anteriores podem ser consultados e são somente leitura |
| T7 | Integração e fluxo completo | Integrar SPA/API/PostgreSQL e exercitar criação, abertura de meses fora de ordem, mudanças de padrão e fechamento | Critérios funcionais e imutabilidade histórica verificados no fluxo ponta a ponta |
| T8 | Backup e release local | Documentar backup/restore PostgreSQL, migrações e inicialização/atualização local | Restauração validada em cópia e procedimento local documentado |

A execução é sequencial por dependências: T0/T1 antes de T2; T2/T3 antes de T4/T5; API estável antes de T6; T7/T8 antes de declarar a V1 pronta.

## 7. Plano de verificação

- **Unitários .NET (xUnit):** soma por natureza; duas casas; entradas acima de duas casas; pendência não soma; mudança de mês corrente; mês fechado rejeita alteração; versão efetiva resolve regras ao abrir meses fora de ordem. Usar EF Core InMemory nos testes unitários que precisam de um `DbContext`; testar regras puras sem banco quando possível.
- **Integração PostgreSQL/API:** migrations do EF Core desde banco vazio; constraints; transação de regra/ocorrência; consulta por mês; erros de mês fechado; idempotência ao abrir mês repetidas vezes e sob chamadas concorrentes. Não usar EF Core InMemory como evidência desses comportamentos específicos do PostgreSQL.
- **Regressão de histórico:** atualizar valor padrão no mês atual altera esse mês e meses futuros previstos, mas não muda ocorrências anteriores; pausar/encerrar mantém histórico.
- **Angular:** navegação mensal, dados separados por mês, estados vazios, estado somente leitura para meses passados, formulário com precisão de duas casas e mensagem para pendências.
- **Ponta a ponta:** cadastrar itens recorrentes e avulsos, abrir meses em ordem diferente, conferir valores e saldo, avançar para mês seguinte e verificar imutabilidade do mês anterior.
- **Recuperação:** backup e restore PostgreSQL local em cópia isolada, conferindo regras, ocorrências e totais após restauração.

Testes de carga e infraestrutura distribuída não são necessários sem requisito de volume concorrente ou serviço remoto. A UI deve incluir navegação por teclado e acessibilidade básica.

## 8. Resiliência, rollback e recuperação

- Criar/alterar regra e ocorrências afetadas dentro de transação para não deixar estado parcialmente atualizado.
- Gerar ocorrências com transação e unicidade por regra/mês; repetir em caso de falha sem duplicar.
- No fechamento natural do mês, bloquear alterações pelo backend. Para respeitar imutabilidade histórica também em atualizações de regra, a operação de mudança altera apenas o mês corrente e seguintes.
- Resumos permanecem derivados e podem ser recalculados; não persistir saldo como segunda fonte de verdade.
- Versionar migrações PostgreSQL. Antes de mudança destrutiva, fazer backup; preferir migrações aditivas. Documentar recuperação se não houver rollback seguro de esquema.
- Manter backup local em local separado dos arquivos ativos do banco e ensaiar restauração. Frequência e retenção ainda dependem da rotina de uso; não há metas de RPO/RTO definidas.
- Logs locais podem registrar identificadores técnicos e operação, sem descrições ou valores financeiros.

## 9. Segurança e evolução futura

Na V1, API e PostgreSQL ficam acessíveis somente no ambiente local/privado conforme a configuração de execução; não criar login, usuários, OAuth ou SAML. Documentar explicitamente como limitar a exposição de rede para que “local” não signifique acidentalmente uma API aberta na rede local.

A arquitetura deve permitir adicionar autenticação depois por middleware/policies ASP.NET Core e proteção das rotas da SPA. Não assumir um provedor específico nem persistir uma identidade falsa na V1. Se a aplicação for publicada futuramente, revisar threat model, autenticação, autorização, HTTPS, gestão de segredos, CORS e recuperação de conta antes da publicação.

## 10. Business Value

O benefício declarado é pessoal e de aprendizagem: consolidar registros, evitar cálculo manual e preservar histórico consultável. O alcance atual é uma pessoa; horas de engenharia, custo de hospedagem e valor monetário do benefício não foram estimados. Não há base válida para calcular ROI ou payback numérico.

Usar uma solução local com um único deploy reduz operação e custo recorrente. Versionamento de regras, geração idempotente e imutabilidade por mês reduzem risco de alterar o histórico ou criar duplicatas. O uso de backup PostgreSQL restaurável reduz o impacto de perda do equipamento. Aplicar um buffer de 20% sobre estimativas futuras quando houver esforço mensurável; não inventar horas nesta fase.

## 11. Riscos e priorização

- **Alto — abertura fora de ordem com recorrência:** exige versão efetiva de configuração e chave única para não aplicar estado atual incorretamente ao passado.
- **Alto — mês fechado alterado por regra:** toda atualização em lote deve excluir meses anteriores ao corrente e ter cobertura de regressão.
- **Médio — limites técnicos de valor:** definir precisão PostgreSQL compatível com `System.Decimal`; não há teto comercial de valor definido.
- **Médio — pausa/encerramento com meses futuros já gerados:** definir se ocorrências futuras são removidas ou mantidas antes da UI final.
- **Médio — escopo local:** configuração de bind/host precisa evitar exposição acidental fora do dispositivo.
- **Baixo — volume de lançamentos:** não há limite de produto; PostgreSQL e índice por mês atendem ao desenho esperado, sem necessidade de cotas artificiais.

## 12. Critério de pronto para iniciar implementação

- Gate `plan` aprovado.
- T0 concluído, especialmente política para ocorrências futuras já materializadas e fuso operacional.
- Versões suportadas e dependências .NET 10, Angular, Entity Framework Core/provider PostgreSQL, xUnit e EF Core InMemory selecionadas e documentadas.
- Critérios de aceite cobrem valores com duas casas, geração mensal ao abrir mês fora de ordem, pausa/fim, mudança de padrão em meses correntes/futuros e imutabilidade dos anteriores.

## 13. Encaminhamento ao PRD

Atualizar o PRD com a stack .NET/Angular/PostgreSQL, execução local/privada sem autenticação na V1 e possibilidade futura de login, navegação antes de novembro de 2026, recorrência mensal com início/fim/pausa/encerramento, geração ao abrir meses em qualquer ordem, valores com duas casas e regra de imutabilidade de meses passados. O plano registra ainda duas decisões operacionais que precisam de confirmação: efeito de pausa/encerramento sobre meses futuros já gerados e a definição de mês corrente no fuso local.


