# Valora — Registro de elicitação de produto

**Playbook:** `product_discovery`  
**Etapa:** `elicitation_interview`  
**Feature:** Valora — ferramenta pessoal de gestão financeira  
**Slug da sessão:** `valora-a-personal-finance-management-too`  
**Data da elicitação:** 8 de novembro de 2026  
**Status:** Entrevista concluída; pronto para formulação do PRD.

## 1. Problema e valor

### Público-alvo

Uma pessoa que atualmente controla as finanças pessoais em bloco de notas e planilhas.

### Problema observado

As receitas e despesas correntes do mês são registradas manualmente e as contas aritméticas também são feitas à mão. O método atual não mantém um histórico organizado dos meses anteriores nem facilita sua consulta. O usuário quer visualizar os lançamentos e o saldo mensal para ter mais clareza sobre o que precisa gerenciar na vida financeira.

### Alternativa atual

Bloco de notas e planilhas, com cálculos manuais.

### Proposta de valor

Permitir que o usuário registre receitas e despesas, consulte seus valores e veja automaticamente quanto sobra no mês, com histórico mensal e tratamento explícito de itens recorrentes e não recorrentes.

## 2. Escopo funcional e jornada

### V1 — requisitos funcionais

1. Registrar receitas e despesas manualmente. Cada lançamento tem descrição e valor.
2. Marcar cada receita ou despesa como recorrente ou não recorrente.
3. Calcular o saldo mensal considerando todos os lançamentos do mês:

   `saldo do mês = total de receitas do mês − total de despesas do mês`

4. Disponibilizar uma visão mensal com todas as receitas e despesas, recorrentes e não recorrentes, mostrando valores e saldo.
5. Disponibilizar uma segunda visão do mês contendo apenas receitas e despesas não recorrentes. Cada item pode ter uma descrição ou observação adicional opcional.
6. Disponibilizar telas/formulários próprios para criar uma receita e para criar uma despesa.
7. Iniciar o acompanhamento em **novembro de 2026**. A página inicial deve exibir o mês corrente; deve ser possível consultar meses anteriores a qualquer momento.
8. Gerar automaticamente no mês seguinte cada item configurado como recorrente. Cada item recorrente deve permitir escolher entre:
   - gerar a ocorrência seguinte com um valor padrão parametrizado; ou
   - gerar a ocorrência seguinte sem valor preenchido, para inserção manual.
9. Aplicar recorrência tanto a receitas quanto a despesas. Os valores podem variar de um mês para outro; não se presume que recorrente signifique valor imutável.

### Jornada principal

1. O usuário abre a aplicação e vê o mês corrente (com novembro de 2026 como período inicial do produto).
2. Registra uma receita ou despesa por meio do formulário correspondente, informando descrição, valor e se é recorrente.
3. Para itens recorrentes, define se o próximo mês recebe um valor padrão ou um campo de valor em branco para preencher manualmente.
4. Consulta todos os lançamentos e o saldo do mês na visão mensal completa.
5. Consulta separadamente os itens não recorrentes do mesmo mês e, quando necessário, acrescenta uma descrição opcional.
6. Navega para meses anteriores para consultar o histórico disponível.

### Regras de recorrência

- Um item marcado como recorrente gera uma ocorrência no mês seguinte.
- A configuração de valor padrão é por item: o usuário pode parametrizar um valor para a ocorrência seguinte ou deixá-la sem valor, para cadastro manual.
- A renda recorrente pode normalmente manter o valor, como salário, mas pode mudar excepcionalmente.
- Despesas recorrentes, como contas de água, energia ou cartão, podem ter valores diferentes a cada mês.
- Rendimentos como dividendos podem ser lançados com valores próprios em cada mês.
- Não foi solicitado um tipo de lançamento separado para “valor fixo” e “valor variável”; a regra aplicável é recorrente/não recorrente e a opção de valor padrão ou em branco.

### Priorização MoSCoW

| Prioridade | Escopo |
| --- | --- |
| **Must Have** | Cadastro manual de receitas e despesas com descrição e valor; marcação recorrente/não recorrente; configuração do valor padrão ou em branco para recorrências; geração no mês seguinte; visão mensal de todos os itens; visão separada dos não recorrentes com descrição opcional; saldo mensal; navegação por meses; moeda BRL. |
| **Should Have** | Nenhum item adicional foi confirmado para a V1. |
| **Could Have** | Nenhum item adicional foi confirmado para a V1. |
| **Won't Have (V1)** | Importação bancária ou de outros serviços; integrações externas; relatórios comparativos entre meses; comparação analítica entre receitas e despesas além do saldo; categorização além do atributo recorrente/não recorrente; funcionalidades financeiras não listadas acima. |

## 3. Dados, plataforma e integrações

- **Plataforma:** aplicação web acessível por browser; sem aplicativo móvel nesta etapa.
- **Estado do projeto:** será criado e modelado do zero; não há banco de dados, API, esquema ou arquivos de negócio existentes.
- **Entrada de dados:** lançamentos manuais.
- **Integrações externas:** nenhuma prevista para a V1.
- **Moeda:** real brasileiro (BRL).
- **Usuários:** uso pessoal, uma pessoa.
- **Dados principais identificados:** mês de referência, tipo do lançamento (receita/despesa), descrição, valor, recorrência, configuração de valor padrão ou em branco para recorrências e observação opcional para itens não recorrentes.

## 4. Prioridade, esforço e viabilidade

- **Alcance inicial:** 1 usuário por mês (uso pessoal).
- **Impacto percebido pelo usuário:** muito alto; o aplicativo deve ampliar a visão do que precisa gerenciar na vida financeira.
- **Prazo:** não há prazo definido; trata-se de um projeto pessoal também destinado ao aprendizado do tech-agents.
- **Custo do atraso:** nenhum custo financeiro ou operacional mensurável declarado.
- **Esforço:** ainda não estimado; deve ser dimensionado em etapa técnica posterior. Como o esforço não está disponível, não se calcula uma pontuação RICE numérica nesta etapa.
- **Confiança:** a prioridade é baseada no relato direto do usuário, mas não há telemetria ou pesquisa externa, dado o uso individual.
- **ROI financeiro:** não aplicável com os dados atuais. O benefício declarado é pessoal e de aprendizagem, sem valor financeiro quantificado.

## 5. Sucesso e critérios de aceite

### KPI principal

O usuário consegue registrar receitas e despesas e visualizar seus valores e o saldo do mês.

### Critérios de aceite iniciais (Given–When–Then)

1. **Visão mensal completa**  
   **Dado** um mês com receitas e despesas recorrentes e não recorrentes, **quando** o usuário abre a visão mensal, **então** vê todos esses lançamentos e o saldo calculado pela soma das receitas menos a soma das despesas do mesmo mês.

2. **Visão de não recorrentes**  
   **Dado** um mês com lançamentos não recorrentes, **quando** o usuário abre a visão separada, **então** vê apenas as receitas e despesas não recorrentes daquele mês e pode informar uma descrição adicional opcional por item.

3. **Criação manual**  
   **Dado** que o usuário abre o formulário de receita ou despesa, **quando** informa descrição, valor e recorrência e salva, **então** o lançamento fica registrado para o mês correspondente e aparece nas visões aplicáveis.

4. **Recorrência com valor padrão**  
   **Dado** um lançamento recorrente configurado com valor padrão, **quando** ocorre a criação do mês seguinte, **então** uma ocorrência desse lançamento é criada com o valor parametrizado.

5. **Recorrência sem valor padrão**  
   **Dado** um lançamento recorrente configurado sem valor, **quando** ocorre a criação do mês seguinte, **então** uma ocorrência é criada sem valor preenchido para cadastro manual.

6. **Mês e histórico**  
   **Dado** que o período inicial do produto é novembro de 2026, **quando** o usuário abre a página inicial no mês corrente ou navega para um mês anterior, **então** a aplicação mostra os dados do período selecionado, sem misturar lançamentos de meses diferentes.

## 6. Limites e perguntas para refinamento técnico

Os seguintes detalhes não foram definidos na entrevista e devem ser resolvidos na especificação/arquitetura, sem ampliar o escopo funcional:

- Como representar e tratar um lançamento recorrente cujo valor padrão esteja em branco para que o saldo mensal não use um valor inexistente.
- O comportamento exato de criação/fechamento de mês e de geração idempotente de ocorrências recorrentes.
- Se meses anteriores a novembro de 2026 devem ser navegáveis sem dados ou se a navegação histórica começa nesse mês.
- Regras de precisão/arredondamento monetário e validação de valores.
- Armazenamento, autenticação e proteção dos dados financeiros para um usuário pessoal.

## 7. Registro de validação

O usuário confirmou a definição do problema e o público-alvo, detalhou o comportamento das recorrências e das duas visões, informou plataforma, origem dos dados e moeda, e definiu o KPI principal, os formulários necessários e o período inicial de novembro de 2026. A entrevista está pronta para a formulação do PRD na etapa seguinte.
