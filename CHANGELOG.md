# Histórico

## 0.3.0

- Novo hatch de quatro portas inspirado no Celta: capô e teto curvos, para-brisa e vigia inclinados, caixas de roda abertas, pneus perfilados, aros, frisos, maçanetas, retrovisores, limpadores, antena e emblema estilizado.
- Faróis trapezoidais, lanternas traseiras integradas, terceira luz de freio e luzes de ré ligadas à transmissão.
- Pintura metálica, reflexos de ambiente, céu procedural, sombras suaves e antialiasing 4×.
- Asfalto e gramado texturizados, vegetação irregular, casas com telhados, passeios, bancos, postes e colinas.
- Placas PARE e 30 redesenhadas; texto 3D respeita profundidade e não aparece através do carro.
- Malhas estáticas do veículo combinadas por material; mantidos os colliders e parâmetros da direção.
- Acrescentadas seis verificações estruturais dos gráficos e capturas frontal/traseira diretamente do executável.

## 0.2.0

- Corrigida a aplicação de faltas antes de dirigir: a velocidade vertical da acomodação da suspensão não conta como deslocamento na via.
- PARE exige uma parada contínua de 2 s; pausas interrompidas não se somam. Uma parada válida permanece registrada enquanto o carro retoma a marcha.
- A conclusão da rampa exige permanecer na faixa da rampa, inclusive no topo.
- A conclusão interrompe a simulação. Reiniciar ou trocar de exercício retoma a física e limpa os comandos anteriores.
- Reiniciar também limpa faróis, torque, freio e esterço das rodas.
- Dicas de finalização deixam de orientar a dar nova partida ou soltar o freio de mão.
- Aumentados fontes e contraste do painel; dica de próxima ação movida para liberar a visão do carro; corrigido corte do cronômetro.
- Testes físicos passam pelo controlador usado pelo jogador, incluindo arrancada, frenagem, ré, direção e saída real em rampa. Acrescentadas verificações de regressão.

## 0.1.0

- Primeira versão Unity com carro manual, circuito, baliza, rampa, espelhos, sinalização e relatório didático.
