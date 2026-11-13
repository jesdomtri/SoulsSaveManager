# Tasks

## 1. Baseline y seguridad de cambios

- [x] 1.1 Documentar comportamiento actual por juego/perfil (rutas, formato de backups, casos DM/RPCS3) en notas del cambio y verificar que cubre los flujos de crear/listar/cargar/eliminar.
- [x] 1.2 Crear proyecto de pruebas automatizadas y estructura mínima de ejecución (`dotnet test`) verificando que la suite corre en local/CI aunque inicialmente tenga pocos casos.
- [x] 1.3 Añadir pruebas de caracterización del comportamiento crítico actual (resolución de rutas/nombres y semántica observable de backup) y verificar que detectan cambios intencionales al modificar expectativas.

## 2. Rutas y configuración deterministas

- [x] 2.1 Implementar resolución de rutas absolutas por juego y contexto de usuario/perfil (sin prefijos sin unidad ni dependencia de CWD) y verificar con pruebas unitarias para DS1/DS2/DS3/SKR/ED/DM.
- [x] 2.2 Introducir un contrato único de configuración (`appsettings.json`) y un componente de acceso con lectura/escritura segura y atómica, verificando persistencia y recarga mediante pruebas.
- [x] 2.3 Hacer configurable el directorio raíz de backups y eliminar la dependencia de `.\Backups` relativo al CWD, verificando que distintas rutas de ejecución resuelven el mismo destino configurado.
- [x] 2.4 Implementar compatibilidad/migración no destructiva desde nombres/ubicaciones legadas de configuración y backups, verificando con pruebas que los datos existentes siguen siendo detectables y preservados.

## 3. Core de backup fiable y simétrico

- [x] 3.1 Implementar servicio de backup con operaciones de listar, crear, restaurar y eliminar usando copia recursiva que preserve estructura relativa, verificando con pruebas de IO en directorios temporales (incluye subdirectorios).
- [x] 3.2 Añadir validaciones de entradas y precondiciones (nombre de backup, existencia/lectura/escritura de origen y destino, colisiones) y verificar mediante pruebas y mensajes de error accionables.
- [x] 3.3 Incorporar confirmaciones para operaciones destructivas y manejo seguro de fallos (sin reportar éxito parcial; estrategia de restauración segura según decisión de diseño) verificando escenarios de cancelación y fallo inducido.
- [x] 3.4 Integrar el nuevo core en la UI de `BackupsManager` manteniendo los flujos existentes, verificando manualmente crear/listar/cargar/eliminar para un juego estándar y para el caso especial de Demon’s Souls.

## 4. Resolución de perfiles y resiliencia externa

- [x] 4.1 Implementar descubrimiento de perfiles/usuarios desde el almacenamiento resuelto y representación desacoplada de UI, verificando con pruebas casos con múltiples perfiles, ninguno o ruta inexistente.
- [x] 4.2 Implementar etiquetado con caché local y resolución externa opcional asíncrona (timeout/fallback al identificador), verificando que la ausencia de red o errores externos no bloquean la selección ni las operaciones de backup.
- [x] 4.3 Conectar la UI al nuevo resolvedor de perfiles manteniendo comportamiento funcional, verificando que se muestran etiquetas cacheadas/resueltas y que la carga no congela la interfaz en escenarios de fallo simulado.

## 5. Arquitectura, observabilidad y mantenibilidad

- [x] 5.1 Extraer y registrar servicios/abstracciones (rutas, settings, backups, perfiles) mediante DI y reducir lógica de negocio en code-behind, verificando que el core puede ejercerse sin controles WPF.
- [x] 5.2 Añadir logging estructurado y mapeo consistente de errores a mensajes de usuario con contexto, verificando que fallos de IO/config generan registros y mensajes claros.
- [x] 5.3 Limpiar dependencias y consolidar librerías (p.ej. JSON/HTTP/paquetes no usados) de acuerdo con la implementación final, verificando que el proyecto compila y los tests pasan sin referencias innecesarias.

## 6. Plataforma, CI y entrega

- [x] 6.1 Migrar el proyecto a una versión de .NET soportada (LTS) y actualizar paquetes compatibles, verificando `dotnet build` y `dotnet test` exitosos en el toolchain soportado.
- [x] 6.2 Revisar y ajustar `app.manifest` para privilegios mínimos requeridos, verificando que las operaciones soportadas funcionan sin elevación innecesaria o documentando la excepción justificada.
- [x] 6.3 Modernizar el workflow de CI (acciones y pasos vigentes) para ejecutar build, pruebas y validaciones configuradas de forma reproducible, verificando una ejecución exitosa en el pipeline.
- [x] 6.4 Definir y documentar un proceso de release reproducible (versionado y artefactos), verificando que genera los artefactos esperados a partir del proceso documentado.

## 7. Verificación integral y transición

- [x] 7.1 Ejecutar una validación integral de regresión sobre los flujos críticos (crear/restaurar/eliminar, preservación de estructura, perfiles, config, migración legada) y verificar que los criterios de los specs se cumplen.
- [x] 7.2 Documentar cambios de comportamiento, migración y decisiones abiertas resueltas (p.ej. ubicación de `BackupRoot`, estrategia de restore) y verificar que la documentación y el estado final del cambio quedan actualizados.
