param(
    [string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor',
    [string]$OutputDirectory = ''
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $taskRoot 'Temp~/Compile'
}
$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$outputRoot = (Resolve-Path -LiteralPath $OutputDirectory).Path
$dataRoot = Join-Path $UnityEditor 'Data'
$runtime = Join-Path $dataRoot 'NetCoreRuntime/dotnet.exe'
$compiler = Join-Path $dataRoot 'DotNetSdkRoslyn/csc.dll'
$referenceRoot = Join-Path $dataRoot 'NetStandard/ref/2.1.0'
foreach ($required in @($runtime, $compiler, $referenceRoot)) {
    if (-not (Test-Path -LiteralPath $required)) { throw "Required Unity compiler/reference path missing: $required" }
}
$response = [System.Collections.Generic.List[string]]::new()
$response.Add('-nologo')
$response.Add('-target:library')
$response.Add('-langversion:9.0')
$response.Add('-nostdlib+')
$response.Add('-warn:4')
$response.Add('-warnaserror+')
$response.Add('-define:UNITY_EDITOR,UNITY_EDITOR_WIN,UNITY_2022_3,UNITY_2022_3_OR_NEWER')
$response.Add('-out:"' + (Join-Path $outputRoot 'dennokoworks.MatSync.Editor.dll') + '"')
$references = @(Get-ChildItem -LiteralPath $referenceRoot -Filter '*.dll')
$references += @(Get-ChildItem -LiteralPath (Join-Path $dataRoot 'Managed/UnityEngine') -Filter '*.dll')
foreach ($reference in $references) { $response.Add('-r:"' + $reference.FullName + '"') }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $taskRoot 'Editor') -Recurse -Filter '*.cs')
foreach ($source in $sources) { $response.Add('"' + $source.FullName + '"') }
$responsePath = Join-Path $outputRoot 'compile.rsp'
[System.IO.File]::WriteAllLines($responsePath, $response, [System.Text.UTF8Encoding]::new($false))
$logPath = Join-Path $outputRoot 'compile.log'
$compilerOutput = @(& $runtime $compiler ('@' + $responsePath) 2>&1)
$compilerExit = $LASTEXITCODE
$log = @("Unity Editor: $UnityEditor", "Sources: $($sources.Count)", "References: $($references.Count)") + $compilerOutput + @("Exit code: $compilerExit")
[System.IO.File]::WriteAllLines($logPath, [string[]]$log, [System.Text.UTF8Encoding]::new($false))
$log | Write-Output
exit $compilerExit
