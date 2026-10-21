Você vai me ajudar a desenvolver um jogo para uma Game Jam com prazo extremamente curto. Antes de programar qualquer coisa, entenda completamente o conceito, a gameplay e a arquitetura que quero utilizar.

IMPORTANTE: posteriormente vou enviar meu GDD completo. Este texto serve como contexto técnico e conceitual inicial. Quando eu enviar o GDD, use-o como fonte principal para detalhes adicionais, mas mantenha os princípios de arquitetura e prioridade descritos aqui, a menos que eu peça explicitamente para alterá-los.

==================================================

1. CONCEITO GERAL
    ==================================================

O jogo é inspirado nas histórias e no imaginário popular de Franklin Cascaes sobre a Ilha de Santa Catarina, Florianópolis/SC, especialmente nas histórias envolvendo bruxas.

A premissa é:

O jogador precisa proteger um bebê durante aproximadamente 2 minutos e 30 segundos enquanto bruxas tentam invadir/atacar a casa.

A gameplay é baseada em:

* observação;
* identificação de sinais estranhos;
* gerenciamento de itens defensivos;
* cooldowns;
* reação rápida;
* eventos aleatórios;
* gerenciamento da proteção do bebê.

A partida deve durar aproximadamente 150 segundos.

Durante esses 150 segundos, ataques das bruxas podem acontecer aleatoriamente.

O jogador precisa observar o ambiente e perceber os sinais de que uma bruxa está prestes a atacar.

Os ataques possuem dois conceitos principais:

1. PRÉ-ATAQUE
2. ATAQUE

O pré-ataque é a janela de percepção do jogador.

Durante o pré-ataque, a bruxa dá algum sinal visual ou sonoro de que está prestes a atacar.

Se o jogador identificar corretamente o ataque e utilizar o item adequado, consegue impedir ou enfraquecer a bruxa.

Se não perceber ou não conseguir impedir o ataque antes do tempo acabar, o ataque entra na segunda fase.

A segunda fase é o ATAQUE propriamente dito.

Dependendo do tipo de ataque, o jogador deverá reagir de alguma maneira, podendo existir Quick Time Events.

==================================================
2. LOOP PRINCIPAL DA GAMEPLAY

O loop principal deve funcionar aproximadamente assim:

INÍCIO DA PARTIDA
↓
Timer começa em 150 segundos
↓
Jogador observa o ambiente
↓
Sistema aguarda intervalo aleatório
↓
Sistema seleciona um ataque aleatório
↓
PRÉ-ATAQUE
↓
Jogador percebe ou não percebe
↓
┌───────────────┐
│               │
DEFENDE          NÃO DEFENDE
│               │
↓               ↓
Ataque cancelado   ATAQUE
↓
QTE/reação
↓
sucesso/falha
↓
volta à gameplay
↓
próximo evento
↓
Timer chega a 0
↓
VITÓRIA

Se a vida/proteção do bebê chegar a 0:
↓
DERROTA

O jogo não deve ficar constantemente atacando o jogador.

É importante existir um intervalo entre eventos para permitir que o jogador observe o ambiente, utilize os itens e se prepare.

==================================================
3. SISTEMA DE TEMPO

A partida deve durar aproximadamente:

150 segundos = 2 minutos e 30 segundos.

O GameManager deve controlar o tempo global da partida.

Exemplo conceitual:

GameManager

* gameDuration = 150f
* currentTime
* babyHealth
* gameState

Estados possíveis:

* Waiting/Start
* Playing
* Victory
* Defeat
* Paused, se necessário

Enquanto o jogo estiver em Playing:

currentTime diminui com o tempo.

Quando currentTime <= 0:

→ vitória.

Quando o bebê perder toda a vida:

→ derrota.

O timer deve ficar separado do sistema de ataques.

O AttackManager NÃO deve ser responsável por controlar o tempo total da partida.

==================================================
4. SISTEMA DE ATAQUES

Quero um AttackManager responsável por controlar os ataques.

O AttackManager deve:

* decidir quando um ataque pode acontecer;
* escolher aleatoriamente qual ataque acontecerá;
* iniciar o pré-ataque;
* controlar a duração do pré-ataque;
* permitir que o jogador reaja;
* iniciar o ataque caso o jogador falhe;
* finalizar o evento;
* respeitar intervalos entre ataques.

Os ataques devem ser sistemas independentes.

Estrutura conceitual:

AttackManager
├── BullAttack
├── FireAttack
└── DoorAttack

Não quero que toda a lógica do jogo fique dentro de um único script gigante.

Cada ataque deve possuir sua própria lógica, mas utilizar os sistemas compartilhados do jogo.

Por exemplo:

BullAttack:

* mostra o sinal visual;
* aguarda resposta;
* verifica se o item correto foi usado;
* caso não seja defendido, inicia QTE;
* finaliza o ataque.

FireAttack:

* mostra o sinal visual;
* aguarda resposta;
* verifica proteção;
* inicia ataque caso necessário.

DoorAttack:

* inicia evento da porta;
* determina se é mãe ou bruxa;
* espera a decisão do jogador;
* resolve o evento.

==================================================
5. ALEATORIEDADE DOS ATAQUES

Os ataques devem ocorrer aleatoriamente.

Porém, NÃO quero ataques completamente caóticos que possam acontecer imediatamente uns após os outros sem espaço para o jogador reagir.

O AttackManager deve possuir algo semelhante a:

tempo mínimo entre ataques;
tempo máximo entre ataques.

Por exemplo:

Random.Range(minAttackInterval, maxAttackInterval)

Depois de um ataque terminar, o sistema espera novamente um intervalo aleatório antes de iniciar outro.

Posteriormente podemos adicionar dificuldade progressiva.

Exemplo:

Primeiros 40 segundos:

* ataques mais espaçados;
* pré-ataques mais fáceis de perceber.

40–100 segundos:

* ataques mais frequentes;
* pré-ataques um pouco mais rápidos.

100–150 segundos:

* maior frequência;
* sinais mais sutis;
* maior pressão.

Porém, isso deve ser configurável através de variáveis, e não valores fixos espalhados pelo código.

==================================================
6. ATAQUE 1 — BRUXA TRANSFORMADA EM BOI

Esse é o primeiro ataque que deve ser implementado porque será o ataque-base/vertical slice do projeto.

Durante o estado normal, o jogador olha pela janela.

Do lado de fora existe um pasto com bois normais.

Exemplo:

BOI
BOI
BOI
BOI
BOI

Em determinado momento, aleatoriamente, uma bruxa pode se transformar em um boi.

O jogador precisa perceber que existe um boi diferente.

==================================================
PRÉ-ATAQUE DO BOI

Durante o pré-ataque:

* um dos bois apresenta comportamento estranho;
* pode haver uma pequena transformação;
* pode existir mudança visual;
* pode existir mudança nos olhos;
* pode existir alteração da sombra;
* pode existir animação rápida;
* pode existir som diferente;
* etc.

Na primeira versão NÃO precisamos de uma animação complexa.

Podemos simplesmente trocar o visual de um boi normal por uma versão estranha por alguns frames/segundos.

O importante é existir uma janela de percepção.

Exemplo:

Boi normal
↓
pequena transformação
↓
Boi estranho
↓
janela de reação
↓
volta ao normal ou parte para o ataque.

==================================================
DEFESA DO BOI

Se o jogador perceber o boi estranho, deve utilizar o item apropriado.

Se a defesa for realizada durante o pré-ataque:

→ ataque cancelado;
→ bruxa é repelida;
→ feedback visual/sonoro;
→ evento termina.

O item utilizado entra em cooldown.

==================================================
ATAQUE DO BOI

Caso o jogador não consiga impedir o ataque durante o pré-ataque:

A bruxa parte para o ataque.

Nesse momento inicia um Quick Time Event.

A primeira versão do QTE pode ser simples:

“PRESSIONE [TECLA] RAPIDAMENTE”

O jogador precisa pressionar uma determinada tecla várias vezes dentro de um período.

Se atingir a quantidade necessária:

→ jogador repele a bruxa;
→ bebê não sofre dano;
→ ataque termina.

Se não atingir:

→ bebê sofre dano;
→ ataque termina ou entra em consequência definida.

O QTE deve ser controlado por um QTEManager separado do BullAttack.

O BullAttack solicita ao QTEManager:

StartQTE(…)

e recebe um resultado:

Success
ou
Failure.

Isso evita colocar toda a lógica do QTE dentro de cada ataque.

==================================================
7. SISTEMA DE ITENS

Os itens são fundamentais para a gameplay.

Cada item possui:

* nome;
* ícone;
* cooldown;
* duração, quando aplicável;
* estado de disponibilidade;
* função específica.

Deve existir um sistema central de itens.

Conceitualmente:

ItemManager
├── SaintMichael
├── AveMaria
└── Broom

Não quero criar três sistemas completamente independentes de cooldown.

O ItemManager deve controlar os cooldowns.

Cada item deve possuir seu próprio cooldown.

Exemplo:

Saint Michael:
cooldown = X segundos

Ave Maria:
cooldown = Y segundos

Broom:
cooldown = Z segundos

Esses valores precisam ser facilmente alteráveis no Inspector/configuração.

Enquanto um item estiver em cooldown:

→ não pode ser utilizado.

Quando o cooldown terminar:

→ item volta a estar disponível.

A UI deve mostrar visualmente o cooldown.

==================================================
8. SÃO MIGUEL

O terço possui duas orações diferentes.

Uma delas é a oração de São Miguel.

São Miguel é utilizado como uma ferramenta ofensiva/defensiva contra a bruxa.

A função conceitual é:

Jogador percebe uma bruxa em processo de ataque
↓
usa São Miguel
↓
bruxa é exorcizada/repelida
↓
ataque termina
↓
São Miguel entra em cooldown.

O efeito exato pode variar de acordo com o ataque.

==================================================
9. AVE MARIA

Ave Maria funciona de maneira diferente.

Ela não precisa necessariamente eliminar a bruxa.

Ela protege o bebê.

Quando o jogador utiliza Ave Maria:

→ o bebê recebe uma proteção temporária.

Durante alguns segundos:

BabyProtected = true

Visualmente:

* aura ao redor do bebê;
* efeito de luz;
* efeito religioso/místico;
* eventualmente som específico.

Enquanto BabyProtected == true:

Qualquer ataque que normalmente causaria dano ao bebê deve ser bloqueado.

Exemplo:

Bruxa ataca
↓
GameManager/AttackManager verifica:

Baby.IsProtected?

SIM:
→ dano bloqueado.

NÃO:
→ dano aplicado.

Isso é importante porque não quero criar uma lógica diferente para Ave Maria em cada ataque.

A proteção deve ser um estado global do bebê.

==================================================
10. SISTEMA DO BEBÊ

O bebê deve ter seu próprio componente/sistema.

BabyController ou BabyState.

Ele deve controlar:

* vida;
* estado de proteção;
* recebimento de dano;
* morte/derrota;
* efeitos visuais de proteção.

Conceitualmente:

Baby
├── Health
├── Protection
└── Damage handling

Quando um ataque tentar causar dano:

Baby.TakeDamage(amount)

Esse método deve verificar se o bebê está protegido.

Se estiver protegido:

→ não recebe dano.

Caso contrário:

→ perde vida.

Isso mantém a lógica centralizada.

==================================================
11. ATAQUE 2 — BRUXA EM CHAMAS

O segundo ataque é baseado na ideia de que as bruxas poderiam se transformar em chamas.

Ainda precisamos definir completamente a estética, mas a ideia que mais gosto é a entrada pela chaminé.

A casa possui uma chaminé.

Durante o estado normal:

→ nada estranho acontece.

Durante o pré-ataque:

* começa a sair fumaça da chaminé;
* a fumaça pode mudar de aparência;
* pode surgir uma pequena chama;
* pode haver som;
* a chama pode crescer;
* eventualmente a presença da bruxa fica evidente.

Isso cria um sinal visual semelhante ao boi.

O jogador precisa perceber:

“Tem alguma coisa entrando pela chaminé.”

==================================================
DEFESA DO FOGO

O item principal para esse ataque é São Miguel.

Jogador percebe:

→ usa São Miguel
→ bruxa é exorcizada
→ ataque termina
→ item entra em cooldown.

Também existe Ave Maria.

Se Ave Maria estiver ativa:

→ bebê fica protegido;
→ ataque pode acontecer;
→ dano é bloqueado.

Caso nenhum sistema defensivo seja utilizado:

→ bruxa entra;
→ inicia a fase de ataque.

O ataque de fogo pode utilizar um QTE diferente futuramente.

Por exemplo:

Aparecem teclas em sequência:

A → S → D → W

O jogador precisa pressionar corretamente.

Porém, essa variação de QTE é secundária.

Primeiro precisamos fazer o QTE básico funcionar.

==================================================
12. ATAQUE 3 — A PORTA

O terceiro ataque acontece na porta da casa.

Quando existe um evento na porta:

O jogador pode receber uma visita da mãe.

Porém, a bruxa também pode se disfarçar de mãe.

Portanto:

Evento da porta
↓
sorteio
↓
Mãe
OU
Bruxa disfarçada

==================================================
CASO SEJA A MÃE

A mãe bate/chama.

O jogador pode utilizar a vassoura atrás da porta.

Se for realmente a mãe:

→ não acontece nada de ruim.

O jogador pode abrir a porta.

A mãe verifica se está tudo bem.

Depois:

→ evento termina;
→ gameplay continua.

==================================================
CASO SEJA A BRUXA

A bruxa está disfarçada de mãe.

Ela pode fornecer algum sinal que permita ao jogador perceber:

* voz ligeiramente errada;
* som de agonia;
* ruído estranho;
* comportamento anormal;
* silêncio estranho;
* algum detalhe visual.

Se o jogador perceber:

→ NÃO abre a porta;
→ utiliza a vassoura ou simplesmente mantém a porta fechada;
→ bruxa é repelida/enfraquecida;
→ evento termina.

A vassoura serve para enfraquecer os poderes da bruxa atrás da porta.

==================================================
SE O JOGADOR ABRIR A PORTA PARA A BRUXA

Ainda não quero obrigatoriamente utilizar QTE aqui.

O evento deve ter uma consequência própria.

Por exemplo:

Jogador abre
↓
descobre que é a bruxa
↓
som/efeito de susto
↓
bruxa invade
↓
bebê sofre dano ou ocorre uma consequência grave.

A razão para evitar QTE inicialmente é que essa situação pode ficar artificial se simplesmente colocarmos:

“APERTE X RAPIDAMENTE”

depois que a porta já foi aberta.

Primeiro implementar a consequência simples.

Depois, se houver tempo, podemos testar um QTE específico.

==================================================
13. VASSOURA

A vassoura é um item defensivo específico da porta.

Quando existe um evento na porta:

→ jogador pode posicionar a vassoura atrás da porta.

Se for a mãe:

→ nada acontece.

Se for a bruxa:

→ vassoura enfraquece a bruxa;
→ bruxa reage com som de agonia;
→ bruxa é repelida;
→ evento termina.

A vassoura também deve ter cooldown.

==================================================
14. QTE MANAGER

Quero um sistema separado chamado QTEManager.

Ele deve ser genérico.

O QTEManager deve conseguir receber parâmetros como:

* tecla;
* quantidade necessária de pressionamentos;
* tempo disponível;
* dificuldade;
* tipo de QTE.

Exemplo conceitual:

StartMashQTE(
key,
requiredPresses,
duration
)

Retorno:

Success
ou
Failure.

Posteriormente pode existir:

StartSequenceQTE(
keySequence,
timeLimit
)

Mas isso NÃO é prioridade inicial.

Primeiro:

Mash QTE.

==================================================
15. GAME STATES

Quero evitar lógica espalhada.

O jogo pode possuir estados globais:

GameState:

* Playing
* AttackPreWarning
* AttackActive
* Victory
* Defeat

Ou uma estrutura equivalente.

O importante é garantir que o jogo saiba em que estado está.

Por exemplo:

Enquanto um ataque está acontecendo:

→ não iniciar outro ataque aleatório.

Quando o ataque termina:

→ voltar para Playing.

==================================================
16. UI

A interface mínima deve mostrar:

* tempo restante;
* vida do bebê;
* itens disponíveis;
* cooldown dos itens;
* eventualmente feedback do pré-ataque;
* QTE quando necessário.

O timer deve ser extremamente claro.

Exemplo:

02:30

e diminuir até:

00:00

A vida do bebê pode ser:

❤️❤️❤️

ou qualquer representação visual definida no GDD.

Os cooldowns podem aparecer como:

SÃO MIGUEL
[██████░░] 3s

AVE MARIA
[████░░░░] 2s

VASSOURA
[████████] PRONTO

==================================================
17. ARQUITETURA DE CÓDIGO

Não quero um único script gigante.

A arquitetura inicial deve ser aproximadamente:

GameManager
│
├── AttackManager
│     ├── BullAttack
│     ├── FireAttack
│     └── DoorAttack
│
├── ItemManager
│     ├── SaintMichael
│     ├── AveMaria
│     └── Broom
│
├── BabyController
│
├── QTEManager
│
└── UIManager

Dependendo da engine, os nomes e implementação podem mudar.

O princípio é mais importante que os nomes.

Cada sistema deve possuir responsabilidade clara.

GameManager:
→ controla a partida.

AttackManager:
→ controla eventos/ataques.

Individual Attack scripts:
→ controlam comportamento específico de cada ataque.

ItemManager:
→ controla utilização e cooldowns.

BabyController:
→ controla vida e proteção.

QTEManager:
→ controla Quick Time Events.

UIManager:
→ atualiza interface.

==================================================
18. PRINCÍPIO DE REUTILIZAÇÃO

Evite duplicar código.

Exemplo ruim:

BullAttack:
código próprio de cooldown da Ave Maria.

FireAttack:
outro código diferente de cooldown da Ave Maria.

DoorAttack:
terceiro código de cooldown da Ave Maria.

Isso NÃO deve acontecer.

Deve existir:

ItemManager
↓
AveMaria
↓
Cooldown

Todos os ataques simplesmente perguntam ao sistema:

“O bebê está protegido?”

ou:

“O item está disponível?”

Isso torna o projeto muito mais fácil de manter.

==================================================
19. PRIORIDADE ABSOLUTA DE DESENVOLVIMENTO

O prazo da Game Jam é muito curto.

Portanto, NÃO devemos tentar desenvolver tudo simultaneamente.

A ordem obrigatória recomendada é:

==============================
FASE 1 — MVP

1. Criar cena principal.
2. Criar bebê.
3. Criar timer de 150 segundos.
4. Criar vida do bebê.
5. Criar GameManager.
6. Criar condição de vitória.
7. Criar condição de derrota.
8. Criar AttackManager.
9. Criar sistema de intervalo aleatório entre ataques.

==============================
FASE 2 — PRIMEIRO ATAQUE

10. Criar pasto/janela.
11. Criar boi normal.
12. Criar boi estranho.
13. Criar pré-ataque do boi.
14. Criar janela de reação.
15. Criar item defensivo.
16. Criar cooldown.
17. Fazer defesa cancelar ataque.
18. Fazer falha iniciar ataque.
19. Criar QTE.
20. Fazer QTE ter sucesso/falha.
21. Fazer falha causar dano.

Nesse ponto já precisamos ter um jogo MINIMAMENTE JOGÁVEL.

==============================
FASE 3 — SISTEMA DE ITENS

22. ItemManager.
23. São Miguel.
24. Ave Maria.
25. Proteção temporária do bebê.
26. Cooldowns individuais.
27. UI dos cooldowns.

==============================
FASE 4 — ATAQUE DE FOGO

28. Chaminé.
29. Fumaça.
30. Pré-ataque.
31. São Miguel repelindo o fogo.
32. Ataque de fogo.
33. Integração com proteção da Ave Maria.

==============================
FASE 5 — ATAQUE DA PORTA

34. Porta.
35. Batida/chamada.
36. Sorteio mãe/bruxa.
37. Sinal sonoro da bruxa.
38. Vassoura.
39. Bruxa repelida.
40. Consequência de abrir para a bruxa.

==============================
FASE 6 — POLIMENTO

Som.
Música.
Partículas.
Animações.
Screen shake.
Iluminação.
Transições.
Feedback visual.
Efeitos sobrenaturais.
Detalhes inspirados em Florianópolis/Cascaes.

==================================================
20. REGRA MAIS IMPORTANTE PARA A GAME JAM

Se o tempo estiver acabando:

NÃO adicionar conteúdo novo.

Garantir que o seguinte esteja funcionando:

* partida inicia;
* timer funciona;
* bebê existe;
* bebê pode sofrer dano;
* ataques aparecem;
* pelo menos um ataque funciona;
* item funciona;
* cooldown funciona;
* QTE funciona;
* vitória funciona;
* derrota funciona.

Se isso estiver pronto, temos um jogo entregável.

Depois adicionamos os outros ataques.

É preferível entregar:

UM ATAQUE MUITO BEM FUNCIONAL

do que:

TRÊS ATAQUES QUEBRADOS.

==================================================
21. ASSETS — PRIORIDADE

Assets essenciais:

Cenário:

* quarto;
* janela;
* pasto;
* bois;
* porta;
* chaminé;
* bebê.

Bruxas:

* forma normal;
* forma de boi;
* forma de fogo;
* disfarce de mãe.

Itens:

* terço;
* representação de São Miguel;
* representação de Ave Maria;
* vassoura.

UI:

* timer;
* vida;
* ícones dos itens;
* cooldowns;
* QTE.

Som:

* ambiente;
* vento;
* bois;
* transformação;
* chaminé/fogo;
* batida na porta;
* voz da mãe;
* som estranho da bruxa;
* agonia da bruxa;
* utilização do terço;
* sucesso/falha;
* dano;
* vitória;
* derrota.

IMPORTANTE:

Durante a Game Jam, assets temporários são aceitáveis.

Não quero bloquear a programação esperando os assets finais.

Podemos utilizar:

* sprites simples;
* placeholders;
* formas geométricas;
* imagens provisórias;
* sons provisórios.

Depois substituímos pelos assets finais.

==================================================
22. FILOSOFIA DE DESENVOLVIMENTO

Sempre que eu pedir ajuda para implementar alguma coisa, considere:

1. Qual é a versão mais simples que funciona?
2. Isso é necessário para a gameplay?
3. Podemos reutilizar algum sistema existente?
4. Isso precisa ser feito agora ou pode esperar?
5. Essa implementação pode quebrar outro sistema?
6. Existe uma forma mais simples adequada para uma Game Jam?

Não quero soluções excessivamente complexas se uma solução simples funcionar.

O objetivo não é construir uma arquitetura empresarial.

O objetivo é entregar um jogo funcional, estável e divertido dentro do prazo.

==================================================
23. COMO VOCÊ DEVE ME AJUDAR

Quando eu enviar o GDD, analise primeiro.

Não comece automaticamente criando centenas de scripts.

Primeiro:

1. Identifique todos os sistemas necessários.
2. Separe sistemas essenciais de sistemas secundários.
3. Identifique dependências.
4. Monte a ordem de implementação.
5. Diga quais scripts/classes/componentes precisam existir.
6. Diga quais objetos precisam existir na cena.
7. Diga quais assets são necessários.
8. Só então começamos a implementar.

Quando começarmos a programar:

* faça uma etapa por vez;
* explique onde cada script deve ser colocado;
* explique quais objetos precisam existir;
* explique quais componentes precisam ser adicionados;
* explique quais variáveis precisam ser configuradas;
* evite criar dependências desnecessárias;
* mantenha o código modular;
* priorize soluções rápidas e confiáveis.

Se eu estiver utilizando uma engine específica, adapte toda a implementação à engine e versão que eu informar.

Se algum detalhe do GDD entrar em conflito com essa arquitetura, não assuma automaticamente uma solução. Explique o conflito e proponha a solução mais simples e segura para a Game Jam.

==================================================
24. OBJETIVO FINAL

O jogador deve sentir que está:

“sozinho em uma casa, tentando proteger um bebê durante a noite enquanto tenta perceber as manifestações de bruxas antes que elas consigam entrar.”

A tensão deve vir principalmente da OBSERVAÇÃO.

O jogador não deve simplesmente esperar um alerta vermelho.

Ele precisa olhar para:

* a janela;
* o pasto;
* a chaminé;
* a porta;
* os sons;
* os comportamentos estranhos.

A gameplay deve fazer o jogador pensar:

“Isso era um boi normal ou era a bruxa?”

“Eu ouvi alguma coisa na chaminé?”

“É realmente minha mãe atrás dessa porta?”

“Uso meu item agora ou guardo para o próximo ataque?”

Esse é o núcleo da experiência.

O jogo deve combinar:

OBSERVAÇÃO
+
GERENCIAMENTO DE RECURSOS
+
REAÇÃO
+
ALEATORIEDADE
+
ATMOSFERA DE FOLCLORE CATARINENSE.

Agora aguarde o meu GDD. Quando eu enviar, use todas essas informações juntamente com o GDD para montar o plano técnico definitivo do projeto.