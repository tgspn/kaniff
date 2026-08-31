# Packaging

Manifests to distribute the Kaniff CLI. Replace the placeholders before publishing:

- `tgspn` → your GitHub org/user.
- `REPLACE_WITH_SHA256` → SHA-256 of the released `kaniff-<version>-win-x64.zip`
  (`Get-FileHash kaniff-0.1.0-win-x64.zip -Algorithm SHA256`).
- Bump `version` / `PackageVersion` to match the release tag.

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
