# SoulsSaveManager

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=jesdomtri_SoulsSaveManager&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=jesdomtri_SoulsSaveManager)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=jesdomtri_SoulsSaveManager&metric=bugs)](https://sonarcloud.io/summary/new_code?id=jesdomtri_SoulsSaveManager)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=jesdomtri_SoulsSaveManager&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=jesdomtri_SoulsSaveManager)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=jesdomtri_SoulsSaveManager&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=jesdomtri_SoulsSaveManager)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=jesdomtri_SoulsSaveManager&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=jesdomtri_SoulsSaveManager)

Aplicación de escritorio WPF para Windows que permite gestionar backups de partidas guardadas de juegos Souls-like (Dark Souls Remastered, Dark Souls II/III, Sekiro, Elden Ring y Demon’s Souls vía RPCS3).

## Características

- Selección de juego y perfiles/usuarios detectados desde las rutas de guardado.
- Crear, listar, restaurar y eliminar backups de forma recursiva preservando la estructura de archivos.
- Ubicación de backups configurable y determinista por defecto en `%LOCALAPPDATA%\SoulsSaveManager\Backups`.
- Soporte de ubicación personalizada para Demon’s Souls (RPCS3).
- Etiquetado de perfiles con caché local y resolución opcional de nombres (Steam) de forma asíncrona y tolerante a fallos.
- Logging de diagnóstico en `%LOCALAPPDATA%\SoulsSaveManager\logs\app.log`.

## Requisitos

- Windows
- .NET 8 SDK (para compilar) / .NET 8 Desktop Runtime compatible (según distribución)

## Compilar y probar

```powershell
dotnet restore SoulsSaveManager.sln
dotnet build SoulsSaveManager.sln --configuration Release
dotnet test SoulsSaveManager.sln --configuration Release
```

## Migración y publicación

Consulta [docs/MIGRATION_AND_RELEASE.md](docs/MIGRATION_AND_RELEASE.md) para cambios de comportamiento, migración desde versiones anteriores y el proceso de release reproducible.
