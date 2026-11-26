# Design

## Context

Véase `proposal.md` para la motivación y el alcance. Actualmente `services/Ascenso.S4.Becas/` contiene únicamente su `README.md` y no cuenta con proyectos `.csproj` ni código fuente. Mientras tanto, los bloques transversales (`Ascenso.SharedKernel`, `Ascenso.Contracts`, `Ascenso.Messaging.Outbox`) ya están implementados y compilados en `building-blocks/`.

Las restricciones arquitectónicas vigentes (`ADR-02`, `ADR-08`, `ADR-12`, `ADR-16`, `ADR-25`, `ADR-27`, `DDD_v3.3.md §11b` y `§15.2`) exigen:
- Separación estricta en 4 proyectos (`Domain`, `Application`, `Infrastructure`, `Api`) más `tests/{Unit,Integration,Architecture}`.
- Aislamiento completo del schema PostgreSQL `s4` (ningún servicio lee ni escribe schemas de S2 o S3).
- Definición única de estados que cuentan como beca activa (`S6`: `Otorgada`, `Activa`, `Suspendida`, `Renovada`; excluyendo `Inactiva`, `Concluida` y `Revocada`) compartida con `HU-16.1` y `HU-01.2`.
- Como el contrato completo de transición de `Beca` debe coordinarse con otro integrante que comparte el agregado, este cambio introduce únicamente el esqueleto mínimo compartido de `Beca` y la consulta de solo lectura de RF-18.

## Goals / Non-Goals

**Goals:**
- Crear la estructura base de `services/Ascenso.S4.Becas/` registrada en `Ascenso.sln`, con pruebas de arquitectura que garanticen la regla de dependencias de Clean Architecture.
- Establecer el esqueleto mínimo del agregado `Beca` (`RaizDeAgregado<Guid>` con `Version: int`), `TramoBeca` (`Entidad<Guid>`), las enumeraciones `EstadoBeca`, `Alcance`, `ModalidadPago`, `OrigenTramo` y la política única `PoliticaEstadoBeca.CuentaComoActiva(EstadoBeca)`.
- Habilitar la consulta transaccionalmente consistente y de solo lectura del historial de becas por cédula de persona, por programa o por periodo académico en S4 (`IConsultaHistorialBecas`), incluyendo autorización por rol y exclusión total de datos socioeconómicos.
- Documentar en `contracts/openapi.yaml` la nueva ruta pública `GET /api/v1/becas/historial`.

**Non-Goals:**
- Implementar métodos de transición de estado o endpoints de ciclo de vida de `Beca` (`Suspender`, `Reanudar`, `SolicitarPausa`, `AprobarPausa`, `SolicitarRenovacion`, `AprobarRenovacion`, `InactivarPorNoRenovacion`, `Revocar`, `Concluir`, `FirmarCompromiso`, `ProgramarDesembolsos`, `RegistrarPago`) ni la máquina de estados `Stateless` completa.
- Implementar el endpoint interno `GET /internal/v1/beneficios-persona/{personaId}/vigentes` (`HU-01.2`, que corresponde a `BeneficiosPersona` en RF-16) ni la línea de tiempo detallada `HU-28.1` (RF-28).
- Implementar la proyección CQRS en MongoDB de S6 (`historial_becas` como lectura de reportería); por decisión **C1** (`pendientes-v1.3.md`), RF-18 es dueño S4 porque alimenta flujos transaccionales (`HU-16.1`).
- Modificar `Ascenso.SharedKernel`, `Ascenso.Contracts`, `Ascenso.Messaging.Outbox` o cualquier otro microservicio (`S1`–`S3`, `S5`, `S6`).

## Decisions

### 1. Esqueleto compartido mínimo de `Beca` y `PoliticaEstadoBeca`
- **Decisión:** Modelar `Beca` en `Ascenso.S4.Becas.Domain` heredando de `RaizDeAgregado<Guid>` (`Ascenso.SharedKernel.TiposBase`) con `Version: int` (`ADR-16`, `ADR-25`), propiedades base (`PostulanteId: Guid`, `ProgramaId: Guid`, `OfertaId: Guid`, `Estado: EstadoBeca`, `Alcance: Alcance`, `Modalidad: ModalidadPago`), colección de `TramoBeca(Id: Guid, Periodo: CodigoPeriodo, MontoAprobado: Dinero, Origen: OrigenTramo)` y `PoliticaEstadoBeca.CuentaComoActiva(EstadoBeca)`. Para permitir pruebas e integración sin acoplar todavía constructores de negocio futuros, se expone un método de fábrica/construcción interno o constructor de hidratación limpio sin lógica de otros RF.
- **Alternativa considerada:** Dejar `Beca` con toda su máquina de estados (`Stateless`, `ADR-08`) desde este cambio. **Descartada** porque las transiciones de ciclo de vida pertenecen a otros RF (de Sebas y otros RF de Leonel) y aún no se ha cerrado el acuerdo de métodos compartidos.

### 2. Aislamiento de schemas para búsqueda por cédula y nombres (`referencia_personas` y `referencia_programas`)
- **Decisión:** Como `Beca` en `DDD_v3.3.md §11b` solo almacena `PostulanteId` y `ProgramaId`, pero `HU-18.1` exige buscar por `Cedula` (`Ascenso.SharedKernel.ValueObjects.Cedula`), distinguir *"persona inexistente"* de *"persona sin becas"* (mostrando su nombre completo) y mostrar el nombre del programa sin leer las bases de S2 ni S3, S4 mantendrá en su propio schema `s4` dos tablas de referencia de solo lectura local:
  - `referencia_personas (PersonaId, Cedula, NombreCompleto)` — **nunca** almacena ingreso familiar ni composición familiar (garantiza el Escenario 7 por diseño).
  - `referencia_programas (ProgramaId, Nombre, Alcance)`.
  Estas tablas se consultan con `AsNoTracking` desde `ConsultaHistorialBecasEfCore` (implementación de `IConsultaHistorialBecas`) y en las pruebas de este incremento se siembran directamente (las suscripciones a eventos de S2/S3 pertenecen a sus respectivos RF).
- **Alternativa considerada:** Hacer llamadas HTTP síncronas desde S4 hacia S2 y S3 durante cada consulta de historial. **Descartada** porque viola la lista cerrada de llamadas síncronas entre servicios (`ADR-02`, `v1.3 §3.1`).

### 3. Infraestructura preparada para `ADR-12` (Outbox/Inbox y `RegistroDecision`) sin generar registros en RF-18
- **Decisión:** Configurar `BecaDbContext` en `Ascenso.S4.Becas.Infrastructure` apuntando al schema `s4`, registrando `OutboxMessage`, `InboxMessage`, `RegistroDecision` y `AppendOnlyRegistroDecisionInterceptor` (`Ascenso.Messaging.Outbox`), de modo que el servicio quede listo para todos los RF siguientes de S4. En ejecución de RF-18, `ConsultarHistorialBecasHandler` (MediatR `12.5.0`, `ADR-27`) realiza únicamente consultas de solo lectura (`AsNoTracking`) y no genera registros en `RegistroDecision`, tal como exige `HU-18.1`.
- **Alternativa considerada:** Postergar Outbox/Inbox y `RegistroDecision` en `BecaDbContext` hasta RF-15/RF-16. **Descartada** para evitar rehacer la migración inicial base del schema `s4` cuando otros RF entren en paralelo.

### 4. Autorización por rol en dominio/aplicación y contrato HTTP público
- **Decisión:** Encapsular en `PoliticaAccesoHistorialBecas` (capa `Domain`) la verificación de alcance:
  - `Coordinador`: autorizado a consultar por cédula (cualquier persona), por `programaId` o por `periodo`.
  - `Becario`: autorizado únicamente a consultar su propia cédula; cualquier consulta sobre otra cédula o búsqueda agregada (`programaId` o `periodo`) produce denegación de acceso (`403 Forbidden`).
  - `Postulante`: denegado en búsqueda agregada (`programaId` o `periodo`, `403 Forbidden`).
  - Exponer en `Ascenso.S4.Becas.Api` el endpoint `GET /api/v1/becas/historial` con exactamente uno de los criterios (`cedula`, `programaId`, `periodo`), devolviendo `HistorialBecasResponse` (`items`, `mensaje`) o `ProblemDetails` (`400`, `401`, `403`, `404`), y registrarlo en `contracts/openapi.yaml` (no existen cambios a `openapi-interno.yaml` ni nuevos eventos de integración en `Ascenso.Contracts`).
- **Alternativa considerada:** Tres endpoints separados (`/por-persona`, `/por-programa`, `/por-periodo`). **Descartada** para mantener una interfaz uniforme coherente con la historia `HU-18.1` (filtros intercambiables sobre el mismo historial).

## Risks / Trade-offs

- **[Cambios futuros al agregar transiciones a `Beca` con Sebas]** → Mantener el esqueleto reducido estrictamente a `EstadoBeca`, propiedades base de `DDD_v3.3 §11b`, `TramoBeca` y `PoliticaEstadoBeca`, evitando supuestos sobre firmas de métodos de otros RF.
- **[Tablas locales `referencia_personas` y `referencia_programas` sin consumidores de eventos todavía]** → Documentar claramente que la población reactiva desde eventos de S2/S3 se conectará en los RF correspondientes (o mediante migraciones/consumidores dedicados), usando datos sembrados en pruebas de integración.
- **[Acceso de Dirección, Finanzas y Evaluadores sin definir en PRD]** → No asumir permisos no especificados; limitar los roles autorizados en código y spec a los cubiertos explícitamente por `HU-18.1` (`Coordinador`, límite de `Becario` y `Postulante`), dejando lo demás como pregunta abierta.

## Open Questions

Preguntas abiertas vigentes en `docs/requisitos/historias-de-usuario/anexo-b-preguntas-abiertas.md` (`HU-18.1`, pendientes de la pregunta 2 del cliente):
- **Anexo B — `HU-18.1` #1:** ¿Dirección o el equipo financiero necesitan acceso directo a este historial consolidado, o lo reciben únicamente a través de los reportes de RF-34 y RF-23?
- **Anexo B — `HU-18.1` #2:** ¿Un evaluador del comité necesita consultar alguna vez el historial de becas de un postulante (solo de postulaciones asignadas), o esa consulta es exclusiva de coordinador, finanzas y dirección?
- **Anexo B — `HU-18.1` #3:** Si la historia resulta extensa en UI, se mantiene en backend un único recurso con tres filtros (`persona`, `programa`, `periodo`) tal como se diseña aquí.

