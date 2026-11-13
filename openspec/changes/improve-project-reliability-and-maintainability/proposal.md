# Proposal

## Why

SoulsSaveManager es una utilidad WPF útil para gestionar backups de partidas de juegos Souls-like, pero hoy presenta riesgos de correctitud (copias no simétricas/posible pérdida de estructura), fuerte dependencia de rutas relativas/CWD y configuración inconsistente, además de deuda técnica (UI y lógica acopladas, sin pruebas, dependencias desactualizadas y .NET 6 fuera de soporte). Mejorar su fiabilidad y mantenibilidad es necesario para evitar corrupción o pérdida de saves y facilitar su evolución segura.

## What Changes

- Hacer **fiable y simétrico** el flujo de backup/restore: copia recursiva, preservación de estructura, validaciones previas, confirmaciones en operaciones destructivas y manejo de fallos sin estados parciales silenciosos.
- **Normalizar rutas y configuración**: rutas con raíz explícita (sin prefijos ambiguos), backup root configurable (no dependiente de `.\Backups`/CWD), y un único contrato de `appsettings.json` con lectura/escritura consistente.
- **Separar responsabilidades** para testabilidad y mantenimiento: extraer servicios de resolución de rutas/juegos, operaciones de backup, resolución de perfiles de usuario y almacenamiento de settings; desacoplar utilidades de controles WPF.
- **Mejorar observabilidad y experiencia operativa**: logging estructurado, mensajes de error accionables y comportamiento predecible ante casos borde (nombres inválidos, rutas inexistentes, sin red para lookup de Steam).
- **Actualizar plataforma y entrega**: migrar a una versión de .NET soportada, limpiar/actualizar dependencias, revisar privilegios de ejecución y modernizar CI/release.
- Establecer una base de **pruebas automatizadas** (caracterización y pruebas del core de IO/rutas) para prevenir regresiones.
- **BREAKING (posible, a definir en diseño/migración)**: cambios en la ubicación/formato de almacenamiento de backups o en claves/rutas de configuración para corregir el modelo actual relativo al CWD.

## Capabilities

### New Capabilities
- `backup-core`: operaciones de creación, listado, restauración y eliminación de backups de forma recursiva, simétrica y segura (validaciones, preservación de estructura, manejo de errores).
- `path-and-config`: resolución determinista de rutas de guardado/backups por juego y usuario, configuración persistente consistente (`appsettings.json`) y migración desde el comportamiento relativo al CWD cuando aplique.
- `user-profile-resolution`: identificación/etiquetado de usuarios de guardado con caché local y resolución opcional de nombres (p.ej. Steam) desacoplada de la UI y tolerante a fallos de red.
- `app-maintainability`: arquitectura modular (servicios/abstracciones), logging/diagnóstico y base de pruebas para el core.
- `platform-and-delivery`: actualización de runtime/dependencias, revisión de privilegios/manifest y pipeline CI/release mantenible.

### Modified Capabilities
- (none)

## Impact

- **Código afectado**: `SoulsSaveManager/App.xaml.cs`, `MainWindow*.xaml(.cs)`, `BackupsManager*.xaml(.cs)`, `Entities/Game.cs`, `Utils/UtilsClass.cs`, y nueva estructura de servicios/modelos/pruebas.
- **Datos/configuración**: `appsettings.json` (esquema/nombre/ubicación de uso) y carpeta de backups (posible reubicación o migración).
- **Dependencias/plataforma**: `SoulsSaveManager.csproj` (target framework, paquetes como HtmlAgilityPack/Newtonsoft/otros), `app.manifest` (nivel de privilegios).
- **CI/CD**: `.github/workflows/build.yml` y proceso de publicación/empaquetado.
- **Sistemas externos**: integración opcional con Steam para nombres de perfil (debe degradar correctamente sin red/cambios externos).
