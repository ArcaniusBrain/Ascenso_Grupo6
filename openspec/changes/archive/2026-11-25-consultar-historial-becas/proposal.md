# Proposal

## Why

El equipo necesita habilitar en **S4 (`Ascenso.S4.Becas`)** la consulta completa del historial de becas por persona, por programa o por periodo (**RF-18**, **HU-18.1**), respetando la regla de no doble beneficio (**S6** sobre estados activos, excluyendo *Inactiva*), la consulta histórica de periodos anteriores (**RF-38**), el control de acceso por rol (**RF-37**) y la confidencialidad socioeconómica. Este requisito fue confirmado en **S4** por la decisión **C1** (`pendientes-v1.3.md`) y sigue los lineamientos de **ADR-02**, **ADR-16**, **ADR-25** y **ADR-27**.

Como `services/Ascenso.S4.Becas` todavía no tiene código, este cambio también inicializa el **esqueleto mínimo compartido** del servicio S4 y del agregado `Beca` (`DDD_v3.3.md §11b` y `§15.2`) únicamente para establecer la estructura y la relación de datos necesarias para RF-18, sin implementar lógica de negocio ni endpoints de otros RF (en especial los asignados a otro integrante).

## What Changes

- Inicializar la estructura base de `services/Ascenso.S4.Becas/` (`Domain`, `Application`, `Infrastructure`, `Api` y `tests/{Unit,Integration,Architecture}`) y registrarla en `Ascenso.sln`.
- Definir el esqueleto mínimo compartido del agregado `Beca` (`Version` en raíz de agregado, propiedades base, `TramoBeca`, `EstadoBeca`, `Alcance`, `ModalidadPago`, `OrigenTramo`) y la regla única `CuentaComoActiva` (`Otorgada`, `Activa`, `Suspendida`, `Renovada` = activas; `Inactiva`, `Concluida`, `Revocada` = no activas), sin métodos de transición de ciclo de vida ni endpoints ajenos a RF-18.
- Implementar la consulta de solo lectura del historial de becas por cédula de persona, por programa o por periodo en S4, garantizando que no se exponga información socioeconómica y que becarios y postulantes queden restringidos según su rol.
- Agregar en `contracts/openapi.yaml` la ruta pública `GET /api/v1/becas/historial` con sus esquemas y respuestas tipadas.

## Capabilities

### New Capabilities
- `becas`: Consulta del historial de becas de una persona (por cédula), por programa o por periodo en S4 (`RF-18`, `HU-18.1`), incluyendo clasificación de estados que cuentan como beca activa (`S6`), confidencialidad socioeconómica y restricciones de acceso por rol.

### Modified Capabilities
<!-- Ninguna capacidad existente en openspec/specs/ cambia sus requisitos. -->

## Impact

- **Servicios afectados:** únicamente **S4** (`services/Ascenso.S4.Becas/`). No se modifican `S1`, `S2`, `S3`, `S5` ni `S6`.
- **Bloques compartidos:** **no** se modifican `Ascenso.SharedKernel`, `Ascenso.Contracts` ni `Ascenso.Messaging.Outbox`.
- **Solución y paquetes:** registro de los nuevos proyectos de S4 en `Ascenso.sln` y, de requerirse paquetes de ASP.NET Core / MediatR / pruebas arquitectónicas al implementar, declaración centralizada en `Directory.Packages.props`.
- **Contratos:** adición de `GET /api/v1/becas/historial` y sus esquemas en `contracts/openapi.yaml` durante la implementación.

