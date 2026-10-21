# A Vigília

Protótipo 2D de uma Game Jam, inspirado no imaginário de bruxas da Ilha de Santa Catarina e nas histórias de Franklin Cascaes. O jogador protege um bebê durante 150 segundos, observando sinais no quarto e administrando itens defensivos.

## Abrir em outro computador

1. Instale o Git e o Unity Hub.
2. Entre no GitHub com a conta que tem acesso a este repositório privado.
3. Clone o projeto: `git clone https://github.com/pedrogabriel08/a-vigilia.git`.
4. No Unity Hub, instale **Unity 6000.6.7f1**, com **Web Build Support** para exportar para navegador.
5. Adicione a pasta clonada ao Hub e abra. Aguarde a reconstrução da Library e a importação dos pacotes.
6. Abra `Assets/_Game/Scenes/Main_MVP.unity`, pressione Play e clique em **INICIAR VIGÍLIA**.

Abra a raiz que contém `Assets`, `Packages` e `ProjectSettings`. Não use o SampleScene como cena principal. A Library não precisa ser transferida. Preserve os arquivos `.meta` para manter as referências.

## Estado atual

- Timer de 150 s, vida do bebê, proteção, vitória, derrota e reinício.
- Ataques sequenciais de boi, fogo e porta; intervalo aleatório de 8–14 s.
- Armário, arrasto de um item por vez, tesoura, terço e vassoura.
- QTE de cliques/Espaço e de segurar, com dano gradual e vida visível.
- Interface, cenário e sons ainda provisórios.
- 93 verificações de lógica passaram na sessão de 21/10/2026. Isso não substitui testar a jogabilidade.
- A exportação Web concluiu após as 17h30 com resultado `Succeeded`, tamanho de 54.974.604 bytes e `Errors=1` no resumo. Esse erro ainda precisa ser conferido no Console/relatório da Unity; a build não foi validada no navegador nem publicada na itch.io. [Resumo local da exportação](docs/BUILD-WEB-2026-10-21.txt).

## Documentação e continuidade

- [Mensagem para a próxima IA](docs/CONTINUAR-COM-OUTRA-IA.md): contexto técnico, decisões aprovadas e próximos passos.
- [Guia de controles](docs/GUIA-PROTOTIPO.md): controles e configuração no Inspector.
- [Verificações da sessão](docs/VERIFICACOES-2026-10-21.txt).
- [Contexto original](docs/CONTEXTO-ORIGINAL.md) e [GDD original](docs/GDD-ORIGINAL.md): referências históricas. As decisões posteriores descritas na mensagem de continuidade prevalecem.

## Verificação e build

O menu **Game Jam > Preparar e verificar versão atual** configura o protótipo e executa as verificações. Os instaladores **Preparar** podem reposicionar elementos da interface; evite usá-los sobre um layout final sem uma cópia da cena. Os menus **Verificar** executam as respectivas verificações.

**Game Jam > Gerar build Web** exporta para `Builds/Web`. O relatório local é `Library/VigiliaWebBuild.txt`: somente `Succeeded` confirma a exportação. Sirva a pasta por HTTP para testar. Para itch.io, empacote `index.html`, `Build` e `TemplateData` na raiz do ZIP. A publicação é uma etapa posterior.

## Rotina com Git

Antes de continuar em outro computador, faça `git pull`. Ao terminar, salve a cena e os assets no Unity, confira `git status`, faça commit e `git push`. Evite editar a mesma cena simultaneamente em duas máquinas. Cache, IDE e builds são ignorados; fontes, configurações e `.meta` são versionados.
