---
feature_name: "Valora — gestão pessoal de receitas e despesas"
problem_statement: >-
  O usuário controla receitas e despesas em notas e planilhas e calcula os
  totais manualmente. O método atual não mantém um histórico mensal organizado
  nem facilita a consulta dos meses anteriores, reduzindo a clareza sobre o
  saldo e sobre o que precisa ser gerenciado.
user_stories:
  - "Como usuário pessoal, quero registrar receitas e despesas com descrição, valor e recorrência para manter os lançamentos mensais organizados."
  - "Como usuário pessoal, quero ver todos os lançamentos e o saldo do mês para entender quanto sobra sem calcular manualmente."
  - "Como usuário pessoal, quero consultar uma lista separada de itens não recorrentes e adicionar detalhes opcionais para lembrar do que se tratava cada item."
  - "Como usuário pessoal, quero configurar cada item recorrente para aparecer no mês seguinte com valor padrão ou sem valor para preencher manualmente."
  - "Como usuário pessoal, quero navegar para meses anteriores para consultar meu histórico financeiro."
acceptance_criteria:
  - "Dado um mês com receitas e despesas recorrentes e não recorrentes, quando o usuário abre a visão mensal, então vê todos os lançamentos daquele mês e o saldo igual à soma das receitas menos a soma das despesas."
  - "Dado um mês com itens não recorrentes, quando o usuário abre a visão separada, então vê somente esses itens e pode informar uma descrição complementar opcional por item."
  - "Dado um lançamento não recorrente salvo, quando o usuário consulta ambas as visões, então o item aparece nas duas e é contado uma única vez no saldo."
  - "Dado um lançamento recorrente configurado com valor padrão, quando o mês seguinte é criado ou ativado, então uma ocorrência é criada com esse valor."
  - "Dado um lançamento recorrente configurado sem valor, quando o mês seguinte é criado ou ativado, então uma ocorrência sem valor é criada para preenchimento manual e não distorce o saldo."
  - "Dado que existem lançamentos em meses diferentes, quando o usuário seleciona um mês, então listas e saldo incluem somente os lançamentos desse mês."
  - "Dado que o acompanhamento começa em novembro de 2026, quando o usuário acessa a aplicação, então a página inicial mostra o mês corrente e permite consultar meses anteriores sem misturar períodos."
moscow_prioritization:
  must_have:
    - "Aplicação web para uso via browser, individual, com valores em BRL."
    - "Cadastro manual de receitas e despesas com descrição e valor."
    - "Marcação recorrente/não recorrente e configuração individual de valor padrão ou em branco para ocorrências futuras."
    - "Geração mensal de recorrências; visualização mensal completa; visualização separada de itens não recorrentes; saldo mensal; navegação por meses; formulários separados de receita e despesa."
  should_have: []
  could_have: []
  wont_have:
    - "Integrações bancárias ou com outros serviços, importação de extratos e aplicativo móvel nativo."
    - "Comparativos analíticos entre meses, gráficos, metas, orçamento, projeções ou suporte multiusuário."
edge_cases:
  - "Ocorrência recorrente criada sem valor: deve ficar pendente de preenchimento e não entrar no cálculo como zero ou valor fictício."
  - "Geração do mesmo mês mais de uma vez: deve evitar ocorrências recorrentes duplicadas."
  - "Valor de despesa recorrente ou receita recorrente muda de um mês para outro: permitir ajuste na ocorrência mensal."
  - "Item não recorrente: deve aparecer na visão mensal completa e na visão separada, mas ser somado apenas uma vez."
  - "Mês sem lançamentos: exibir listas vazias e saldo zero, sem carregar lançamentos de outro mês."
  - "Mês anterior a novembro de 2026: a política de navegação/estado vazio deve ser definida na etapa técnica."
  - "Valores monetários e arredondamento: validar e armazenar com precisão adequada a BRL, a definir na etapa técnica."
---

# Valora — PRD da V1

**Status:** Aguardando aprovação do gate `prd`  
**Playbook:** `product_discovery`  
**Feature:** Valora — gestão pessoal de receitas e despesas  
**Slug:** `valora-a-personal-finance-management-too`  
**Data:** 8 de novembro de 2026  
**Documento de origem:** [Registro de elicitação](valora-a-personal-finance-management-too-elicitation-log.md)

## 1. Resumo

Valora é uma aplicação web pessoal para registrar manualmente receitas e despesas em reais, calcular o saldo de cada mês e consultar o histórico mensal. O produto substitui o controle disperso em notas e planilhas com cálculos manuais. A V1 terá uma visão mensal com todos os lançamentos, uma visão separada dos itens não recorrentes e formulários para cadastrar receitas e despesas. Itens recorrentes serão criados no mês seguinte com um valor padrão opcional ou sem valor, para preenchimento manual.

## 2. Problema e oportunidade

O usuário controla as finanças do mês usando bloco de notas e planilhas. Registra receitas e despesas e calcula os totais manualmente. O método atual não oferece um histórico mensal organizado nem uma forma fácil de consultar lançamentos anteriores. Isso reduz a clareza sobre a situação financeira e sobre o que precisa ser gerenciado.

### Usuário primário

Uma pessoa que atualmente controla as próprias finanças por notas e planilhas. O uso inicial é individual (um usuário).

### Resultado esperado

O usuário registra receitas e despesas, visualiza os respectivos valores, consulta meses anteriores e entende o saldo do mês sem fazer as contas à mão.

## 3. Objetivos e métricas

### Objetivo da V1

Permitir o registro e a consulta de receitas e despesas do mês, incluindo recorrências, com cálculo automático do saldo mensal.

### KPI principal

O usuário consegue registrar receitas e despesas e visualizar seus valores e o saldo do mês.

### Medidas de aceite do produto

- É possível criar e consultar receitas e despesas no mês selecionado.
- A visão mensal mostra itens recorrentes e não recorrentes.
- A visão de não recorrentes inclui somente itens avulsos do mês selecionado.
- O saldo exibido corresponde à soma das receitas menos a soma das despesas daquele mês.
- O usuário consegue navegar para meses anteriores e consultar os registros de cada mês sem misturá-los.
- Ocorrências recorrentes para o mês seguinte respeitam a configuração individual de valor padrão ou valor em branco.

## 4. Escopo

### Must Have — V1

1. Aplicação web acessível por browser.
2. Criação manual de uma receita ou despesa com descrição e valor.
3. Definição de cada lançamento como recorrente ou não recorrente.
4. Para cada lançamento recorrente, configuração de como a próxima ocorrência é criada:
   - com um valor padrão parametrizado; ou
   - sem valor, para preenchimento manual.
5. Criação de ocorrência recorrente no mês seguinte, tanto para receitas quanto para despesas.
6. Visão principal do mês com todas as receitas e despesas (recorrentes e não recorrentes), seus valores e o saldo mensal.
7. Segunda visão do mês contendo somente receitas e despesas não recorrentes, com campo de descrição/observação adicional opcional por item.
8. Formulário/tela de criação de receita e formulário/tela de criação de despesa.
9. Mês corrente exibido na página inicial; consulta de meses anteriores disponível a qualquer momento.
10. Período inicial do produto definido como novembro de 2026.
11. Exibição e armazenamento de valores em reais brasileiros (BRL).
12. Lançamentos inseridos manualmente, sem integração externa.

### Won't Have — fora da V1

- Integração bancária, importação de extratos ou conexão com outros serviços.
- Aplicativo móvel nativo.
- Comparativos, gráficos ou relatórios analíticos entre meses.
- Categorização de despesas/receitas além da distinção recorrente/não recorrente.
- Projeções, metas, orçamento ou recomendações financeiras.
- Gestão de investimentos; dividendos podem ser registrados como receitas manuais.
- Suporte multiusuário ou compartilhamento de contas.

## 5. Requisitos funcionais

### RF-01 — Criar receita

O sistema deve permitir criar uma receita informando descrição, valor, mês de referência e se é recorrente. Se recorrente, deve permitir configurar valor padrão para a ocorrência seguinte ou optar por criá-la sem valor.

### RF-02 — Criar despesa

O sistema deve permitir criar uma despesa informando descrição, valor, mês de referência e se é recorrente. Se recorrente, deve permitir configurar valor padrão para a ocorrência seguinte ou optar por criá-la sem valor.

### RF-03 — Visão mensal completa

Para o mês selecionado, o sistema deve listar todas as receitas e despesas, sejam recorrentes ou não, mostrando os valores e o saldo mensal.

### RF-04 — Saldo mensal

O sistema deve calcular o saldo do mês selecionado pela fórmula:

`saldo = soma das receitas do mês − soma das despesas do mês`

Somente lançamentos pertencentes ao mês selecionado entram nesse cálculo. Um lançamento recorrente sem valor preenchido não deve ser tratado como um valor monetário no cálculo. A interface deve indicar que o valor precisa ser informado.

### RF-05 — Visão de itens não recorrentes

O sistema deve permitir abrir uma visão separada que liste apenas as receitas e despesas não recorrentes do mês selecionado. Cada item pode ter uma descrição complementar opcional. Esses itens também aparecem na visão mensal completa e participam do saldo.

### RF-06 — Geração recorrente

Quando um mês seguinte for criado/ativado, o sistema deve gerar uma ocorrência para cada lançamento recorrente do mês anterior, preservando sua descrição e configuração de recorrência. O valor da nova ocorrência deve obedecer à configuração do item: copiar o valor padrão parametrizado ou permanecer em branco para preenchimento manual.

### RF-07 — Navegação mensal

O sistema deve abrir na página inicial mostrando o mês corrente e permitir selecionar meses anteriores. Os registros e totais devem ser sempre limitados ao mês selecionado. O acompanhamento do produto começa em novembro de 2026.

## 6. Regras de negócio

1. Um lançamento é uma receita ou uma despesa.
2. Cada lançamento tem descrição, valor (que pode estar ausente apenas em uma ocorrência recorrente criada sem valor para preenchimento manual), mês de referência e estado recorrente/não recorrente.
3. Receita ou despesa recorrente gera uma ocorrência no mês seguinte.
4. A configuração de valor padrão é individual para cada lançamento recorrente. A ocorrência seguinte pode nascer com esse valor ou sem valor.
5. Recorrência não implica valor imutável. O usuário pode precisar alterar valores, especialmente em despesas recorrentes; algumas receitas também podem variar.
6. Lançamentos não recorrentes pertencem ao mês em que foram registrados e podem incluir uma descrição complementar opcional.
7. O mesmo item não recorrente aparece tanto na visão mensal completa quanto na visão separada de não recorrentes.
8. Todas as receitas e despesas com valor válido do mês selecionado participam do saldo mensal.
9. Moeda da V1: BRL.
10. Período de início: novembro de 2026.

## 7. Jornadas principais

### 7.1 Registrar uma receita ou despesa

1. O usuário abre o formulário de receita ou o formulário de despesa.
2. Informa descrição e valor para o mês selecionado.
3. Marca o lançamento como recorrente ou não recorrente.
4. Se recorrente, escolhe a geração do próximo mês com valor padrão ou sem valor.
5. Salva o lançamento.
6. O item aparece nas visões aplicáveis e o saldo mensal é atualizado quando houver valor.

### 7.2 Consultar o mês

1. O usuário abre a página inicial.
2. O sistema apresenta o mês corrente (com novembro de 2026 como mês inicial do produto).
3. O usuário consulta a lista completa e o saldo.
4. O usuário pode alternar para a visão de itens não recorrentes.
5. O usuário pode navegar para um mês anterior e consultar os dados daquele período.

### 7.3 Criar/ativar o mês seguinte

1. Ao criar/ativar o mês seguinte, o sistema localiza os itens recorrentes do mês anterior.
2. Cria uma ocorrência de cada item para o novo mês.
3. Se houver valor padrão, preenche a ocorrência com esse valor.
4. Se a configuração estiver sem valor, cria a ocorrência para preenchimento manual.
5. O saldo do novo mês considera apenas valores efetivamente informados.

## 8. Critérios de aceite (Given–When–Then)

### CA-01 — visão mensal completa

**Dado** um mês com receitas e despesas recorrentes e não recorrentes, **quando** o usuário abre a visão mensal, **então** vê todos os lançamentos daquele mês, com seus valores e o saldo calculado como receitas menos despesas.

### CA-02 — visão não recorrente

**Dado** um mês com itens recorrentes e não recorrentes, **quando** o usuário abre a visão separada, **então** vê somente receitas e despesas não recorrentes daquele mês e pode incluir uma descrição complementar opcional para cada item.

### CA-03 — lançamento avulso aparece em ambas as visões

**Dado** um item não recorrente salvo no mês selecionado, **quando** o usuário consulta a visão mensal e a visão separada, **então** o item aparece nas duas e é contabilizado uma única vez no saldo.

### CA-04 — recorrência com valor padrão

**Dado** um lançamento recorrente com valor padrão configurado, **quando** o mês seguinte é criado/ativado, **então** o sistema cria uma ocorrência para o mês seguinte com esse valor.

### CA-05 — recorrência sem valor

**Dado** um lançamento recorrente configurado para gerar sem valor, **quando** o mês seguinte é criado/ativado, **então** o sistema cria uma ocorrência sem valor monetário e permite seu preenchimento manual, sem incluí-la como zero ou valor fictício no saldo.

### CA-06 — isolamento mensal

**Dado** que existem lançamentos em meses diferentes, **quando** o usuário seleciona um mês, **então** as listas e o saldo contêm somente os lançamentos do mês selecionado.

### CA-07 — mês inicial e histórico

**Dado** que o período inicial configurado é novembro de 2026, **quando** o usuário acessa a aplicação ou navega para um mês anterior, **então** a página inicial exibe o mês corrente e a consulta mensal não mistura dados de períodos distintos.

### CA-08 — moeda

**Dado** qualquer valor exibido na V1, **quando** o usuário consulta um lançamento ou saldo, **então** o valor é apresentado em reais brasileiros (BRL).

## 9. Dados conceituais

### Lançamento financeiro

- Identificador.
- Natureza: receita ou despesa.
- Descrição principal.
- Valor monetário (opcional para uma ocorrência recorrente ainda não preenchida).
- Mês de referência.
- Recorrente: sim/não.
- Modo de valor para recorrência: valor padrão parametrizado ou em branco.
- Descrição complementar opcional, aplicável a itens não recorrentes conforme o escopo acordado.
- Referência à regra/origem recorrente, quando gerado automaticamente.

Este é um modelo conceitual para orientar a etapa técnica, não uma decisão de esquema de banco de dados.

## 10. Dependências, riscos e decisões pendentes

### Dependências

- Nenhuma API, serviço externo ou dado de terceiros.
- Aplicação, persistência e interface serão criadas do zero.

### Riscos e questões para arquitetura

1. **Geração recorrente idempotente:** definir como evitar duplicar ocorrências se o mês for criado/ativado mais de uma vez.
2. **Mês sem valor:** especificar estado visual e operacional para ocorrências pendentes de valor; elas não podem distorcer o saldo.
3. **Persistência e segurança:** escolher armazenamento e controles proporcionais a dados financeiros pessoais, incluindo decisão sobre autenticação, mesmo para uso inicial individual.
4. **Histórico antes de novembro de 2026:** o usuário pediu consulta de meses anteriores e definiu novembro de 2026 como período inicial. Confirmar se a navegação pode mostrar meses anteriores vazios ou se a navegação começa em novembro de 2026.
5. **Precisão monetária:** definir representação decimal e regras de validação/arredondamento na especificação técnica.
6. **Editabilidade de ocorrências:** confirmar se alterar uma ocorrência recorrente modifica apenas aquele mês ou também o valor padrão futuro. A V1 deve permitir valores diferentes em cada mês, mas esse detalhe operacional ainda não foi especificado.

### Priorização

Alcance inicial: 1 pessoa; impacto percebido: muito alto; custo de atraso declarado: zero; prazo: não definido. Esforço e valor financeiro não foram estimados, portanto não há pontuação RICE nem ROI financeiro válidos nesta etapa. A motivação de aprendizagem e utilidade pessoal deve ser considerada na priorização técnica.

## 11. Fora de escopo e premissas

- Este PRD não escolhe framework, banco de dados, hospedagem, modelo de autenticação nem arquitetura.
- Todos os lançamentos serão manuais na V1.
- Não há comparação ou visualização analítica entre meses na V1; o histórico serve para consultar os meses anteriores.
- Não há comparação entre categorias; o saldo mensal é o resumo agregado solicitado.
- O valor de uma ocorrência recorrente pode variar; a existência de um padrão de valor não deve impedir ajuste mensal.

## 12. Definition of Done do PRD

- O usuário, problema, proposta de valor, escopo da V1, exclusões, plataforma, moeda e origem dos dados estão documentados.
- As regras de recorrência e saldo estão descritas.
- As telas/visões e jornadas essenciais estão descritas.
- Critérios de aceite verificáveis estão definidos.
- Riscos e decisões técnicas pendentes estão encaminhados à etapa de arquitetura.
- Aprovação humana do gate `prd` registrada antes de avançar o playbook.
