<#
.SYNOPSIS
    Updates the Scoop and winget manifests with a released version and its SHA-256.

.DESCRIPTION
    Called automatically by .github/workflows/release.yml once the release archive
    has been built, so the manifests never have to be edited by hand. Can also be
    run locally against an already published release.

.PARAMETER Version
    The release version, without the leading "v" (for example: 1.2.0).

.PARAMETER Sha256
    SHA-256 of kaniff-<Version>-win-x64.zip. If omitted, ArchivePath must be given.

.PARAMETER ArchivePath
    Path to the win-x64 archive. Its hash is computed when Sha256 is not supplied.

.PARAMETER Owner
    GitHub owner used to build the download URLs. Defaults to "tgspn".

.EXAMPLE
    ./scripts/Update-Manifests.ps1 -Version 1.2.0 -ArchivePath ./kaniff-1.2.0-win-x64.zip

.EXAMPLE
    ./scripts/Update-Manifests.ps1 -Version 1.2.0 -Sha256 A1B2C3...
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+')]
    [string] $Version,

    [Parameter(ParameterSetName = 'Hash', Mandatory)]
    [ValidatePattern('^[0-9a-fA-F]{64}$')]
    [string] $Sha256,

    [Parameter(ParameterSetName = 'Archive', Mandatory)]
    [string] $ArchivePath,

    [string] $Owner = 'tgspn'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($PSCmdlet.ParameterSetName -eq 'Archive') {
    if (-not (Test-Path -LiteralPath $ArchivePath)) {
        throw "Archive not found: $ArchivePath"
    }
    $Sha256 = (Get-FileHash -LiteralPath $ArchivePath -Algorithm SHA256).Hash
    Write-Host "Computed SHA-256 of $ArchivePath"
}

# winget requires uppercase hex; Scoop accepts either but lowercase is conventional.
$upperHash = $Sha256.ToUpperInvariant()
$lowerHash = $Sha256.ToLowerInvariant()

$repoRoot = Split-Path -Parent $PSScriptRoot
$packaging = Join-Path $repoRoot 'packaging'
$assetUrl = "https://github.com/$Owner/kaniff/releases/download/v$Version/kaniff-$Version-win-x64.zip"

Write-Host "Version : $Version"
Write-Host "SHA-256 : $upperHash"
Write-Host "Asset   : $assetUrl"
Write-Host ''

function Update-ManifestFile {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [hashtable[]] $Replacements
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Manifest not found: $Path"
    }

    $original = Get-Content -LiteralPath $Path -Raw
    $content = $original

    foreach ($rule in $Replacements) {
        $updated = [regex]::Replace($content, $rule.Pattern, $rule.Replacement)
        if ($updated -eq $content) {
            Write-Warning "Pattern did not match in $(Split-Path -Leaf $Path): $($rule.Pattern)"
        }
        $content = $updated
    }

    if ($content -eq $original) {
        Write-Host "unchanged : $(Resolve-Path -Relative $Path)"
        return
    }

    # Preserve the file's existing encoding style (UTF-8 without BOM).
    [System.IO.File]::WriteAllText($Path, $content, [System.Text.UTF8Encoding]::new($false))
    Write-Host "updated   : $(Resolve-Path -Relative $Path)"
}

# --- Scoop -------------------------------------------------------------------
Update-ManifestFile -Path (Join-Path $packaging 'scoop/kaniff.json') -Replacements @(
    @{ Pattern = '(?m)^(\s*"version"\s*:\s*")[^"]*(")'; Replacement = "`${1}$Version`${2}" }
    @{ Pattern = '(?m)^(\s*"url"\s*:\s*")https://github\.com/[^"]*/releases/download/v[0-9][^"]*(")'; Replacement = "`${1}$assetUrl`${2}" }
    @{ Pattern = '(?m)^(\s*"hash"\s*:\s*")[^"]*(")'; Replacement = "`${1}$lowerHash`${2}" }
)

# --- winget ------------------------------------------------------------------
$wingetVersionRule = @{ Pattern = '(?m)^(PackageVersion:\s*).*$'; Replacement = "`${1}$Version" }

foreach ($name in 'Kaniff.Kaniff.yaml', 'Kaniff.Kaniff.locale.en-US.yaml') {
    Update-ManifestFile -Path (Join-Path $packaging "winget/$name") -Replacements @($wingetVersionRule)
}

Update-ManifestFile -Path (Join-Path $packaging 'winget/Kaniff.Kaniff.installer.yaml') -Replacements @(
    $wingetVersionRule
    @{ Pattern = '(?m)^(\s*InstallerUrl:\s*).*$'; Replacement = "`${1}$assetUrl" }
    @{ Pattern = '(?m)^(\s*InstallerSha256:\s*).*$'; Replacement = "`${1}$upperHash" }
)

Write-Host ''
Write-Host 'Manifests are up to date.'
