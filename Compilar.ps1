$ErrorActionPreference = 'Stop'
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $unityEditor)) { throw 'Instale Unity 6000.6.6f1 com Windows Build Support (Mono), ou ajuste o caminho deste script.' }
$buildLog = Join-Path $PSScriptRoot 'Build\unity-build.log'
New-Item -ItemType Directory -Force -Path (Join-Path $PSScriptRoot 'Build') | Out-Null
$unityArgs = '-batchmode -nographics -quit -projectPath "{0}" -executeMethod ProjectBuilder.Build -logFile "{1}"' -f $PSScriptRoot, $buildLog
$buildProcess = Start-Process -FilePath $unityEditor -ArgumentList $unityArgs -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Falha de compilação. Consulte $buildLog" }
Write-Host 'Jogo gerado em Build\Windows\CNH Lab.exe'
