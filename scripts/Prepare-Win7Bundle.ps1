param([string]$MSBuildPath)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$bundle = Join-Path $root 'out\win7-x86'
$prerequisites = Join-Path $bundle 'prerequisites'
New-Item -ItemType Directory -Force $prerequisites | Out-Null

# Microsoft Update Catalog: cd7a382c-62c6-46c0-8098-0d50a7953b61.
$url = 'https://catalog.s.download.windowsupdate.com/c/msdownload/update/software/updt/2023/09/microsoftedgestandaloneinstallerx86_179f59bc54d73843d9288a9fd5609de0e507b911.exe'
$expectedHash = '60B22C71128F10ABC5E92DD8FED90FF6CA55B8382BA01E48D87FAF1664746A31'
$installer = Join-Path $prerequisites 'MicrosoftEdgeStandaloneInstallerX86.exe'
if (-not (Test-Path -LiteralPath $installer)) {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $installer
}
if ((Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash -ne $expectedHash) {
    throw 'WebView2 installer hash mismatch. Replace the file with the official Catalog download.'
}
$signature = Get-AuthenticodeSignature -LiteralPath $installer
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation') {
    throw 'WebView2 installer does not have a valid Microsoft signature.'
}

if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
}
if (-not $MSBuildPath -or -not (Test-Path -LiteralPath $MSBuildPath)) {
    throw 'Visual Studio MSBuild is required. Pass -MSBuildPath with its full path.'
}

# A new directory for each build prevents old deployment manifests being included.
$publishDir = Join-Path $bundle ('ClickOnce-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$project = Join-Path $root 'kido_teacher_app\kido_teacher_app.csproj'
$profile = Join-Path $PSScriptRoot 'Win7Offline.pubxml'
& $MSBuildPath $project /restore /t:Publish /p:Configuration=Release /p:Platform=x86 /p:PublishProfile=Win7Offline "/p:PublishProfileFullPath=$profile" "/p:PublishDir=$publishDir\" /v:minimal
if ($LASTEXITCODE -ne 0) { throw "ClickOnce publish failed: $LASTEXITCODE" }
if (-not (Test-Path -LiteralPath (Join-Path $publishDir 'setup.exe'))) { throw 'Missing setup.exe' }
& (Join-Path $PSScriptRoot 'Test-Win7Payload.ps1') -PublishDirectory $publishDir

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Install-WebView2-Win7.cmd') -Destination $prerequisites
Copy-Item -LiteralPath (Join-Path $root 'docs\WIN7-X86.md') -Destination (Join-Path $bundle 'README.md')
@{
    RuntimeVersion = '109.0.1518.140'
    Architecture = 'x86'
    Source = $url
    SHA256 = $expectedHash
    SignatureStatus = [string]$signature.Status
    ClickOnceDirectory = $publishDir
    BuiltAt = (Get-Date).ToString('o')
    Win7Tested = $false
} | ConvertTo-Json | Set-Content -Encoding UTF8 (Join-Path $bundle 'build-info.json')
$zip = Join-Path $root 'out\IchiTeacher-Win7-x86-test.zip'
Compress-Archive -Path $publishDir, $prerequisites, (Join-Path $bundle 'README.md'), (Join-Path $bundle 'build-info.json') -DestinationPath $zip -Force
Write-Host "Prepared: $bundle"
Write-Host "ZIP: $zip"
Write-Host 'Win7 installation/runtime testing is still required. No upload was performed.'
