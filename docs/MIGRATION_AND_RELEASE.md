# Migration, Behavior Changes, and Release

## Behavior changes (implemented)

- **.NET runtime:** migrated from `net6.0-windows` to **.NET 8 (LTS)** (`net8.0-windows`).
- **Backup/restore semantics:** backups are now **recursive and structure-preserving** in both create and restore (fixes previous non-recursive create / flattened restore behavior).
- **Path resolution:** save and backup paths are resolved as **absolute paths** (no `\Users\...` without drive, no dependence on current working directory).
- **Default backup location:** backups default to `%LOCALAPPDATA%\SoulsSaveManager\Backups\<Game>\<Profile>` (configurable via settings `backupRoot`).
- **Configuration:** canonical settings file is `%LOCALAPPDATA%\SoulsSaveManager\appsettings.json` with schema `{ "backupRoot": ..., "users": {...}, "locations": {"DM": "..."} }`. Writes are atomic.
- **Profile labels:** cached labels are used; external name resolution (Steam XML endpoint) is optional, async, and falls back to profile ID on failure (non-blocking).
- **Safety/UX:** delete and overwrite-restore require confirmation; errors are surfaced with actionable messages and logged to `%LOCALAPPDATA%\SoulsSaveManager\logs\app.log`.
- **Privileges:** application manifest requests `asInvoker` (no forced administrator elevation) for standard per-user save operations.
- **Dependencies:** removed unused/legacy packages (HtmlAgilityPack, ScrapySharp, Newtonsoft.Json, old config/ASP.NET abstractions); core uses built-in JSON/HTTP and DI.

## Migration notes

- **Settings migration:** on first run with the new version, the app looks for legacy `appsettings.json` / `appSettings.json` in the app base directory and current working directory and migrates values into the canonical LocalAppData settings file (non-destructive).
- **Legacy backups discovery:** backup listing/restore/delete can discover backups stored in legacy locations:
  - `<AppBaseDir>\Backups\<Game>\<Profile>`
  - `<CWD>\Backups\<Game>\<Profile>` (as previously used)
  Newly created backups are written to the configured/default backup root (`%LOCALAPPDATA%\SoulsSaveManager\Backups\...` by default).
- **Demon’s Souls (RPCS3):** custom save location continues to be supported and persisted in `locations.DM`.
- **Existing backups:** legacy backup folders are not deleted or moved automatically. Users may copy/move them into the new backup root if desired.

## Open design decisions (resolved/defaulted for this implementation)

- **BackupRoot default:** `%LOCALAPPDATA%\SoulsSaveManager\Backups` (per-user, writable without elevation). Legacy locations are discoverable for compatibility.
- **Restore strategy:** in-place restore preserving structure with explicit overwrite confirmation (no silent success). Pre-restore snapshot is not automated in this iteration.
- **Steam name resolution:** optional, asynchronous lookup via Steam profile XML with timeout/fallback to ID and local cache; core workflows remain usable offline.
- **Privileges:** default to `asInvoker`; elevate only if a specific environment proves necessary (not observed for standard user-profile save paths).

## Release process (reproducible)

### Prerequisites
- Windows with .NET 8 SDK (`8.0.x`)
- `dotnet` available on PATH

### Build & test
```powershell
dotnet restore SoulsSaveManager.sln
dotnet build SoulsSaveManager.sln --configuration Release
dotnet test SoulsSaveManager.sln --configuration Release --no-build
```

### Publish artifacts
Example (framework-dependent, win-x64):
```powershell
dotnet publish SoulsSaveManager/SoulsSaveManager.csproj -c Release -r win-x64 --self-contained false -o artifacts/publish/win-x64-fdd
```

Example (self-contained, win-x64):
```powershell
dotnet publish SoulsSaveManager/SoulsSaveManager.csproj -c Release -r win-x64 --self-contained true -o artifacts/publish/win-x64-sc
```

Package the contents of the chosen publish output directory into a versioned zip (e.g. `SoulsSaveManager-v<version>-win-x64.zip`) and attach to the GitHub release/tag. Ensure the application version is updated as appropriate before tagging.
