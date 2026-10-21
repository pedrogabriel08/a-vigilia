# A Vigília — protótipo da Game Jam

Projeto: `raiz do projeto clonado`

Unity: **6000.6.7f1**, projeto 2D. Cena de jogo: `Assets/_Game/Scenes/Main_MVP.unity`.

## Começar

Abra **Game Jam > Abrir Base MVP**, pressione **Play** e clique em **INICIAR VIGÍLIA**.

A partida dura 150 segundos. O bebê começa com 100 de vida. Os eventos acontecem um por vez, com um intervalo aleatório de 8 a 14 segundos entre eles.

## Controles e defesas

Clique no armário para abrir. Arraste um item com o botão esquerdo até seu local de uso e solte. O item retorna ao armário. Soltar fora de um alvo aceito não gasta a recarga; usar em um local aceito consome a recarga mesmo sem ameaça presente.

| Item | Local de uso | Resultado | Recarga |
|---|---|---|---|
| Tesoura | Janela | Cancela totalmente o pré-ataque do boi. | 5 s |
| Terço | Berço | Ave-Maria: proteção por 15 segundos. | 30 s |
| Terço | Chaminé | São Miguel: cancela o pré-ataque do fogo. | Os mesmos 30 s do terço |
| Vassoura | Porta | Se for bruxa, provoca um gemido e enfraquece sua resistência. | 7 s |

O terço é um único item: as duas orações compartilham a recarga. A proteção deve ser ativada antes da fase de ataque. Enquanto ela estiver ativa nesse momento, a ameaça é bloqueada sem iniciar QTE.

## Boi e fogo

Na janela, um boi muda de aparência por 5 segundos. Na chaminé, aparecem fumaça e chama durante o pré-ataque. Use a defesa apropriada antes de essa janela terminar.

Se a ameaça avançar sem proteção, começa o QTE:

- **12 cliques no botão de reação ou 12 pressionamentos de Espaço.**
- Prazo de **8 segundos**.
- Perda de **5 pontos de vida por segundo** durante o desafio.
- Ao atingir a quantidade, o ataque termina e o dano para.
- Ao esgotar o prazo, o ataque termina mantendo o dano já recebido, sem dano extra.
- A derrota acontece quando a vida chega a zero.

O tempo da partida continua durante os desafios. Não é possível abrir o armário ou pegar itens durante um QTE. Um item já carregado é devolvido quando o desafio começa.

## Porta

A visitante anuncia que vai entrar. Ela pode ser a mãe ou uma bruxa disfarçada. O som de batida e o texto são sinais do evento.

1. Use a vassoura na porta durante o aviso. Um gemido indica a bruxa.
2. Clique na porta durante o aviso para iniciar a resistência.
3. Mantenha o botão de resistência pressionado. Soltar interrompe o acúmulo até pressionar novamente; o prazo continua correndo.

A resistência exige 4 segundos dentro de um prazo de 7 segundos. Contra uma bruxa enfraquecida pela vassoura, exige 2,5 segundos. Durante a resistência à bruxa há dano gradual de 5 pontos por segundo.

Se a bruxa vencer a resistência ou o jogador deixar o aviso expirar sem impedir a entrada, ela invade e causa derrota. Não existe um segundo QTE após a invasão. O escudo ativo antes da investida também bloqueia esse evento.

Deixar a mãe entrar é inofensivo: ela aparece brevemente e sai. Impedir a entrada da mãe também não causa dano ao bebê.

## Resultado e apresentação

Sobreviver até o timer chegar a zero produz vitória. A derrota mostra um susto provisório por um segundo e depois a tela de resultado. **TENTAR NOVAMENTE** restaura a partida sem recarregar a cena.

O cenário, personagens, ícones e o susto ainda usam arte provisória. Os sons gerados por código são placeholders para batida, gemido, vitória e derrota; não são vozes gravadas nem os assets OGG/MP3 finais. Música, animações finais, PSDs e polimento ainda estão pendentes.

A lamparina e ataques simultâneos continuam adiados, preservando a regra de um evento por vez.

## Configuração no Inspector

| Objeto | Valores principais |
|---|---|
| `Systems/GameManager` | Game Duration: 150; referência ao bebê. |
| `Canvas/Room_Placeholders/Cradle/Baby` | Max Health: 100. |
| `Systems/AttackManager` | Min/Max Attack Interval: 8/14; lista de boi, fogo e porta. |
| `Systems/BullAttack` e `Systems/FireAttack` | Pre Attack Duration: 5; Required Presses: 12; QTE Duration: 8; Damage Per Second: 5. |
| `Systems/DoorAttack` | Witch Chance: 0,6; Hold Duration: 4; Weakened Hold Duration: 2,5; Response Time: 7. |
| `Systems/ItemManager` | Tesoura: 5; Terço: 30; Vassoura: 7. |
| `Systems/RosaryPrayers` | Protection Duration: 15. |
| `Systems/QTEManager` | Mash Key: Space. |

As artes ficam separadas das regras e das áreas transparentes de uso. Preserve componentes e referências ao substituir os objetos. Os elementos atuais usam **UI Image** dentro de Canvas.

## Verificações e build

O menu **Game Jam > Preparar e verificar versão atual** prepara as referências e executa as verificações de base, agendamento, itens, QTE, fogo, terço e porta. Foram aprovadas **93 verificações** na versão integrada.

Os relatórios ficam em `Library/Vigilia*Verification.txt`. Eles cobrem lógica e callbacks; não substituem testes de jogabilidade e balanceamento com pessoas.

Use **Game Jam > Gerar build Web** para iniciar a exportação para navegador. O destino é `Builds/Web` dentro do projeto e o relatório é `Library/VigiliaWebBuild.txt`. Somente um resultado **Succeeded** confirma a build. A exportação é release, com os controles de desenvolvimento ocultos e sem compressão para facilitar hospedagem.

Para testar a build localmente, sirva a pasta por HTTP; abrir `index.html` com duplo clique pode não funcionar. A publicação na itch.io deve usar um ZIP contendo `index.html`, `Build` e `TemplateData` na raiz. O jogo não foi publicado automaticamente.

## Trocar as artes e reorganizar o quarto

Trabalhe com o Play desligado e salve uma cópia da cena antes de reorganizar. Em cada objeto de arte, troque o `Source Image` do componente `Image`. Os itens de armário continuam precisando de `DraggableItem` e `CanvasGroup`; as áreas de uso continuam precisando de `ItemDropTarget` e de raycast ativo.

Ao mover a janela, o berço, a chaminé ou a porta, mova o grupo correspondente para levar junto as áreas de interação. Para alterar somente a aparência, mantenha esses componentes nos objetos e ajuste os filhos de arte. As posições de UI são controladas por `RectTransform` e suas âncoras.

Os menus **Preparar** são instaladores do protótipo e alguns deles reposicionam elementos do armário/interface. Depois de aplicar a arte e o layout final, evite executá-los sem necessidade. Os menus **Verificar** executam somente as verificações indicadas.

## Cópia do projeto

`A-Vigilia-Projeto.zip` contém `Assets`, `Packages` e `ProjectSettings`. Extraia em uma pasta nova e abra essa pasta pelo Unity Hub usando 6000.6.7f1. A Unity reconstruirá a Library na primeira abertura. A cópia não inclui cache nem builds.

`Verificacoes-prototipo.txt` reúne os relatórios das 93 verificações. O próximo trabalho de conteúdo é substituir a arte e os sons provisórios, testar a dificuldade com pessoas e só então ajustar os valores no Inspector.

## Estado ao encerrar a sessão — 21/10/2026, 17h30

O protótipo foi salvo e as 93 verificações passaram. A exportação Web foi iniciada, mas permanecia em execução na Unity no encerramento. A build ainda não foi validada no navegador nem empacotada para upload. Consulte `Library/VigiliaWebBuild.txt` e aguarde `Succeeded` antes de usar os arquivos exportados.

