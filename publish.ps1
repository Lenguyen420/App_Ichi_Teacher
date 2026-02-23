param(
    [string]$ConfigPath = "publish.config.json",
    [ValidateSet("major", "minor", "patch", "revision")]
    [string]$Bump = "patch",
    [switch]$NoBump,
    [switch]$SkipUpload
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FromRoot {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return $Path }
    $root = $PSScriptRoot
    if ([System.IO.Path]::IsPathRooted($Path)) { return $Path }
    return Join-Path $root $Path
}

function Get-ConfigValue {
    param([object]$Config, [string]$Name)
    if ($null -eq $Config) { return $null }
    if ($Config.PSObject.Properties.Name -contains $Name) {
        return $Config.$Name
    }
    return $null
}

function Normalize-Version {
    param([Version]$Version)
    $major = $Version.Major
    $minor = if ($Version.Minor -lt 0) { 0 } else { $Version.Minor }
    $build = if ($Version.Build -lt 0) { 0 } else { $Version.Build }
    $rev = if ($Version.Revision -lt 0) { 0 } else { $Version.Revision }
    return [Version]::new($major, $minor, $build, $rev)
}

function Bump-Version {
    param([Version]$Version, [string]$Part)
    $v = Normalize-Version $Version
    switch ($Part) {
        "major" { return [Version]::new($v.Major + 1, 0, 0, 0) }
        "minor" { return [Version]::new($v.Major, $v.Minor + 1, 0, 0) }
        "patch" { return [Version]::new($v.Major, $v.Minor, $v.Build + 1, 0) }
        "revision" { return [Version]::new($v.Major, $v.Minor, $v.Build, ($v.Revision + 1)) }
    }
}

$configFullPath = Resolve-FromRoot $ConfigPath
if (-not (Test-Path $configFullPath)) {
    throw "Missing config file: $configFullPath"
}

$config = Get-Content $configFullPath -Raw | ConvertFrom-Json

$projectPath = Resolve-FromRoot (Get-ConfigValue $config "Project")
$publishDir = Resolve-FromRoot (Get-ConfigValue $config "PublishDir")
$versionFile = Resolve-FromRoot (Get-ConfigValue $config "VersionFile")
$configuration = (Get-ConfigValue $config "Configuration")
if (-not $configuration) { $configuration = "Release" }
$publishProfile = (Get-ConfigValue $config "PublishProfile")
if (-not $publishProfile) { $publishProfile = "" }

if (-not (Test-Path $projectPath)) {
    throw "Project not found: $projectPath"
}

if (-not (Test-Path $versionFile)) {
    throw "Version file not found: $versionFile"
}

$versionJson = Get-Content $versionFile -Raw | ConvertFrom-Json
if (-not $versionJson.latestVersion) {
    throw "version.json missing latestVersion"
}

$currentVersion = [Version]$versionJson.latestVersion
if ($NoBump) {
    $newVersion = Normalize-Version $currentVersion
}
else {
    $newVersion = Bump-Version $currentVersion $Bump
}
$newVersionDisplay = "{0}.{1}.{2}" -f $newVersion.Major, $newVersion.Minor, $newVersion.Build
$assemblyVersion = "$newVersionDisplay.0"
$publishVersion = $assemblyVersion

$packagePrefix = (Get-ConfigValue $config "PackagePrefix")
if (-not $packagePrefix) { $packagePrefix = "ichiteacher" }
$downloadBase = if ($config.DownloadBaseUrl) { $config.DownloadBaseUrl.TrimEnd("/") } else { "" }
$clickOnceAppFile = (Get-ConfigValue $config "ClickOnceApplicationFile")
if (-not $clickOnceAppFile) { $clickOnceAppFile = "" }
$clickOnceAppFileEncoded = if ($clickOnceAppFile) { [System.Uri]::EscapeDataString($clickOnceAppFile) } else { "" }

if (-not $NoBump) {
    $versionJson.latestVersion = $newVersionDisplay
}
if ($downloadBase) {
    if ($clickOnceAppFileEncoded) {
        $versionJson.downloadUrl = "$downloadBase/$clickOnceAppFileEncoded"
    }
    else {
        $versionJson.downloadUrl = "$downloadBase/$zipName"
    }
}

if (-not $NoBump) {
    $versionJson | ConvertTo-Json -Depth 10 | Set-Content -Path $versionFile -Encoding UTF8
}

if (-not (Test-Path $publishDir)) {
    New-Item -ItemType Directory -Path $publishDir | Out-Null
}

Write-Host "Publishing version $newVersionDisplay ..."
if ($publishProfile) {
    dotnet publish $projectPath -c $configuration `
        /p:PublishProfile=$publishProfile `
        /p:ApplicationVersion=$assemblyVersion `
        /p:PublishVersion=$publishVersion `
        /p:Version=$assemblyVersion `
        /p:AssemblyVersion=$assemblyVersion `
        /p:FileVersion=$assemblyVersion
}
else {
    dotnet publish $projectPath -c $configuration -o $publishDir `
        /p:PublishVersion=$publishVersion `
        /p:Version=$assemblyVersion `
        /p:AssemblyVersion=$assemblyVersion `
        /p:FileVersion=$assemblyVersion
}

$publishVersionPath = Join-Path $publishDir (Split-Path $versionFile -Leaf)
Copy-Item -Path $versionFile -Destination $publishVersionPath -Force

$versionTxtPath = Join-Path $publishDir "version.txt"
Set-Content -Path $versionTxtPath -Value $newVersionDisplay -Encoding UTF8

if ($config.UseUpdateVersionApi) {
    $apiBase = (Get-ConfigValue $config "ApiBaseUrl")
    if ($apiBase) { $apiBase = $apiBase.TrimEnd("/") }
    $endpoint = (Get-ConfigValue $config "UpdateVersionEndpoint")
    if (-not $endpoint) { $endpoint = "/updateversion" }
    $token = (Get-ConfigValue $config "UpdateVersionToken")
    if (-not $token) { $token = "" }

    if (-not $apiBase) {
        throw "ApiBaseUrl is required when UseUpdateVersionApi=true"
    }
    if (-not $token) {
        throw "UpdateVersionToken is required when UseUpdateVersionApi=true"
    }

    $payload = @{
        latestVersion = $newVersionDisplay
        forceUpdate = [bool]$versionJson.forceUpdate
        downloadUrl = $versionJson.downloadUrl
    } | ConvertTo-Json

    $headers = @{
        Authorization = "Bearer $token"
    }

    $updateUrl = "$apiBase$endpoint"
    Write-Host "Updating version via API..."
    Invoke-RestMethod -Method Post -Uri $updateUrl -Headers $headers -ContentType "application/json" -Body $payload | Out-Null
}

if ($SkipUpload) {
    Write-Host "Skip upload enabled."
    exit 0
}

$server = $config.Server
if (-not $server -or -not $server.Host -or -not $server.User -or -not $server.RemoteDir) {
    Write-Host "Server config missing. Skipping upload."
    exit 0
}

$scpExe = (Get-ConfigValue $config "ScpExe")
if (-not $scpExe) { $scpExe = "scp" }
$sshExe = (Get-ConfigValue $config "SshExe")
if (-not $sshExe) { $sshExe = "ssh" }
$uploadPublishDirValue = Get-ConfigValue $config "UploadPublishDir"
$uploadPublishDir = if ($null -ne $uploadPublishDirValue) { [bool]$uploadPublishDirValue } else { $false }
$scpArgs = @()
if ($server.Port) {
    $scpArgs += "-P"
    $scpArgs += "$($server.Port)"
}
$sshBatchMode = $true
$sshBatchModeValue = Get-ConfigValue $config "SshBatchMode"
if ($null -ne $sshBatchModeValue) {
    $sshBatchMode = [bool]$sshBatchModeValue
}
if ($sshBatchMode) {
    $scpArgs += "-o"
    $scpArgs += "BatchMode=yes"
}
$scpArgs += "-o"
$scpArgs += "ConnectTimeout=15"
if ($server.KeyPath) {
    $keyPath = Resolve-FromRoot $server.KeyPath
    $scpArgs += "-i"
    $scpArgs += $keyPath
}

$remote = "$($server.User)@$($server.Host):$($server.RemoteDir)"

if ($uploadPublishDir) {
    $sshArgs = @()
    if ($server.Port) {
        $sshArgs += "-p"
        $sshArgs += "$($server.Port)"
    }
    if ($sshBatchMode) {
        $sshArgs += "-o"
        $sshArgs += "BatchMode=yes"
    }
    $sshArgs += "-o"
    $sshArgs += "ConnectTimeout=15"
    if ($server.KeyPath) {
        $keyPath = Resolve-FromRoot $server.KeyPath
        $sshArgs += "-i"
        $sshArgs += $keyPath
    }

    $sshTarget = "$($server.User)@$($server.Host)"
    $remoteDirEscaped = $server.RemoteDir.Replace("'", "''")
    & $sshExe @sshArgs $sshTarget "mkdir -p '$remoteDirEscaped'"

    Write-Host "Uploading ClickOnce publish directory..."
    $itemsToUpload = Get-ChildItem -Path $publishDir -Force
    foreach ($item in $itemsToUpload) {
        if ($item.PSIsContainer) {
            & $scpExe @scpArgs -r $item.FullName $remote
        }
        else {
            & $scpExe @scpArgs $item.FullName $remote
        }
    }
}
else {
    Write-Host "UploadPublishDir=false. Skipping publish dir upload."
}

Write-Host "Uploading version.json..."
& $scpExe @scpArgs $versionFile $remote

Write-Host "Done."
