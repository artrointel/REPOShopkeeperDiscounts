[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9_-]+$')]
    [string]$ProfileName = 'ShopkeeperDiscounts-Test',
    [string]$CoreSource,
    [switch]$NoRestore
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$profilesRoot = Join-Path $env:APPDATA 'Thunderstore Mod Manager/DataFolder/REPO/profiles'
if ([string]::IsNullOrWhiteSpace($CoreSource)) { $CoreSource = Join-Path $profilesRoot 'REPO/BepInEx/core' }
if (-not (Test-Path -LiteralPath (Join-Path $CoreSource 'BepInEx.Preloader.dll'))) { throw 'Set -CoreSource to an existing BepInEx 5 core directory.' }
$profile = Join-Path $profilesRoot $ProfileName
$bep = Join-Path $profile 'BepInEx'
$plugins = Join-Path $bep 'plugins'
New-Item -ItemType Directory -Path $plugins -Force | Out-Null
if (-not (Test-Path -LiteralPath (Join-Path $bep 'core/BepInEx.Preloader.dll'))) { Copy-Item -LiteralPath $CoreSource -Destination $bep -Recurse }
$cache = Join-Path $projectRoot 'artifacts/dependencies'
New-Item -ItemType Directory -Path $cache -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($package in @(@{ Name='REPOConfig'; Version='1.2.7' }, @{ Name='MenuLib'; Version='2.6.0' })) {
    $zip = Join-Path $cache "$($package.Name)-$($package.Version).zip"
    if (-not (Test-Path -LiteralPath $zip)) {
        Invoke-WebRequest -UseBasicParsing "https://thunderstore.io/package/download/REPOModding/$($package.Name)/$($package.Version)/" -OutFile $zip
    }
    $directory = Join-Path $plugins "REPOModding-$($package.Name)"
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $archive = [IO.Compression.ZipFile]::OpenRead($zip)
    try {
        $entry = $archive.GetEntry("$($package.Name).dll")
        if ($null -eq $entry) { throw "Expected library missing from package: $($package.Name)" }
        # Extract only the exact known DLL; do not trust arbitrary archive paths.
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $directory "$($package.Name).dll"), $true)
    } finally { $archive.Dispose() }
}
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.nuget/packages'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$argsBuild = @('build', (Join-Path $projectRoot 'src/ShopkeeperDiscounts/ShopkeeperDiscounts.csproj'), '-c', 'Debug', '-p:CopyFilesToPluginOutputDirectoryOnBuild=false')
if ($NoRestore) { $argsBuild += '--no-restore' }
& (Join-Path $projectRoot '.dotnet/dotnet.exe') @argsBuild
if ($LASTEXITCODE -ne 0) { throw 'Debug build failed.' }
# The former plugin GUID is different, so BepInEx would load both versions.
# Move legacy files outside BepInEx before deploying the renamed plugin.
$legacyFiles = @(Get-ChildItem -LiteralPath $plugins -Recurse -File -Filter 'ShopDiscountBot.dll')
$legacyConfig = Join-Path $bep 'config/artrointel.ShopDiscountBot.cfg'
if (Test-Path -LiteralPath $legacyConfig) { $legacyFiles += Get-Item -LiteralPath $legacyConfig }
if ($legacyFiles.Count -gt 0) {
    if (Get-Process -Name REPO -ErrorAction SilentlyContinue) {
        throw 'Close R.E.P.O. before removing the legacy ShopDiscountBot plugin.'
    }
    $backupRoot = Join-Path $projectRoot ("artifacts/legacy-profile-backups/{0}/{1}" -f $ProfileName, [Guid]::NewGuid().ToString('N'))
    $bepPrefix = [IO.Path]::GetFullPath($bep).TrimEnd('\') + '\'
    foreach ($legacyFile in $legacyFiles) {
        $legacyPath = [IO.Path]::GetFullPath($legacyFile.FullName)
        if (-not $legacyPath.StartsWith($bepPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Legacy file is outside the selected profile: $legacyPath"
        }
        $backupPath = Join-Path $backupRoot $legacyPath.Substring($bepPrefix.Length)
        New-Item -ItemType Directory -Path (Split-Path -Parent $backupPath) -Force | Out-Null
        Move-Item -LiteralPath $legacyPath -Destination $backupPath
    }
    Write-Host "Legacy ShopDiscountBot files backed up to: $backupRoot"
}
$destination = Join-Path $plugins 'ShopkeeperDiscounts'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'src/ShopkeeperDiscounts/bin/Debug/netstandard2.1/ShopkeeperDiscounts.dll') -Destination $destination -Force
Write-Host "Test profile ready: $profile"
