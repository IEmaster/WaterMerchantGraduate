param([Parameter(Mandatory=$true)][string]$GamePath)
$ErrorActionPreference='Stop'
$g=Join-Path $GamePath 'Probably Stolen_Data/Managed'
foreach($name in 'DseCecil','UnityEngine.CoreModule','Assembly-CSharp'){[void][Reflection.Assembly]::LoadFrom("$g\$name.dll")}
$root=(Resolve-Path package).Path
$dlls=@(Get-ChildItem -LiteralPath $root -Filter '*.dll' -Recurse -File | ForEach-Object FullName)
$refs=@(Get-ChildItem -LiteralPath $g -Filter '*.dll' -File | ForEach-Object BaseName)+@('WaterMerchantGraduate')
$r=[ModScanner]::Scan([string[]]$dlls,[string[]]$refs,$root,$true)
[xml]$manifest=Get-Content -LiteralPath (Join-Path $root manifest.xml) -Raw
$declared=@($manifest.ModManifest.Capabilities.Capability | ForEach-Object { [string]$_ })
$missing=@($r.Capabilities | Where-Object { [string]$_ -notin $declared })
$r|Format-List|Out-String|Set-Content -Encoding utf8 artifacts/native-scan-detail.txt
if($r.Refused -or $r.Error -or $missing.Count){throw ('Scan refused or undeclared permissions: '+($missing -join ',')+' '+$r.Error)}
Write-Output ('Native scanner passed; capabilities='+($r.Capabilities -join ',')+'; hardFlags='+($r.HardFlags -join ','))
