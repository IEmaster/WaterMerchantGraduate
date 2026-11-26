param(
 [Parameter(Mandatory=$true)][string]$GamePath,
 [string]$CompilerPath,
 [switch]$Tests
)
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$managed=Join-Path $GamePath 'Probably Stolen_Data/Managed'
if(!(Test-Path -LiteralPath (Join-Path $managed 'Assembly-CSharp.dll'))){throw 'GamePath must point to the Probably Stolen installation.'}
if(!$CompilerPath){
 $command=Get-Command csc.exe -ErrorAction SilentlyContinue
 if($command){$CompilerPath=$command.Source}
 else {
  $vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
  if(Test-Path -LiteralPath $vswhere){$CompilerPath=(& $vswhere -latest -products '*' -find 'MSBuild/Current/Bin/Roslyn/csc.exe' | Select-Object -First 1)}
 }
}
if(!$CompilerPath -or !(Test-Path -LiteralPath $CompilerPath)){throw 'Install Visual Studio Build Tools (C# compiler), or supply -CompilerPath.'}
Push-Location $root
$previousGamePath=$env:PROBABLY_STOLEN_PATH
try {
 $env:PROBABLY_STOLEN_PATH=$GamePath
 New-Item -ItemType Directory -Path artifacts,dist -Force | Out-Null
 $references=@('Assembly-CSharp','Assembly-CSharp-firstpass','UnityEngine.CoreModule','Unity.Localization','0Harmony','netstandard') | ForEach-Object { '/r:'+(Join-Path $managed ($_+'.dll')) }
 $sources=@(Get-ChildItem src -Filter '*.cs' -File | ForEach-Object FullName)
 & $CompilerPath /nologo /target:library /optimize+ /out:package/WaterMerchantGraduate.dll @references @sources
 if($LASTEXITCODE){throw 'Compilation failed'}
 & ./tools/scan.ps1 -GamePath $GamePath > artifacts/native-scan.txt
 if($Tests){
  & $CompilerPath /nologo /target:exe /define:NATIVE_BOUNDARY /out:artifacts/water-tests.exe src/WaterMerchantGraduate.cs src/WaterMerchantGraduateLocalization.cs tests/TestWaterMerchantGraduate.cs
  if($LASTEXITCODE){throw 'Test compilation failed'}
  & ./artifacts/water-tests.exe > artifacts/water-tests.txt
  if($LASTEXITCODE){throw 'Water tests failed'}
  Get-Content artifacts/water-tests.txt -Tail 1
 }
 [xml]$manifest=Get-Content package/manifest.xml -Raw
 $zip=Join-Path $root ('dist/WaterMerchantGraduate-'+$manifest.ModManifest.ModVersion+'.zip')
 Compress-Archive -Path package/* -DestinationPath $zip -Force
 Write-Output ('Built '+$zip)
} finally {
 $env:PROBABLY_STOLEN_PATH=$previousGamePath
 Pop-Location
}
