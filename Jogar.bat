@echo off
cd /d "%~dp0"
if exist "Build\Windows\CNH Lab.exe" (
  start "" "Build\Windows\CNH Lab.exe"
) else (
  echo O executavel ainda nao foi gerado. Abra o projeto no Unity e use CNH LAB - Gerar jogo Windows.
  pause
)
