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
| `packaging/` | Scoop and winget manifests (updated automatically on release). |
| `scripts/` | Release helper scripts. |
| `docs/images/` | Screenshots referenced by `README.md`. |

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
- Screenshots go in `docs/images/`. Capture them with a **dark** system theme —
  the sidebar and cards are hard-coded dark, so a light theme renders the window
  as a mismatched hybrid. Never screenshot the **My IP** tool: it is the tool the
  app opens on and it refreshes automatically, so a capture would commit your
  real public IP and local interfaces to the repository.

## Pull requests

`main` is protected: **every change goes through a pull request**, including
changes from the maintainers. Direct pushes are rejected.

- Create a branch from `main`.
- Keep pull requests focused on a single change.
- Describe what changed and why. Include CLI output or a screenshot when the
  change is user-visible.
- The `build` check must pass, your branch must be up to date with `main`, and
  all review conversations must be resolved before merging.
- History is linear: merges are done by **squash** or **rebase**, so keep your
  commit messages meaningful.

```bash
git switch -c my-change
# ... work ...
git push -u origin my-change
gh pr create --fill
```

## Reporting bugs

Open an issue using the bug report template and include the Kaniff version, your
OS, the exact command you ran, and what you expected to happen.
