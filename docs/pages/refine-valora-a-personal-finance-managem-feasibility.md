# Valora — viabilidade do codebase e impacto arquitetural

**Playbook:** `technical_refinement`  
**Etapa:** `codebase_feasibility`  
**Objetivo:** Refinar `valora-a-personal-finance-management-too-prd`  
**Data:** 8 de novembro de 2026  
**Status:** Avaliação concluída; arquitetura preliminar recomendada, decisões de produto identificadas para confirmação.

## 1. Resumo executivo

O repositório está praticamente vazio do ponto de vista de produto: contém um README curto, o PRD e o registro de elicitação, sem código de aplicação, testes, configuração de build, dependências, infraestrutura, API ou esquema de dados. Portanto, não há arquitetura existente cuja compatibilidade possa ser avaliada, nem base para estimar esforço com precisão.

Para a V1 de uso pessoal e sem integrações, recomenda-se começar com uma aplicação web modular em um único deploy e uma persistência transacional simples. Manter UI, regras de domínio e acesso a dados separados dentro do mesmo processo reduz acoplamento e facilita testes e evolução, sem impor microserviços, filas ou cache distribuído sem necessidade demonstrada. A geração recorrente deve ser idempotente e transacional, com uma chave de unicidade por regra de origem e mês de referência.

O PRD precisa esclarecer alguns comportamentos para que a implementação seja determinística: limite inferior da navegação mensal, significado de “mês criado/ativado”, ciclo de vida da recorrência, efeito de editar/parar uma série e se a configuração de valor padrão é uma regra persistente ou apenas a instrução para a próxima ocorrência. Recomendações provisórias são apresentadas como propostas, não como decisões já confirmadas pelo usuário.

## 2. Evidências e limites da análise

### Encontrado no workspace

- `README.md`: nome e descrição resumida do produto.
- `docs/pages/valora-a-personal-finance-management-too-prd.md`: requisitos, critérios de aceite, modelo conceitual e riscos ainda pendentes.
- `docs/pages/valora-a-personal-finance-management-too-elicitation-log.md`: escopo relatado, contexto de uso individual, BRL e ausência de integrações.
- `.gitignore` modificado e `.squad-state-main.json` criado pela sessão do playbook.
- Não foram encontrados arquivos de aplicação, manifestos de dependências, migrações, esquemas, testes, pipeline CI/CD ou IaC.

### Limitações

A avaliação é de viabilidade e desenho inicial, não uma revisão de código. Não há framework, banco, hospedagem, ambiente de execução ou meta de disponibilidade definida. A ferramenta Context7 exigida pelo roteiro de pesquisa não está exposta entre as ferramentas disponíveis nesta sessão; como não há bibliotecas ou versões escolhidas no projeto, nenhuma afirmação específica de versão foi feita. A pesquisa de documentação de bibliotecas deve ocorrer quando a stack for definida e antes do desenho de implementação correspondente.

## 3. Leitura arquitetural dos requisitos

### Domínios e responsabilidades

1. **Lançamentos:** receitas e despesas, descrição, valor pendente ou preenchido, referência mensal e eventual observação.
2. **Recorrência:** regra que origina ocorrências futuras, configuração para valor padrão ou pendente e estado ativo/inativo.
3. **Consulta mensal:** seleção do período, listagem filtrada e saldo derivado dos lançamentos daquele período.

A distinção entre regra de recorrência e ocorrência mensal merece ser explícita no modelo. Uma ocorrência é um registro financeiro independente, editável no mês correspondente; a regra descreve a continuidade para meses futuros. Isso evita que uma edição retroativa reescreva o histórico e permite valores diferentes por mês.

### Requisitos de consistência

- O saldo é uma projeção calculada a partir dos lançamentos do período, não um valor armazenado como fonte independente da verdade.
- Ocorrências sem valor devem ter estado explicitamente pendente (valor ausente), e não valor zero. O saldo soma somente valores preenchidos; a UI também deve sinalizar a quantidade de pendências para evitar que saldo parcial seja interpretado como definitivo.
- A visão de avulsos é uma consulta filtrada da mesma coleção de ocorrências, não uma segunda cópia de dados. Assim, o item aparece nas duas visões sem risco de dupla contagem.
- Mês de referência deve ser uma data civil normalizada, como `YYYY-MM` ou primeiro dia do mês, sem depender do fuso horário do navegador ou do servidor.
- Valores BRL devem ser representados em unidades inteiras de centavos ou tipo decimal exato. Evitar ponto flutuante binário para cálculos monetários. Limites máximos, aceitação de valores negativos, arredondamento e formato de entrada ainda precisam ser definidos.

## 4. Arquitetura preliminar recomendada

### Forma de implantação

Aplicação web modular em um único serviço implantável, com camadas e módulos internos claros:

- interface e camada de aplicação para comandos e consultas;
- domínio com regras de saldo, recorrência e validação monetária;
- repositórios/adapter de persistência atrás de contratos pequenos;
- banco transacional como fonte de verdade.

A escolha concreta entre renderização no servidor ou SPA, linguagem, framework e banco deve seguir as preferências de implementação e hospedagem, ainda não registradas. Para a carga individual conhecida, começar com banco relacional local/gerenciado e backup verificável é suficiente. Não há justificativa atual para microsserviços, CQRS, Redis, event sourcing ou mensageria. Se houver crescimento ou colaboração multiusuário no futuro, fronteiras de módulo e interfaces de repositório permitem evolução gradual.

### Contratos de domínio sugeridos

- `Money`: inteiro de centavos, moeda BRL implícita na V1; entradas inválidas rejeitadas antes de persistir.
- `Month`: chave civil `YYYY-MM`, comparável e independente de timezone.
- `Transaction`: `id`, `kind` (income/expense), `description`, `amountCents` anulável somente quando `status=pending_amount`, `month`, `recurrenceRuleId` anulável, `note` opcional, timestamps.
- `RecurrenceRule`: `id`, dados-base necessários para instanciar, `active`, política da próxima ocorrência (`copy_default`/`leave_pending`), valor padrão opcional e início/fim quando definidos.
- `MonthlySummary`: totais e saldo calculados sobre as transações preenchidas do mês, mais contagem de pendências.

Restrições indicativas: `amountCents IS NULL` somente para pendência; valor preenchido não negativo (a confirmar; natureza já determina sinal); unicidade de `(recurrence_rule_id, month)` para ocorrências geradas. O banco deve impor as invariantes críticas além da validação da aplicação.

## 5. Fluxos técnicos e garantias

### Criar lançamento

Validar descrição, natureza, mês e valor; persistir a ocorrência e, quando recorrente, a regra/configuração associada em uma única transação. A criação deve deixar claro se o formulário cadastra a regra e a ocorrência do mês atual de forma inseparável.

### Consultar mês

Uma única consulta filtrada por chave do mês fornece lançamentos para a visão completa; a visão não recorrente aplica filtro adicional. O resumo agrega receitas e despesas com valor preenchido, excluindo pendências, e retorna contagem de valores em falta. Paginação ainda não é necessária para o volume esperado, mas deve ser considerada se o histórico crescer de forma relevante.

### Gerar mês/ocorrências recorrentes

A criação do mês seguinte e a geração devem ser seguras para repetição. Dentro de transação, selecionar regras ativas elegíveis e inserir ocorrências com `INSERT ... ON CONFLICT` equivalente ou captura de conflito pela chave única. Uma falha parcial deve permitir repetir o comando sem duplicar registros. Para o único usuário e baixo volume, não é necessária fila ou DLQ; uma operação síncrona no acesso/criação do mês é mais simples, desde que idempotente.

É necessário evitar deriva acidental: a próxima ocorrência deve ser baseada na regra explícita, não em um valor editado na ocorrência anterior, exceto se a política definida disser que o valor anterior passa a ser o padrão. Se a regra for alterada ou desativada, a mudança deve valer prospectivamente e preservar ocorrências já criadas.

### Edição e exclusão

A edição de uma ocorrência deve atualizar apenas aquele mês. Editar a regra deve afetar apenas gerações futuras. Exclusão de uma ocorrência gerada não deve reativá-la silenciosamente na próxima abertura do mês; é necessário definir se ela pode ser recriada, ignorada via marca de exceção ou se exclusões são soft-delete. A solução mais robusta é preservar a identidade de origem/mês e registrar uma exceção/cancelamento, mas esse detalhe pode ser simplificado se exclusão de recorrências ficar fora da V1.

## 6. Segurança, privacidade e operação

O uso é individual, mas os dados são financeiros. O PRD ainda não decide se a aplicação será apenas local ou acessível pela internet. Essa decisão determina o controle de acesso necessário e não deve ser inferida pelo desenho técnico.

- Se acessível remotamente: autenticação obrigatória, sessões seguras, proteção contra CSRF conforme o modelo de sessão, HTTPS, segredo fora do repositório e política de recuperação de conta.
- Se somente local/privada: restringir exposição de rede e proteger o dispositivo e o arquivo/banco conforme a plataforma; ausência de login não equivale a proteção se a instância puder ser alcançada por terceiros.
- Minimizar dados pessoais: a V1 não precisa de nome legal, dados bancários ou integrações.
- Backup: cópia periódica consistente do banco e teste de restauração documentado. Frequência e retenção devem refletir quanto histórico o usuário aceita perder.
- Observabilidade proporcional: logs sem valores ou descrições financeiras, erros técnicos sem payload sensível e verificação de saúde básica. Métricas distribuídas e tracing não são necessários para o volume conhecido.
- Atualizações do esquema via migrações versionadas e backup antes de mudanças irreversíveis. Alterações destrutivas devem ter caminho de rollback ou restauração documentado.

## 7. Falhas, escalabilidade e recuperação

| Risco | Efeito | Controle recomendado para V1 |
|---|---|---|
| Geração executada duas vezes | Lançamentos duplicados | Restrição única por regra/mês e comando idempotente |
| Falha durante geração mensal | Mês parcialmente populado | Transação atômica; retry seguro |
| Regra editada após ocorrências criadas | Histórico alterado ou expectativa confusa | Separar regra e ocorrência; aplicar alteração prospectivamente |
| Saldo inclui pendências como zero | Usuário interpreta saldo parcial como final | Valor anulável + estado pendente + contador/aviso |
| Erro de ponto flutuante | Divergência em totais | Centavos inteiros ou decimal exato |
| Banco/dispositivo perdido | Perda de histórico | Backup automatizado e restauração ensaiada |
| Acesso remoto sem autenticação | Exposição de dados financeiros | Definir modelo de implantação antes de publicar; autenticar instâncias remotas |
| Falha de timezone na virada do mês | Referência mensal errada | `Month` civil explícito, independente de instante/fuso |

A demanda de uma pessoa não sustenta arquitetura distribuída agora. Escala horizontal da camada web pode ser considerada posteriormente se o serviço for stateless e os dados compartilhados estiverem no banco; sessões locais em memória e arquivos locais por instância impediriam isso. Como isso ainda não é necessário, basta não espalhar estado de negócio em memória nem acoplar regras ao framework.

## 8. Decisões e perguntas que afetam implementação

### Recomendações provisórias

1. Permitir navegação de novembro de 2026 em diante; meses anteriores ficam fora do produto e não devem ser alcançáveis. Isso respeita o período inicial declarado sem prometer histórico vazio anterior.
2. Tratar “criar/ativar mês” como operação idempotente disparada ao abrir ou solicitar um mês elegível, em vez de exigir fechamento manual do mês anterior.
3. Definir recorrência mensal contínua enquanto ativa, com ocorrências editáveis independentemente e regra aplicada só ao futuro.
4. Exigir valor preenchido ao criar uma ocorrência corrente; valor ausente só é permitido nas ocorrências futuras geradas configuradas como pendentes.
5. Definir se valor negativo é permitido. Recomendação: não; receitas e despesas têm natureza própria, valores são magnitude não negativa.

Essas recomendações não foram explicitamente confirmadas na elicitação. Antes de fixar os critérios de aceite técnicos, o responsável pelo produto deve confirmar ou alterar as decisões relevantes.

### Questões abertas prioritárias

- A navegação pode ir antes de novembro de 2026 ou deve ter esse limite rígido?
- A geração acontece ao abrir o mês, ao criá-lo manualmente ou em outro momento? Meses podem ser abertos fora de ordem?
- Recorrência é mensal e indefinida por padrão? Pode ser encerrada, pausada ou ter início/fim?
- Mudar “valor padrão” modifica apenas a próxima ocorrência ou todas as futuras? O que é copiado quando não há padrão?
- Alterar uma ocorrência afeta somente o mês atual? A recomendação é sim.
- O usuário pode excluir lançamentos ou recorrências? Qual é o comportamento para ocorrências já geradas?
- Uso será local/privado ou publicado com acesso remoto? Isso define autenticação e hospedagem.
- Qual volume máximo por lançamento e qual política de precisão/validação monetária?

## 9. Impacto no PRD

Para tornar o PRD implementável, acrescentar ou esclarecer:

- regra explícita de modelagem “regra de recorrência” versus “ocorrência mensal”;
- limite inferior de navegação e comportamento para mês futuro ainda não materializado;
- gatilho de geração, elegibilidade, comportamento fora de ordem e idempotência;
- efeitos prospectivos de edição/desativação de regra e efeitos locais de edição de ocorrência;
- política de exclusão e prevenção de recriação indesejada;
- estado de pendência e apresentação de saldo parcial;
- limites e validação de valores BRL, incluindo negativos e teto de valor;
- requisito de autenticação conforme decisão de implantação;
- requisito de backup e restauração coerente com a hospedagem escolhida.

## 10. Parecer de viabilidade

A V1 é tecnicamente viável como aplicação web pequena e transacional. O núcleo de maior risco não é volume ou distribuição: são semântica de recorrência, valores pendentes, segurança condicionada à forma de hospedagem e preservação do histórico. Uma arquitetura modular em monólito e as restrições idempotentes no banco atendem ao escopo declarado com baixo custo operacional. O PRD pode avançar à etapa de planejamento depois de as decisões de produto acima serem confirmadas ou registradas como recomendações aceitas.
