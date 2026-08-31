# Packaging

Manifests to distribute the Kaniff CLI.

## These files update themselves

You do **not** need to edit the version, the download URL, or the SHA-256 by hand.
When a `vX.Y.Z` tag is pushed, [.github/workflows/release.yml](../.github/workflows/release.yml)
builds the archives, computes the hashes, and the `manifests` job runs
[`scripts/Update-Manifests.ps1`](../scripts/Update-Manifests.ps1) to rewrite these
files and commit them back to `main`.

`REPLACE_WITH_SHA256` is only a placeholder until the very first release.

To update them manually (for example after a hand-made release):

```powershell
# From a local archive - the hash is computed for you
./scripts/Update-Manifests.ps1 -Version 1.2.0 -ArchivePath ./kaniff-1.2.0-win-x64.zip

# Or from a known hash
./scripts/Update-Manifests.ps1 -Version 1.2.0 -Sha256 A1B2C3...
```

## NuGet (dotnet tool)

Published automatically by [.github/workflows/release.yml](../.github/workflows/release.yml)
when a `vX.Y.Z` tag is pushed, provided the `NUGET_API_KEY` repository secret is set.

```bash
dotnet tool install --global Kaniff.Cli
```

## Scoop

Host [scoop/kaniff.json](scoop/kaniff.json) in a Scoop bucket, then:

```bash
scoop bucket add kaniff https://github.com/tgspn/scoop-kaniff
scoop install kaniff
```

## winget

Submit the manifests under [winget/](winget/) to
[microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs), then:

```bash
winget install Kaniff.Kaniff
```
