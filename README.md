# 🔪 Kaniff

[![CI](https://github.com/tgspn/kaniff/actions/workflows/ci.yml/badge.svg)](https://github.com/tgspn/kaniff/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Kaniff.Cli.svg)](https://www.nuget.org/packages/Kaniff.Cli)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

Your developer Swiss Army knife — the little online tools you always google for, in one offline app. Available as a **CLI** and a **desktop GUI** (Windows now, Linux/macOS ready via Avalonia).

## Why

Every day you need a one-off tool: decode a Base64 string, check your public IP, peek inside a JWT, format some JSON. Kaniff keeps them all in a single place, offline, no ads, no "accept cookies".

## Tools

| Tool | CLI verb | What it does |
| ------ | ---------- | -------------- |
| My IP | `ip` | Public IP (ifconfig.me + fallbacks) and local addresses |
| Base64 | `base64` | Encode/decode text (standard or URL-safe) |
| URL Encode | `url` | Percent-encode/decode text for URLs |
| JWT | `jwt` | Decode a token's header and payload (no verification) |
| Hash | `hash` | MD5 / SHA-1 / SHA-256 / SHA-512 of text |
| UUID | `uuid` | Generate random version-4 UUIDs |
| Timestamp | `timestamp` | Convert Unix time to a date and back |
| Case | `case` | camelCase, snake_case, kebab-case, and more |
| Color | `color` | Convert between HEX, RGB and HSL |
| Regex | `regex` | Test a regex and inspect matches/groups |
| QR Code | `qr` | Generate a QR code (ASCII or PNG) |
| String Comparer | `strcmp` | Compare two strings, find the first difference |
| JSON | `json` | Format, minify and validate JSON |

## Project layout

```
src/
  Kaniff.Core/      Shared tool logic (no UI) — the plugin surface
  Kaniff.Cli/       Command-line interface (kaniff.exe)
  Kaniff.Desktop/   Avalonia MVVM desktop app
tests/
  Kaniff.Tests/     xUnit tests for the Core tools
packaging/          Scoop and winget manifests
.github/workflows/  CI and release automation
```

All tools live in `Kaniff.Core` and are shared by both the CLI and the desktop app.

## Install

Once a release is published, pick whichever you prefer:

```bash
# .NET global tool (any OS with the .NET 10 runtime)
dotnet tool install --global Kaniff.Cli

# Scoop (Windows)
scoop bucket add kaniff https://github.com/tgspn/scoop-kaniff
scoop install kaniff

# winget (Windows)
winget install Kaniff.Kaniff
```

### Standalone downloads

Every [release](https://github.com/tgspn/kaniff/releases/latest) ships self-contained
builds — no .NET runtime required. Verify them against `SHA256SUMS.txt`.

| Platform | CLI | Desktop |
| --- | --- | --- |
| Windows x64 | `kaniff-<v>-win-x64.zip` | `kaniff-desktop-<v>-win-x64.zip` |
| Linux x64 | `kaniff-<v>-linux-x64.tar.gz` | `kaniff-desktop-<v>-linux-x64.tar.gz` |
| Linux arm64 | `kaniff-<v>-linux-arm64.tar.gz` | `kaniff-desktop-<v>-linux-arm64.tar.gz` |
| macOS Apple Silicon | `kaniff-<v>-osx-arm64.tar.gz` | `kaniff-desktop-<v>-osx-arm64.tar.gz` |
| macOS Intel | `kaniff-<v>-osx-x64.tar.gz` | `kaniff-desktop-<v>-osx-x64.tar.gz` |

## Requirements

Only to build from source:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## CLI usage

```bash
dotnet run --project src/Kaniff.Cli -- <command> [options]
```

Examples:

```bash
kaniff ip                          # public + local IPs
kaniff ip --local                  # local only
kaniff base64 encode "hello" -u    # URL-safe Base64
kaniff base64 decode aGVsbG8=
kaniff url encode "a b&c=1"
kaniff jwt <token>
kaniff hash "hello"
kaniff uuid 3 -u                   # 3 uppercase UUIDs
kaniff timestamp 1700000000        # Unix -> date (empty = now)
kaniff case "hello world"          # camelCase, snake_case, ...
kaniff color "#3498db"             # HEX/RGB/HSL
kaniff regex "\d+" "abc123"        # list matches
kaniff qr "https://example.com"    # ASCII QR (use --png file.png for an image)
kaniff strcmp "abc" "abd" -i
kaniff json format  '{"a":1}'
echo '{"a":1}' | kaniff json minify   # also reads from stdin
kaniff list                        # list all tools
```

Build a standalone `kaniff.exe`:

```bash
dotnet publish src/Kaniff.Cli -c Release -r win-x64 --self-contained
```

### Install as a global `dotnet tool`

Pack and install the `kaniff` command from the local package:

```bash
dotnet pack src/Kaniff.Cli -c Release
dotnet tool install --global --add-source ./artifacts/nupkg Kaniff.Cli
kaniff list                        # now available everywhere
```

Update or remove it later with `dotnet tool update -g Kaniff.Cli` / `dotnet tool uninstall -g Kaniff.Cli`.

## Desktop app

```bash
dotnet run --project src/Kaniff.Desktop
```

## Contributing

Contributions are very welcome — especially new tools. See
[CONTRIBUTING.md](CONTRIBUTING.md) for the setup and the step-by-step guide to
adding a tool, and note that this project follows a
[Code of Conduct](CODE_OF_CONDUCT.md).

Quick version of adding a new tool:

1. Create a class in `Kaniff.Core/Tools` implementing `ITool` with its operations.
2. Register it in `Kaniff.Core/ToolCatalog.cs`.
3. Add a `case` in the CLI (`Kaniff.Cli/Program.cs`).
4. Add a `ViewModel` + `View` pair in the desktop app and list it in `MainViewModel`.
5. Add tests in `tests/Kaniff.Tests` and a row to the tools table above.

## Releasing

Everything is automated. Push a tag and the
[release workflow](.github/workflows/release.yml) will:

1. Build the CLI and desktop app for 5 platforms (self-contained, single file).
2. Pack the CLI as a `dotnet tool` and push it to NuGet.
3. Publish a GitHub release with all archives and a `SHA256SUMS.txt`.
4. Generate the Scoop manifest from the real hash and push it to the bucket repo.
5. Open a pull request against `microsoft/winget-pkgs`.

```bash
git tag v0.1.0
git push origin v0.1.0
```

### Release configuration

Steps 2, 4 and 5 are skipped with a warning when their configuration is missing,
so the release still succeeds without them.

| Name | Kind | Used by | How to set it up |
| --- | --- | --- | --- |
| `NUGET_USER` | variable | step 2 | Your nuget.org profile name (not your e-mail, and not the package name). Not a secret. |
| `SCOOP_BUCKET_TOKEN` | secret | step 4 | A fine-grained PAT for `tgspn/scoop-kaniff` only, with **Contents: Read and write** |
| `WINGET_TOKEN` | secret | step 5 | A **classic** PAT with the `public_repo` scope. Fine-grained PATs are not supported by the action. |

NuGet publishing uses
[Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
rather than a stored API key: GitHub issues a short-lived OIDC token, nuget.org
validates it against a policy registered for this repository and workflow, and
returns a temporary key valid for one hour. Nothing long-lived is stored, so
there is no key to rotate or leak. Register the policy under
**nuget.org → your username → Trusted Publishing** with:

| Field | Value |
| --- | --- |
| Repository Owner | `tgspn` |
| Repository | `kaniff` |
| Workflow File | `release.yml` |
| Environment | *(leave empty)* |

> A new policy for a repository that has never published is *temporarily active*
> for 7 days. Publish within that window or restart it, otherwise it goes
> inactive.

The built-in `GITHUB_TOKEN` cannot be used for steps 4 and 5 because it is
scoped to this repository and cannot push to another repo or create forks.
Unlike NuGet, winget has no trusted-publishing equivalent yet, so step 5 still
needs a real credential.

Step 5 additionally requires, one time only:

- `microsoft/winget-pkgs` **forked under this account** — the action pushes to
  the fork and does not create it for you.
- **The first version submitted by hand.** The action updates an existing
  package; it cannot introduce `Kaniff.Kaniff` to winget-pkgs. Use
  [Komac](https://github.com/russellbanks/Komac) or `wingetcreate` for v0.1.0,
  after which every later release is automatic.

See [packaging/README.md](packaging/README.md) for why the manifests live
outside this repository.

## Roadmap

- More tools (hexdump, cron expression explainer, diff viewer, lorem ipsum)
- Tool search / command palette in the desktop app
- Homebrew formula for macOS

## Security

Kaniff runs entirely offline except for the public IP lookup. The JWT tool
**decodes** tokens but does not verify signatures. To report a vulnerability, see
[SECURITY.md](SECURITY.md).

## License

Licensed under the [MIT License](LICENSE).
