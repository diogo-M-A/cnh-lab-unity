# CNH LAB — treino de direção manual no Unity

Jogo de estudo em português para PC, criado no **Unity 6000.6.6f1**, sem assets pagos ou pacotes externos. O hatch procedural tem dimensões aproximadas de um compacto brasileiro; é inspirado no porte do Celta, sem reproduzir um modelo Chevrolet oficial.

## Jogar

Para baixar sem instalar o Unity, abra a seção **Releases** deste repositório, baixe `CNH-Lab-Windows-v0.2.0.zip`, extraia a pasta inteira e execute `CNH Lab.exe`. O código-fonte desta página deve ser aberto ou compilado no Unity; o executável é distribuído separadamente na release.

Abra **`Jogar.bat`** nesta pasta ou `Build/Windows/CNH Lab.exe`. Mantenha a pasta `CNH Lab_Data`, `MonoBleedingEdge` e as DLLs ao lado do executável. A interface contém os comandos e quatro exercícios. F1 esconde a ajuda, C alterna câmera, Esc pausa e F2 reinicia.

1. Pressione **J** para ajustar banco, **M** para ajustar espelhos e **B** para colocar cinto. A porta começa fechada; **O** alterna a porta com o carro parado.
2. Segure **Shift**, pressione **I** para ligar, depois **1** para primeira marcha.
3. Segure **S** (freio), pressione **Espaço** para soltar o freio de mão e observe os lados com **Z / X**.
4. Solte o freio, acelere suavemente com **W** e solte Shift: o pedal retorna gradualmente, permitindo encontrar o ponto da embreagem. Não é necessário apertar várias vezes.
5. Para trocar de marcha, solte W, segure Shift, pressione a marcha desejada e solte Shift. **1–5**, **N / 0** para neutro, **R** para ré. Pare antes de inverter o sentido.
6. Para parar: solte W, pressione S, depois Shift quando estiver devagar para não apagar. Ao estacionar: pare, N, freio de mão e I para desligar.

### Todos os comandos

| Tecla | Ação |
|---|---|
| W / seta cima | Acelerador com subida gradual |
| S / seta baixo | Freio de serviço |
| A D / setas laterais | Direção com retorno progressivo |
| Shift esquerdo ou direito | Embreagem: segure para pressionar, solte para acoplar gradualmente |
| 1–5 / N ou 0 / R | Câmbio manual de 5 marchas, neutro, ré |
| I | Partida / desligamento do motor |
| Espaço | Freio de estacionamento |
| Q / E | Seta esquerda / direita; repita para cancelar |
| H | Pisca-alerta |
| L | Faróis |
| B / J / M / O | Cinto / ajuste do banco / ajuste dos retrovisores / porta |
| Z / X | Registrar observação do lado esquerdo / direito; vira a visão na câmera próxima |
| V | Olhar para trás na câmera próxima |
| C | Câmera externa, próxima do motorista, aérea |
| F1 / F2 / Esc | Ajuda / reiniciar exercício / pausa |

O ajuste de banco e espelhos é uma confirmação de checklist, sem regulagem física fina. O teclado não mede força: o jogo transforma os comandos digitais em pedais graduais. Os retrovisores exibem câmeras reais do cenário.

## Exercícios

- **Percurso urbano:** siga os marcadores turquesa e o minimapa no sentido horário, sempre pela direita. Pare antes da linha PARE por 2 s, respeite o semáforo (14 s vermelho, 16 s verde), observe a faixa e sinalize três conversões à direita. Termine parado no último marcador, em N, freio de mão acionado e motor desligado.
- **Baliza / estacionamento:** comece ao lado da vaga, faça a manobra de ré e alinhe entre os dois carros. O centro do carro deve ficar próximo ao centro da vaga, com até 8° de diferença do alinhamento. Finalize parado, N e freio de mão por 2 s. Não há limite artificial de tentativas.
- **Saída em rampa:** suba, pare entre z=10 e z=22 (meio da rampa), segure 3 s com o freio de mão, faça a saída e alcance o topo. Recuar mais de 50 cm gera ocorrência.
- **Condução livre:** pratique comandos e controle do carro sem objetivo de conclusão.

O modo **guiado** mostra instruções e ocorrências. O modo **avaliação** oculta essas dicas; as ocorrências continuam sendo registradas. Ao concluir, o jogo pausa para mostrar o resultado. Reiniciar ou escolher outro exercício retoma a simulação. O resultado descreve o exercício e pontos didáticos, sem declarar aprovação oficial. O relatório é salvo automaticamente ao concluir e também pelo botão na pausa, em `%USERPROFILE%/AppData/LocalLow/CNH Lab/CNH Lab - Treino Categoria B/ultimo-treino.txt`.

## O que é simulado

Rigidbody com quatro WheelColliders, suspensão, tração dianteira, resistência aerodinâmica, torque transmitido conforme embreagem/marcha, marcha à ré, freios nas quatro rodas, freio de mão nas traseiras, partida condicionada ao câmbio/embreagem, marcha bloqueada sem embreagem, motor que apaga sob carga, áudio sintético que acompanha o giro, lanternas de freio, setas animadas, faróis e dois retrovisores em tempo real.

Há detecção de contato com obstáculos/meios-fios, excesso de velocidade, movimento sem cinto/porta fechada, falta de preparação/observação, freio de mão durante movimento, contramão nos trechos retos principais, saída da pista, falta de sinalização nas conversões, PARE e semáforo. O motor apagar gera orientação, sem pontos automáticos.

## Abrir e desenvolver no Unity

No Unity Hub, escolha **Add / Add project from disk** e selecione esta pasta. Abra `Assets/Scenes/Treino.unity` e pressione Play. Se necessário, use **CNH LAB → Preparar cena e configurações**. O cenário é construído ao iniciar, a partir de `TrainingWorld.cs`; isso permite alterar dimensões, posições e materiais sem arquivos binários de assets.

- `ManualPowertrain.cs`: estados e cálculo da transmissão.
- `CarController.cs`: controles, rodas, sinalização, som e colisões.
- `TrainingWorld.cs`: carro, pista, placas, rampa e iluminação.
- `TrainingSession.cs`: exercícios, orientações, ocorrências e relatório.
- `DrivingCamera.cs` / `DrivingHUD.cs`: câmeras, retrovisores e interface.
- `ProjectBuilder.cs`: configuração, testes e build Windows.
- `RuntimeSmoke.cs`: ensaio automatizado da física no executável.
- `ScenarioChecks.cs`: verificações das regras de parada, conversão, estacionamento e rampa.

Para compilar, instale o módulo Windows Build Support (Mono) e use **CNH LAB → Gerar jogo Windows** ou execute `Compilar.ps1`. Pela linha de comando: `Unity.exe -batchmode -nographics -quit -projectPath "CAMINHO_DO_PROJETO" -executeMethod ProjectBuilder.Build -logFile "CAMINHO_DO_LOG"`. O menu **Testar transmissão** verifica as regras do câmbio. Execute `Testar.bat` para ensaiar arrancada, frenagem e regras dos exercícios. O executável aceita `-batchmode -nographics -smokeTest` e produz `Build/runtime-smoke.txt`. Sem `-nographics`, também renderiza `Build/scene-preview.png` diretamente pela câmera, sem a interface.

## Escopo e referência

Esta é uma primeira versão jogável de treinamento de procedimentos. Não é simulador certificado, não reproduz fielmente um Celta e não garante aprovação. A física é aproximada e não transmite o tato real do pedal, esterço ou percepção de distância. Um pedestre simplificado atravessa a faixa durante o vermelho. Não há trânsito autônomo de veículos, volante/pedais USB, VR, danos mecânicos ou rotas específicas de cada DETRAN. Verifique os requisitos do seu estado e treine em veículo com orientação profissional.

Referência consultada em 20/10/2026: [Manual Brasileiro de Exames de Direção Veicular — SENATRAN](https://www.gov.br/transportes/pt-br/assuntos/noticias/2026/arquivos/manual-brasileiro-de-exames-de-direcao-veicular.pdf/view). A pontuação, os 2 s de parada, os tempos de exercício, a rampa e os critérios geométricos da vaga são parâmetros próprios deste jogo, não transcrição da norma. A baliza é oferecida como treino de habilidade, sem afirmar obrigatoriedade nacional no exame.
