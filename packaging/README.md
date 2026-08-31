# Packaging

Manifests to distribute the Kaniff CLI.

## These files are templates, not the published manifests

The version, URL and `REPLACE_WITH_SHA256` placeholders in this folder are
**never** filled in on `main`. A package hash describes a *published artifact*,
not the source code, so it does not belong in this repository's history — and it
cannot be known before the release exists anyway (the `.zip` is not reproducible
byte-for-byte, even though the binaries inside it are).

Instead, [.github/workflows/release.yml](../.github/workflows/release.yml) runs
[`scripts/Update-Manifests.ps1`](../scripts/Update-Manifests.ps1) *after* the
archives are built, using the real hash, and delivers the result to where each
package manager actually reads it:

| Manifest | Published to | Secret required |
| --- | --- | --- |
| `scoop/kaniff.json` | the `scoop-kaniff` bucket repository | `SCOOP_BUCKET_TOKEN` |
| `winget/*.yaml` | a PR to `microsoft/winget-pkgs` | `WINGET_TOKEN` |

Both jobs are skipped with a warning when the secret is not configured, so the
release still succeeds without them.

### Why Scoop needs a separate repository

A Scoop bucket *is* a git repository. `scoop update` does a `git pull` on the
bucket and reads the JSON from the working tree, which makes the manifest a
distribution channel rather than a build input — the same role a `.nupkg` plays
for NuGet. Keeping it in its own repo is what lets `main` stay protected.

### Running the generator manually

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
