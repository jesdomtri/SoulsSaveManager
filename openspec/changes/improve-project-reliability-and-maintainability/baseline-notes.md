# Baseline: comportamiento actual (SoulsSaveManager)

> Notas de caracterización para el cambio `improve-project-reliability-and-maintainability` (task 1.1).
> Basado en inspección del código fuente actual (`SoulsSaveManager/`).

## Juegos, alias y rutas (según `Entities/Game.cs`)

| Alias | Nombre/carpeta (`Name`)        | Ruta base de guardado (`SaveDataPath`) | Ruta base de backups (`BackupPath`) |
|------:|---------------------------------|----------------------------------------|-------------------------------------|
| DS1   | `DARK SOULS REMASTERED`         | `\Users\{UserName}\Documents\NBGI\{Name}` | `.\\Backups\\{Name}` |
| DS2   | `DarkSoulsII`                   | `\Users\{UserName}\AppData\Roaming\{Name}` | `.\\Backups\\{Name}` |
| DS3   | `DarkSoulsIII`                  | `\Users\{UserName}\AppData\Roaming\{Name}` | `.\\Backups\\{Name}` |
| SKR   | `Sekiro`                        | `\Users\{UserName}\AppData\Roaming\{Name}` | `.\\Backups\\{Name}` |
| ED    | `EldenRing`                     | `\Users\{UserName}\AppData\Roaming\{Name}` | `.\\Backups\\{Name}` |
| DM    | `DemonSouls`                    | `""` (vacío; se espera selección manual) | `.\\Backups\\{Name}` |

**Observaciones:**
- `SaveDataPath` se construye sin letra de unidad (prefijo `\Users\...`), por lo que la resolución depende de la unidad/directorio actual.
- `BackupPath` es relativo al CWD (`.\\Backups\\...`).
- Alias desconocido => `Name=""` y ruta default de Roaming con nombre vacío (estado inválido silencioso).

## Modelo de perfiles/usuarios

- **DS1/DS2/DS3/SKR/ED:** `UtilsClass.LoadUsersComboBox()` enumera subdirectorios de `game.SaveDataPath` y muestra `{dir} - {username}`.
  - `username` se obtiene de `App.UsersCache` (`appsettings.json`, sección `users`) o, si falta, se consulta `https://steamcommunity.com/profiles/{GetUserIDWeb(id)}/` (scraping con HtmlAgilityPack del `span.actual_persona_name`) y se persiste vía `OverwriteUsersAppSettings`.
  - `GetUserIDWeb`: para IDs numéricos usa `[U:1:{id}]` en DS1 y el id directo en otros; si no, intenta parseo hex.
- **DM (RPCS3):** lista fija `["NOT USER FOR DEMON SOULS IN RPCS3"]`, combo de usuarios deshabilitado; la ruta de guardado se toma/guarda en `locations:DM` de config y se selecciona manualmente.
- Al cambiar usuario: `_userSaveDataPath = SaveDataPath + selectedUser`, `_userBackupPath = BackupPath + selectedUser`.

## Flujos y semántica observable

### Crear backup (`NewBackup_Click`)
- Nombre = `NewBackupTextBox.Text.Trim()`. Rechaza vacío o si `targetPath` ya existe.
- `targetPath = _userBackupPath\<name>`.
- **Fuente:** `sourcePath = string.IsNullOrWhiteSpace(SaveLocationTextBox.Text) ? targetPath : SaveLocationTextBox.Text;` (si textbox vacío usa el destino; comportamiento sospechoso).
- Valida que `sourcePath` exista, crea `targetPath` y copia **solo archivos del primer nivel**: `Directory.GetFiles(sourcePath)` + `File.Copy` a `targetPath` (sin recursión).
- Refresca lista y muestra "Done".

### Listar backups (`LoadBackupsComboBox`)
- Si existe `userBackupPath`, lista nombres de subdirectorios directos; selecciona índice 0 si hay elementos.

### Cargar/restaurar backup (`LoadBackup_Click`)
- `sourcePath = _userBackupPath\<name>`; `targetPath = SaveLocationTextBox.Text` o `_userSaveDataPath` si vacío.
- Crea `targetPath` si no existe.
- Copia **recursiva** desde source pero **aplana al destino**: `Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)` y copia cada archivo como `Path.Combine(targetPath, Path.GetFileName(file))` con overwrite.
- **Riesgo:** pérdida de estructura de carpetas y colisiones por nombre de archivo.

### Eliminar backup (`DeleteBackup_Click`)
- Borra `Directory.Delete(_userBackupPath\<name>, true)` de forma asíncrona (`Task.Run`) sin confirmación previa explícita; muestra "Done"/error y refresca.

### Abrir ubicaciones / seleccionar-reset
- Abrir save/backup: `Process.Start("explorer.exe", path)` si existe.
- Seleccionar ubicación (DM): `FolderBrowserDialog`, actualiza textbox/ruta y persiste `locations:DM`.
- Reset: restaura ruta derivada del usuario/juego (o limpia en DM).

## Configuración (`App.xaml.cs`, `UtilsClass`)
- `CreateAppSettings` crea `appsettings.json` (nota: distinto casing que el leído en algunos puntos) con `{"users": {}, "locations": {"DM": "Select save location pls"}}` si falta.
- Se carga `appsettings.json` desde CWD con `reloadOnChange: true`; caches en `App.UsersCache/LocationsCache`.
- `OverwriteUsersAppSettings` lee/escribe `appSettings.json` (casing distinto) usando Newtonsoft.Json (`dynamic`) y recarga caches; errores se notifican genéricamente.

## Otros aspectos relevantes
- `app.manifest`: `requireAdministrator`.
- `SoulsSaveManager.csproj`: `net6.0-windows`, WPF+WinForms, paquetes incluyen HtmlAgilityPack, Newtonsoft.Json, ScrapySharp, Microsoft.AspNetCore.Mvc.Abstractions, Microsoft.Extensions.*.
- Sin proyecto de pruebas; CI (`.github/workflows/build.yml`) usa actions antiguas y análisis SonarCloud del repositorio original.
