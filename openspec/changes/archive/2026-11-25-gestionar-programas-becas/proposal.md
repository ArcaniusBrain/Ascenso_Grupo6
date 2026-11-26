# Proposal

## Why

Los programas de becas (**RF-11**: **HU-11.1** y **HU-11.2**) son el eje central de **`S2 Catálogo y Fondos`** (`services/Ascenso.S2.CatalogoFondos/`):

- **HU-11.1** (*Crear programa con monto fijo, modalidad y alcance*): registra nombre, cupo total de becarios, duración en periodos, periodo académico ligado (`RF-38`), monto fijo por periodo (`C1`), modalidad de pago (`MENSUAL`, `SEMESTRAL`, `DIRECTO_UNIVERSIDAD`) y etiqueta `COMPLETO` / `PARCIAL` (para la regla de no doble beneficio de `HU-01.2`/`HU-16.1`/`HU-18.1`), bloqueando roles sin permiso (`RF-37`).
- **HU-11.2** (*Donantes, elegibilidad y permanencia/renovación*): asocia donantes sin duplicados mostrando presupuesto total como suma de aportes (`S8`), restringe opcionalmente a universidades aliadas y sus carreras elegibles (`RF-14`), y configura condiciones estructuradas de permanencia, duración máxima de pausa (`RF-27`) y condiciones de renovación (`RF-15`/`RF-25`/`RF-26`).
- **ADRs aplicados:** `ADR-01/02/04`, `ADR-05` (`s2_catalogo_fondos`), `ADR-12/13` (Outbox), `ADR-16/25` (`Version` explícita) y `ADR-27` (`MediatR 12.5.0`).

## What Changes

- **Dominio S2 (`Ascenso.S2.CatalogoFondos.Domain`):** raíz `Programa : RaizDeAgregado<Guid>` (`Version` explícita), enums `ModalidadPago` y `AlcancePrograma`, entidades internas `RestriccionElegibilidad` y `AsociacionDonante`, value object `CondicionesPermanencia`, evento de dominio `ProgramaCreadoEvento` e `IProgramaRepository`.
- **Aplicación S2 (`Ascenso.S2.CatalogoFondos.Application`):** `PoliticaAutorizacionProgramas`, puerto `IConsultorDonantesYAportes` (preparado para conectar directamente con `RF-13` en el siguiente incremento) y comandos/queries `MediatR 12.5.0` para crear programas, configurar monto/modalidad/alcance, asociar donantes, configurar restricciones universidad/carrera, configurar permanencia/pausa/renovación y consultar programas con su resumen de presupuesto.
- **Infraestructura S2 (`Ascenso.S2.CatalogoFondos.Infrastructure`):** tablas `programas`, `programa_restricciones_elegibilidad` y `programa_donantes` en `CatalogoFondosDbContext` (`s2_catalogo_fondos`), `ProgramaRepositoryEfCore` y actualización de `ConsultorElementosPeriodo` para contar programas reales ligados a un periodo (`HU-38.1 E5`).
- **API y contratos (`Ascenso.S2.CatalogoFondos.Api` + `contracts/openapi.yaml`):** implementa `POST /api/v1/programas` (respetando la ruta existente bajo tag `S2`) y añade las rutas complementarias de consulta y configuración de `HU-11.1` y `HU-11.2`.
- **Pruebas:** unitarias de dominio, integración HTTP + SQLite real cubriendo todos los escenarios de `HU-11.1` (E1–E11) y `HU-11.2` (E1–E14), y actualización de `ArquitecturaCatalogoFondosTests` incluyendo `Programa`.

## Capabilities

### New Capabilities

- `programas`: creación y configuración de programas de becas con monto fijo por periodo, modalidad de pago, etiqueta completo/parcial, asociación de donantes, restricciones universidad/carrera y condiciones de permanencia, pausa y renovación (`RF-11`: `HU-11.1` y `HU-11.2`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** amplía `services/Ascenso.S2.CatalogoFondos/` sin modificar `S1`, `Ascenso.SharedKernel` ni `Ascenso.Contracts`.
- **Archivos compartidos tocados:** `contracts/openapi.yaml` (tag `S2`) y `docs/arquitectura/pendientes-v1.3.md` (cierre de decisiones Anexo B `C17`).
- **Fuera de alcance en este PR:** el registro CRUD de donantes y aportes (`RF-13`, siguiente cambio en `S2`), el saldo transaccional de reservas/disponible (`RF-12`, reservado para `PresupuestoPrograma`) y la edición de rúbrica/compromiso/consecuencias de otras áreas (`HU-07.1`, `HU-17.1`, `HU-25.1`).
