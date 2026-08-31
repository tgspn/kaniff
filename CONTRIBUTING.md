# Contributing to Kaniff

Thanks for taking the time to contribute! Kaniff is a Swiss-army knife of offline
developer tools, and new tools are always welcome.

## Getting started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/tgspn/kaniff.git
cd kaniff
dotnet build
dotnet test
```

Run the CLI:

```bash
dotnet run --project src/Kaniff.Cli -- list
```

Run the desktop app:

```bash
dotnet run --project src/Kaniff.Desktop
```

## Project layout

| Path | Purpose |
| --- | --- |
| `src/Kaniff.Core` | All tool logic. No UI dependencies. |
| `src/Kaniff.Cli` | Command-line front end (`kaniff`). |
| `src/Kaniff.Desktop` | Avalonia desktop front end. |
| `tests/Kaniff.Tests` | xUnit tests for `Kaniff.Core`. |
| `packaging/` | Scoop and winget manifests. |

## Adding a new tool

1. Create `src/Kaniff.Core/Tools/MyTool.cs` implementing `ITool`. Keep the logic
   pure and UI-free, and return a `record` with the results.
2. Register it in `src/Kaniff.Core/ToolCatalog.cs`.
3. Add a verb in `src/Kaniff.Cli/Program.cs` and document it in `PrintHelp()`.
4. Add a `MyToolViewModel` + `MyToolView.axaml` under `src/Kaniff.Desktop`, then
   register the view model in `MainViewModel`.
5. Add tests in `tests/Kaniff.Tests/ToolTests.cs`.
6. Add a row to the tools table in `README.md`.

## Guidelines

- Tools must work **offline**, except where the tool is inherently network-bound
  (for example, public IP lookup).
- Never send user input to a third-party service.
- Keep dependencies to a minimum.
- Code, comments, and documentation are written in English.
- `dotnet build` must produce **zero warnings** and `dotnet test` must pass.

## Pull requests

- Create a branch from `main`.
- Keep pull requests focused on a single change.
- Describe what changed and why. Include CLI output or a screenshot when the
  change is user-visible.
- CI must be green before review.

## Reporting bugs

Open an issue using the bug report template and include the Kaniff version, your
OS, the exact command you ran, and what you expected to happen.
