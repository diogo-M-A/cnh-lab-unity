@echo off
cd /d "%~dp0"
if not exist "Build\Windows\CNH Lab.exe" (
  echo Primeiro compile o jogo usando Compilar.ps1 ou o menu CNH LAB do Unity.
  pause
  exit /b 1
)
echo Testando arrancada, frenagem, re, direcao, rampa e regras dos exercicios...
start /wait "" "Build\Windows\CNH Lab.exe" -batchmode -nographics -smokeTest -logFile "%~dp0Build\runtime.log"
set "TEST_EXIT=%ERRORLEVEL%"
type "Build\runtime-smoke.txt"
if not "%TEST_EXIT%"=="0" echo FALHA: consulte Build\runtime.log. Codigo de saida %TEST_EXIT%.
pause
exit /b %TEST_EXIT%
