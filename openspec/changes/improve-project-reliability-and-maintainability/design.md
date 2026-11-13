# Design

## Context

SoulsSaveManager es una app WPF (.NET 6, Windows) con lógica concentrada en code-behind (`BackupsManager.xaml.cs`), un modelo simple (`Game`) y utilidades acopladas a UI/HTTP/config (`UtilsClass`). El estado actual y los problemas concretos (asimetría create/load, rutas sin raíz y dependientes de CWD, config inconsistente, scraping síncrono, ausencia de pruebas, runtime/deps desactualizados, CI antigua y privilegios elevados) se detallan en `proposal.md` (Why/What/Impact) y en el análisis exploratorio. Este diseño define cómo abordar esas capacidades de forma incremental y verificable.

## Goals / Non-Goals

**Goals:**
- Garantizar semántica correcta y simétrica de backup/restore preservando estructura de archivos.
- Hacer determinista la resolución de rutas y consistente la persistencia de configuración, con compatibilidad/migración desde el comportamiento legado cuando sea viable.
- Separar dominio/IO/UI para habilitar pruebas automatizadas del core y reducir regresiones.
- Mejorar diagnósticos (logging + errores accionables) y tolerancia a fallos externos (p.ej. lookup de perfiles).
- Modernizar plataforma, dependencias, privilegios y CI/release hacia un baseline soportado y reproducible.

**Non-Goals:**
- Reescritura completa de la UI o cambio de framework de presentación en esta etapa.
- Soporte multiplataforma fuera de Windows o gestión de saves en la nube.
- Detección automática perfecta de todos los juegos/plataformas más allá de los soportados actuales (se permiten mejoras incrementales).
- Cambios de comportamiento no relacionados con fiabilidad, mantenibilidad o entrega.

## Decisions

### 1) Core de backup como servicio transaccional y simétrico
- **Decisión:** introducir `IBackupService` (o equivalente) responsable de `List/Create/Restore/Delete` con copia **recursiva** simétrica, validación de entradas (nombres/rutas), y estrategia de seguridad ante fallos (p.ej. copia a directorio temporal y promoción/rollback cuando aplique; nunca reportar éxito parcial).
- **Rationale:** corrige el defecto central observado (crear no recursivo vs restore recursivo aplanado) y centraliza reglas de negocio fuera de la UI.
- **Alternativas:** mantener lógica en code-behind y “parchear” casos (rechazada: frágil, difícil de probar); usar librerías de sync complejas (posible sobrecosto para el alcance actual).

### 2) Resolución de rutas explícita y configurable
- **Decisión:** introducir `IPathResolver`/`IGameProfileCatalog` que produzca rutas **absolutas** (basadas en `Environment.SpecialFolder`/`Path.Combine`), modele `SaveRoot/BackupRoot` por juego+contexto, y permita configurar `BackupRoot` (persistido) en lugar de depender de `.\Backups` o del CWD.
- **Rationale:** elimina ambigüedad de `\Users\...` sin unidad y dependencia del directorio de trabajo; facilita migración.
- **Alternativas:** seguir con rutas relativas pero normalizarlas al inicio (mitiga poco); hardcodear todo por juego (no aborda configurabilidad/migración).

### 3) Configuración unificada y migración segura
- **Decisión:** estandarizar en un único archivo `appsettings.json` y un `ISettingsStore` con esquema definido (caché de perfiles, ubicaciones custom como DM, preferencias). Implementar lectura tolerante a legado (`appSettings.json`/ubicaciones previas) y escritura atómica. Consolidar serialización (preferir una sola pila, idealmente System.Text.Json, evaluando compatibilidad).
- **Rationale:** resuelve inconsistencias actuales de nombre y doble stack JSON, y protege datos persistidos.
- **Alternativas:** mantener Newtonsoft+STJ en paralelo (deuda continua); migración destructiva sin compatibilidad (riesgo de pérdida de datos de usuario).

### 4) Resolución de perfiles desacoplada, con caché y fallback
- **Decisión:** introducir `IUserProfileResolver` que enumere perfiles desde storage y resuelva etiquetas con caché local; la resolución externa (Steam) será opcional, asíncrona, con timeout/fallback al identificador, y sin bloquear operaciones de backup.
- **Rationale:** elimina el acoplamiento UI/red síncrono actual y mejora resiliencia ante cambios de terceros o falta de conectividad.
- **Alternativas:** eliminar lookup externo (pierde UX existente); mantener scraping síncrono (congela UI y es frágil).

### 5) Arquitectura por capas y observabilidad
- **Decisión:** separar UI (views/code-behind mínimo) de servicios de dominio/infra registrados por DI; agregar logging estructurado (p.ej. `Microsoft.Extensions.Logging` + sink a archivo) y mapear excepciones a mensajes accionables. Mantener MVVM ligero donde aporte, sin forzar reescritura total.
- **Rationale:** habilita pruebas y diagnósticos sin un big-bang de UI.
- **Alternativas:** MVVM completo inmediato (mayor costo/riesgo); solo logging sin refactor (no soluciona acoplamiento/testabilidad).

### 6) Estrategia de pruebas
- **Decisión:** añadir proyecto(s) de test para: (a) caracterización de comportamiento actual crítico, (b) unidad del core (rutas, settings, naming, semántica de copia con `System.IO.Abstractions` o directorios temporales reales), y (c) integración ligera de IO. Ejecutar en CI.
- **Rationale:** los cambios tocan operaciones destructivas sobre saves; se requiere red de seguridad.
- **Alternativas:** solo pruebas manuales (insuficiente para regresiones de filesystem).

### 7) Plataforma, privilegios y entrega incremental
- **Decisión:** migrar a .NET LTS soportado y actualizar paquetes/acciones de CI en pasos compatibles; revisar `app.manifest` para reducir a privilegios mínimos (`asInvoker`) si las rutas soportadas no requieren elevación. Formalizar pipeline de release reproducible.
- **Rationale:** reduce riesgo de soporte/seguridad y mejora confiabilidad de entrega.
- **Alternativas:** permanecer en .NET 6 (sin soporte); actualizar todo en un solo salto (mayor riesgo).

## Risks / Trade-offs

- [Cambio de ubicación/formato de backups o settings] → Compatibilidad de lectura + migración explícita y no destructiva, con aviso al usuario y preservación de datos legados.
- [Regresiones en operaciones de copia/restauración] → Suite de caracterización + tests de IO + operaciones no destructivas por defecto y validación post-copia.
- [Reducción de privilegios rompe escenarios que hoy dependían de admin] → Inventariar rutas reales usadas, probar con `asInvoker`, y solo elevar de forma puntual si un caso lo exige.
- [Lookup externo (Steam) inestable/cambios de HTML] → Resolver de forma asíncrona con timeout, caché persistente y fallback a ID; aislar proveedor tras interfaz.
- [Actualización de runtime/deps/CI introduce incompatibilidades] → Migración por fases (baseline → core → plataforma), builds reproducibles y validación en CI.
- [Alcance amplio en una app pequeña] → Priorizar Fase 1 (correctitud core) antes de refactors extensos de UI/packaging.

## Migration Plan

1. **Baseline:** documentar comportamiento/rutas actuales, introducir tests de caracterización y fixtures de filesystem temporal.
2. **Core seguro:** implementar `IPathResolver`/`ISettingsStore`/`IBackupService` y enrutar la UI existente a ellos manteniendo UX equivalente; añadir migración/compatibilidad de `appsettings` y descubrimiento de backups legados.
3. **Robustez y observabilidad:** integrar logging, mejorar mensajes/confirmaciones y hacer opcional-asíncrono el enriquecimiento de perfiles.
4. **Plataforma y entrega:** migrar target framework/paquetes, ajustar manifest/privilegios según evidencia, modernizar CI y definir release reproducible.
5. **Rollout:** validar con escenarios reales por juego (incluido DM/RPCS3), comunicar cambios de ubicación/config y mantener lectura de legado durante una ventana de transición. **Rollback:** conservar copia de settings/backups legados intactos y permitir volver a versión previa mientras la migración sea no destructiva.

## Open Questions

- ¿Cuál debe ser la ubicación por defecto del `BackupRoot` (p.ej. `%LOCALAPPDATA%`/Documentos) y cómo se comunica/migra desde `.\Backups` relativo al CWD sin sorprender a usuarios existentes?
- ¿La restauración debe sobrescribir in-place o ofrecer modo seguro (pre-backup automático del estado actual antes de cargar)?
- ¿Se mantiene la resolución de nombres vía Steam (scraping/otro mecanismo) o se degrada a una estrategia local/opt-in dada su fragilidad?
- ¿Qué nivel de compatibilidad se exige para Demon’s Souls (RPCS3) y variaciones de estructura de guardado entre versiones de juegos?
- ¿Requiere algún escenario real permisos elevados que impidan adoptar `asInvoker` por defecto?
