param([Parameter(Mandatory = $true)][string]$PublishDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath (Join-Path $PublishDirectory 'setup.exe'))) { throw 'Missing setup.exe' }
$manifestFile = Get-ChildItem -LiteralPath $PublishDirectory -Recurse -Filter '*.exe.manifest' | Select-Object -First 1
if (-not $manifestFile) { throw 'Missing ClickOnce application manifest' }
[xml]$manifest = Get-Content -LiteralPath $manifestFile.FullName -Raw
$identity = $manifest.SelectSingleNode('/*[local-name()="assembly"]/*[local-name()="assemblyIdentity"]')
if ($identity.processorArchitecture -ne 'x86') { throw 'Application manifest is not x86' }

$nativeFiles = @('e_sqlite3.dll', 'runtimes\win-x86\native\WebView2Loader.dll')
foreach ($relative in $nativeFiles) {
    $entry = $manifest.SelectSingleNode('//*[local-name()="file" and @name="' + $relative + '"]')
    if (-not $entry) { throw "Native DLL missing from ClickOnce manifest: $relative" }
    $file = Join-Path $manifestFile.DirectoryName ($relative + '.deploy')
    $bytes = [IO.File]::ReadAllBytes($file)
    $peOffset = [BitConverter]::ToInt32($bytes, 0x3c)
    if ([BitConverter]::ToUInt16($bytes, $peOffset + 4) -ne 0x14c) { throw "Native DLL is not x86: $relative" }
    $digest = $entry.SelectSingleNode('.//*[local-name()="DigestValue"]').InnerText
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $actual = [Convert]::ToBase64String($sha.ComputeHash($bytes)) } finally { $sha.Dispose() }
    if ($actual -ne $digest) { throw "Manifest hash mismatch: $relative" }
}
[xml]$config = Get-Content -LiteralPath (Join-Path $manifestFile.DirectoryName 'Ichi Teacher.exe.config.deploy') -Raw
if ($config.configuration.startup.supportedRuntime.sku -ne '.NETFramework,Version=v4.8') { throw 'Runtime config is not net48' }
$sdk = $manifest.SelectSingleNode('//*[local-name()="assemblyIdentity" and @name="Microsoft.Web.WebView2.Core"]')
if ($sdk.version -ne '1.0.1518.46') { throw 'Unexpected WebView2 SDK version' }
Write-Host 'PASS: setup.exe, net48/x86 manifest, SDK 1.0.1518.46, SQLite/WebView2 native x86 files and hashes.'
