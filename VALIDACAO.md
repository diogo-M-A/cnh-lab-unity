# Validação técnica

Ambiente: Unity 6000.6.6f1, Windows x64, backend Mono, física PhysX. Renderização de prévia com Direct3D 11 em Intel UHD Graphics.

- **Compilação Windows:** concluída; executável e arquivos de distribuição em `Build/Windows`.
- **Transmissão:** 12 verificações de troca com/sem embreagem, partida, inversão de sentido, marcha inválida, motor sob carga e ausência de tração em neutro.
- **Física no executável:** 6 verificações de ausência de faltas no nascimento, arrancada com liberação gradual da embreagem, frenagem até menos de 0,5 km/h, marcha à ré, esterço e retomada numa rampa física de 8 graus. Os ensaios usam o `FixedUpdate` normal do `CarController`; não aplicam torque diretamente às rodas pelo código do teste. A aceleração do ensaio ultrapassa deliberadamente o limite do exercício para verificar a tração, sem representar uma condução recomendada.
- **Regras dos exercícios:** 16 verificações de PARE com/sem parada, pausas interrompidas, velocidade vertical, conversão com/sem seta e observação, vaga desalinhada/alinhada, parada/saída/recuo na rampa, conclusão fora da faixa, finalização urbana, pausa e limpeza do estado ao reiniciar. As verificações de regras posicionam estados diretamente; não representam uma pessoa completando todo o percurso pelo teclado.
- **Visual:** cenário e vistas frontal/traseira renderizados pelo executável em 1440×900. Conferidos carro, rodas, lanternas, oclusão de textos, placas e ambiente. Seis verificações estruturais cobrem colliders, rodas combinadas, vértices finitos, shaders/reflexos, qualidade gráfica e luzes. As capturas não incluem a interface sobreposta.
- **Interface no executável:** conferidos painel e retrovisores, ausência de faltas antes de sair, B alterando o cinto, C alternando as três câmeras, Esc abrindo a pausa e F2 retomando a física e limpando o treino.

**Resultado da versão 0.3.0 em 23/10/2026: PASS nas 40 verificações** (12 de transmissão + 6 físicas + 16 de regras + 6 de gráficos). O executável passou tanto com Direct3D 11 quanto sem renderização. Pico do ensaio de aceleração: 44,39 km/h.

Resultados locais detalhados: `Build/transmission-tests.txt`, `Build/runtime-smoke.txt`, `Build/runtime-v030-graphics.log`, `Build/runtime-v030-headless.log` e `Build/unity-v030.log`. Os resumos publicáveis ficam em `Docs/test-results-v0.3.0-graphics.txt` e `Docs/test-results-v0.3.0-headless.txt`. Para repetir o teste da física e das regras, execute `Testar.bat`.

Não foi feita validação com instrutor de trânsito, comparação com a dinâmica de um Celta real, teste com volante/pedais ou certificação de equivalência ao exame. O jogo usa critérios didáticos próprios e teclado com pedais graduais.
