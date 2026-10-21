# Mensagem pronta para a próxima IA

Você vai me ajudar a continuar um jogo Unity de Game Jam chamado **A Vigília**. Já existe um protótipo implementado. **Leia os arquivos existentes e preserve o trabalho; não recrie o projeto do zero.** Converse comigo em português e explique os passos no Unity de forma acessível. Priorize uma versão simples, funcional e adequada ao prazo.

## Ambiente e objetivo

- Repositório: https://github.com/pedrogabriel08/a-vigilia
- Engine real: **Unity 6000.6.7f1** (o usuário inicialmente abreviou a versão; `ProjectSettings/ProjectVersion.txt` é a fonte correta).
- Jogo **2D**, câmera fixa, interação com mouse no quarto. Não há movimentação do personagem/câmera.
- Destino: navegador, com futura publicação na itch.io. Instalar Web Build Support no Unity Hub.
- Cena principal: `Assets/_Game/Scenes/Main_MVP.unity`.
- Código: `Assets/_Game/Scripts`; ferramentas de configuração/verificação: `Assets/_Game/Editor`.
- Não dependa do caminho original do computador anterior; use a raiz clonada.

O jogador é um pai que protege seu bebê por 150 segundos, observando manifestações de bruxas. A atmosfera vem do folclore catarinense/Franklin Cascaes. A tensão deve vir de sinais visuais e sonoros no ambiente, gerenciamento de recargas e reação. Não transforme a observação em uma sequência de alertas explícitos de perigo.

## Regras aprovadas posteriormente ao GDD — prevalecem

1. **Um ataque por vez no MVP.** Lamparina, combustível e simultaneidade ficaram adiados. Não os adicione automaticamente.
2. A partida dura **150 s** e o timer continua correndo durante os desafios. O GameManager controla esse tempo, separadamente do AttackManager.
3. O bebê começa com **100 de vida**, sem regeneração. Durante QTEs hostis perde **5 pontos por segundo**. Esses valores são um ponto inicial de balanceamento e ainda exigem playtest.
4. Boi e fogo usam **12 cliques no botão ou pressionamentos de Espaço em até 8 s**. O dano ocorre durante o QTE mesmo quando o jogador consegue terminá-lo. Ao concluir, o dano para. Ao expirar, o ataque termina mantendo o dano recebido; não há penalidade extra nem derrota automática se ainda houver vida. Isso substitui o texto antigo do GDD que falava em derrota imediata por falha no QTE.
5. Intervalo aleatório de **8–14 s após cada evento**, com pré-ataque de **5 s**. Não deve haver eventos sobrepostos.
6. **Tesoura em cruz:** arrastar do armário à janela durante o pré-ataque do boi cancela completamente a ameaça. Recarga aprovada: **5 s**. Não implemente uma segunda recarga de 10 s do GDD antigo.
7. **Vassoura:** arrastar até a porta durante o aviso revela a bruxa com gemido e a enfraquece. Recarga aprovada: **7 s**. Não implemente uma segunda recarga de 14 s.
8. **Terço:** um único item, duas orações, recarga **compartilhada de 30 s**. Arrastar ao berço faz Ave-Maria: proteção de **15 s**. Arrastar à chaminé faz São Miguel: cancela o pré-ataque de fogo. O uso atual é imediato por soltar no alvo; uma mecânica especial de oração fica para depois.
9. A proteção deve estar ativa **antes** do ataque. Se o bebê estiver protegido quando a ameaça avança, o ataque termina sem QTE. O bloqueio de dano pertence ao BabyController. Não é possível pegar itens ou abrir o armário durante um QTE; itens já carregados são devolvidos quando o desafio começa.
10. O armário abre por clique. O jogador segura e arrasta **um item por vez**, solta no local de uso e o item retorna ao armário. Soltar fora de um alvo aceito não consome recarga. Soltar no alvo aceito consome a recarga mesmo sem ameaça presente.
11. **Porta:** fica fechada. Mãe e bruxa podem anunciar a mesma frase, aproximadamente “Tudo bem aí dentro? Vou entrar.” A vassoura é a pista para identificar a bruxa. Depois, clicar na porta durante o aviso inicia resistência: segurar o botão por **4 s dentro de 7 s**, ou **2,5 s** se a bruxa estiver enfraquecida. A implementação acumula tempo segurado; soltar pausa esse acúmulo e o prazo continua. A bruxa causa dano gradual durante a resistência; a mãe não causa dano.
12. Falhar na resistência à bruxa ou deixá-la entrar ao ignorar o aviso causa invasão e derrota. **Não há um segundo QTE após essa resistência.** A mãe entra brevemente e sai se ignorada; impedi-la também não causa dano. A chance atual de ser bruxa é **0,6**. A proteção ativa também bloqueia o avanço da porta.

## Arquitetura já implementada

- `GameManager` / `GameState`: Waiting, Playing, Victory, Defeat; timer e fim da partida.
- `BabyController`: vida, dano, proteção temporária e reset.
- `AttackManager`: agenda e sorteia eventos, impede sobreposição, limpa no fim da partida.
- `AttackEvent`: ciclo Idle → PreAttack → Active → limpeza. `BullAttack`, `FireAttack` e `DoorAttack` implementam comportamentos próprios.
- `BullVisual`: grupos normal/estranho dos bois.
- `QTEManager`: desafios Mash/Hold, progresso, prazo, callbacks de resultado e de tempo ativo para dano.
- `QTEView` / `QTEButtonInput`: painel bloqueante, progresso, botão e vida do bebê visível durante o desafio.
- `ItemManager`: disponibilidade, recargas centrais e um único item carregado. Enum Scissors/Rosary/Broom.
- `DraggableItem` / `ItemDropTarget`: arrasto e áreas de uso com eventos configurados na cena.
- `WardrobeController`, `ItemCooldownView`, `RosaryPrayers`, `DoorInteraction`: interações específicas.
- `UIManager`: HUD, início, vitória, derrota e nova partida.
- `PrototypeAudio`: tons procedurais temporários para batida/gemido/resultado. **Não são vozes nem arquivos OGG/MP3 finais.**
- `ScareFeedback`: sobreposição provisória de derrota por um segundo.
- Painel de depuração de vida/proteção visível no Editor/development; oculto em release.

Não concentre tudo em um script gigante. Reutilize vida/proteção, recargas, agenda e QTE. Preserve cleanup/cancelamento no fim de partida e reset sem recarregar a cena.

## Cena e apresentação

A sala atual foi construída com **UI Images e RectTransforms dentro de Canvas**, com câmera ortográfica. Há janela/pasto com quatro bois, armário com tesoura/terço/vassoura, berço/bebê, porta e chaminé. As artes e boa parte dos efeitos são placeholders.

Posso substituir as artes e reorganizar o quarto. Preserve componentes, referências serializadas, arquivos `.meta` e áreas de interação. Para mover um ponto de uso, leve junto seu grupo/área transparente. Não converta tudo para outro sistema de renderização sem necessidade.

Os instaladores em `Editor` têm menus **Game Jam > Preparar...** e marcadores locais em `Library/Vigilia*Setup.txt`. Em uma máquina nova a Library é reconstruída, então alguns instaladores podem rodar. Confira a cena após a primeira importação. Alguns menus Preparar reposicionam armário/interface e alteram textos do protótipo: faça backup/commit antes de usá-los sobre arte final. O menu **Preparar e verificar versão atual** executa a preparação integrada na ordem correta.

## Validação já realizada e seus limites

- **93 verificações passaram**: base 14, agenda/boi 18, tesoura/itens 27, QTE 14, sobrevivência integrada 20.
- Cobrem timer, vida/proteção, cooldown, callbacks, cancelamento, reinício, fogo e regras da porta.
- Foram executadas por ferramentas do Editor, não constituem uma suíte automatizada de navegador nem comprovação de balanceamento.
- O usuário testou manualmente e confirmou que a tesoura **acompanha o mouse e usa na janela**.
- No Play Mode foi observado QTE com vida do bebê diminuindo em tempo real.
- Os relatórios históricos estão em `docs/VERIFICACOES-2026-10-21.txt`; novas execuções escrevem em `Library/Vigilia*Verification.txt`.

## Build Web: conferir antes de afirmar que está pronta

Foi iniciada uma primeira exportação Web em 21/10/2026. **Às 17h30 ela ainda estava em execução**, compilando recursos/shaders, e não havia validação no navegador. A Library e a build são locais e não entram no Git; num outro computador será necessário gerar de novo.

- Menu: **Game Jam > Gerar build Web** (`Editor/WebBuild.cs`).
- O instalador troca o target para WebGL e exporta explicitamente a cena Main_MVP para `Builds/Web`, sem depender da lista antiga que pode conter SampleScene.
- Release, compressão desativada, threads desativadas.
- Relatório: `Library/VigiliaWebBuild.txt`. Só `Succeeded` confirma sucesso. `RUNNING` não confirma conclusão.
- Servir `Builds/Web` via HTTP e validar no navegador: início, mouse/arrasto, QTE, áudio após interação, resultado e reinício.
- Para itch.io, preparar ZIP com `index.html`, `Build` e `TemplateData` na raiz. Não publicar sem uma solicitação minha.

## Próximos passos recomendados

1. Abrir o projeto existente, aguardar a importação e verificar Console/cena antes de mudar código.
2. Confirmar o estado da build disponível ou gerar uma nova; testar o fluxo no navegador.
3. Fazer playtest de uma partida e ajustar dificuldade com base em observação. Os números atuais são configuráveis no Inspector.
4. Substituir arte e sons provisórios pelos assets da equipe. A solicitação artística original pede fontes PSD, efeitos OGG e músicas MP3; **esses assets finais ainda não foram produzidos**.
5. Polir feedback, animações, atmosfera e clareza dos sinais. Preservar os três eventos funcionais antes de ampliar escopo.
6. Avaliar futuras mecânicas de oração, variação de QTE, progressão de dificuldade e, somente depois, lamparina/simultaneidade se houver tempo e autorização.

Leia também `README.md`, `docs/GUIA-PROTOTIPO.md` e os scripts. Os documentos originais são históricos e contêm valores/ideias substituídos pelas decisões acima. Se identificar um conflito real ou bug, explique e faça a correção mínima necessária; não me faça repetir perguntas já resolvidas neste documento. Ao terminar uma alteração, diga o que mudou, como verificou e o que ainda precisa ser testado. Use Git para preservar versões do trabalho.
