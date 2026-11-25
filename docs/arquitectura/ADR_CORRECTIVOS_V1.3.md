# ADRs Correctivos Post-v1.3 (ADR-25 a ADR-30)

**Fecha:** 24 de noviembre de 2026  
**Estado:** Adoptados para diseño e inicio de implementación (Grupo 6). Complementan `v1.3/01_DOCUMENTO_MAESTRO/ARQUITECTURA_V1.0.md` (ADR-01 a ADR-24) y se alinean con [`DDD_v3.3.md`](../dominio/DDD_v3.3.md) sin alterar los PDF históricos entregados en la v1.3.

---

## ADR-25 · Versión explícita en cada raíz de agregado (C3 / DDD-07 / ADR-B01)

- **Contexto:** La v1.3 (§7.1, ADR-16) exige concurrencia optimista por raíz, pero mapearla a la columna de sistema `xmin` de PostgreSQL deja sin protección el caso de concurrencia exigido en el PRD (*finanzas confirmando un desembolso mientras se revoca la misma beca*), porque `xmin` solo cambia si se modifica la fila de la tabla raíz, no cuando cambia una fila hija (`Pago`, `Suspension`).
- **Decisión:**
  1. `RaizDeAgregado<TId>` en `Ascenso.SharedKernel` declara una propiedad explícita `Version: int` mapeada como columna persistida y token de concurrencia en EF Core (`IsConcurrencyToken()`).
  2. Toda operación que modifique el estado del agregado —incluidas modificaciones exclusivas sobre entidades internas— invoca `IncrementarVersion()` en la raíz.
  3. EF Core valida `WHERE Id = @id AND Version = @versionEsperada` y actualiza `Version = @nuevaVersion`; en colisión lanza `DbUpdateConcurrencyException`, traducido a HTTP `409 Conflict`.
- **Verificación obligatoria:** prueba de integración paralela con barrera donde dos transacciones modifican hijos distintos de la misma `Beca` (por ejemplo `Pago` y `Suspension` o `Revocar`); exactamente una confirma y la otra recibe conflicto.

---

## ADR-26 · Conciliación segura S2 → S4 y resiliencia de llamadas síncronas (C5 / ADR-B02 / ADR-B05)

- **Contexto:** Si el evento `OfertaEmitida` o `RenovacionSolicitada` se retrasa (Outbox o RabbitMQ), liberar una reserva de S2 únicamente por vencimiento de tiempo podría sobregirar el presupuesto de un programa. Además, las llamadas síncronas entre servicios necesitan límites estrictos para evitar agotamiento de hilos.
- **Decisión:**
  1. Se incorpora la llamada privada `GET /internal/v1/reservas/{reservaId}/vinculo` (S2 → S4, scope `ofertas.consultar-vinculo`) a la lista cerrada de llamadas síncronas interservicios (`openapi-interno.yaml` v1.3).
  2. **Plazo de gracia:** el trabajo programado de conciliación en S2 evalúa únicamente reservas en estado `RESERVADA` con más de **30 minutos** de antigüedad (margen holgado sobre el retraso normal del Outbox).
  3. **Falla segura (fail closed):** S2 solo libera la reserva si S4 responde HTTP `200` con `existeVinculo = false`. Si S4 responde `existeVinculo = true`, S2 vincula la reserva de forma idempotente; si S4 no responde o devuelve error (`5xx`/timeout), S2 **no libera** y reintenta en el siguiente ciclo.
  4. **Resiliencia HTTP:** toda llamada síncrona interservicios (`S4→S2`, `S3→S2`, `S4→S1`, `S3→S4`, `S2→S4`) usa `Microsoft.Extensions.Http.Resilience` con timeout de **3 segundos**, reintentos acotados (máximo 2) **solo para operaciones idempotentes** (GET y POST con `operationId`) y circuit breaker. Al agotarse, la operación falla de forma segura (`503 Service Unavailable`).
- **Consecuencia:** Se garantiza que un retraso de mensajería nunca libere fondos comprometidos ni degrade en cascada a los servicios llamantes.

---

## ADR-27 · Despachador de casos de uso y eventos sin riesgo de licencia (C6 / ADR-B15)

- **Contexto:** A partir de la versión 13, MediatR adoptó doble licencia comercial. El equipo necesita una decisión limpia desde el primer proyecto de `building-blocks/` para evitar deuda legal o migraciones a mitad del semestre.
- **Decisión:**
  - **Opción adoptada:** implementar un **despachador ligero propio** (~60–80 líneas en `Ascenso.SharedKernel`) con interfaces `IComando<TResultado>`, `IConsulta<TResultado>`, `IManejadorComando<TComando, TResultado>`, `IManejadorConsulta<TConsulta, TResultado>` y `IManejador<TEvento>` (coherente con los tipos base del DDD §5), registrado por reflexión/DI nativo de `Microsoft.Extensions.DependencyInjection` y pipeline behaviors simples (validación, logging, correlación).
  - **Alternativa aceptable si el equipo prefiere librería:** fijar explícitamente `MediatR` `12.5.0` (última versión Apache 2.0) en `Directory.Packages.props` con verificación en CI para prohibir upgrade a `>= 13.0.0`.
- **Consecuencia:** Cero riesgo de licencia, código mínimo y totalmente transparente para pruebas y para la defensa académica.

---

## ADR-28 · Arquitectura del Frontend Next.js y gestión de sesión BFF (C7 / ADR-B13)

- **Contexto:** La v1.3 define React + Next.js detrás de HTTPS hacia YARP, pero no cerraba el patrón de renderizado ni dónde residen los tokens OIDC de Keycloak.
- **Decisión:**
  1. **Next.js App Router** operando como **BFF (Backend for Frontend) ligero** únicamente para manejo de sesión y proxy hacia YARP — sin reglas de negocio ni acceso directo a bases de datos.
  2. **Almacenamiento de tokens:** el `access_token` y `refresh_token` de Keycloak se gestionan del lado del servidor de Next.js mediante **cookies `HttpOnly`, `Secure` y `SameSite=Lax`** (cifradas/selladas). **Prohibido guardar JWT en `localStorage` o `sessionStorage`.**
  3. **Renderizado:** Server Components para carga inicial autenticada y Client Components para interacciones ricas (formularios paso a paso, tablas, SignalR de consultas y panel).
  4. **Tiempo real (SignalR):** la conexión WebSocket a S6 pasa por YARP (`/hubs/...`) autenticada con token de corta vida emitido por el BFF; ante reconexión, el cliente reconsulta el estado autoritativo vía API.
- **Consecuencia:** Mitiga robo de tokens por XSS, mantiene la arquitectura limpia (Node.js no contiene lógica transaccional) y unifica CORS/auth detrás del mismo origen.

---

## ADR-29 · Catálogo interno y normalización de `EstadoMatricula` en S5 (C10 / A7 / R37)

- **Contexto:** En la Ronda 5 (R37 / A7), el cliente confirmó que las universidades enviarán el estado de matrícula como **texto libre** y que ASCENSO debe interpretarlo y clasificarlo correctamente.
- **Decisión:**
  1. El dominio de S5 modela `EstadoMatriculaClasificado { TextoOriginal: string, Clasificacion: EstadoMatricula }` (DDD v3.3 §12). **Siempre** se conserva el texto exacto recibido de la universidad para auditoría.
  2. Catálogo cerrado de la enumeración `EstadoMatricula`:
     - `INSCRITO`: alumno matriculado regularmente en el periodo (cumple condición de matrícula).
     - `RETIRADO`: retiro definitivo o abandono del periodo/universidad (dispara evaluación de incumplimiento).
     - `CONGELADO`: pausa o retiro temporal autorizado por la universidad (compatible con pausa de beca).
     - `EGRESADO`: finalizó la carrera / graduado (habilita conclusión de beca).
     - `NO_RECONOCIDO`: el texto recibido no coincide con las reglas de normalización vigentes.
  3. **Reglas de normalización en la ACL de S5:** normalización insensible a mayúsculas, acentos y espacios + diccionario de sinónimos configurable por universidad (por ejemplo `"activo"`, `"matriculado"`, `"regular"`, `"inscrito"` → `INSCRITO`; `"retirado"`, `"baja"`, `"desincorporado"` → `RETIRADO`; `"congelado"`, `"permiso"`, `"licencia"` → `CONGELADO`; `"egresado"`, `"graduado"` → `EGRESADO`).
  4. **Comportamiento ante `NO_RECONOCIDO`:** la fila del lote **no tumba el lote completo**; se registra con clasificación `NO_RECONOCIDO`, se informa en el detalle por fila del lote (`GET .../lotes/{loteId}`) y genera una alerta para clasificación manual del coordinador/enlace sin disparar consecuencias automáticas.
- **Consecuencia:** Cumple R37 y HU-24.3 sin acoplar S5 al vocabulario de una sola universidad ni perder información original.

---

## ADR-30 · Registro de postulante distribuido entre S1 y S3 (D2 / DDD-11 / ADR-B06)

- **Contexto:** Crear una cuenta de postulante requiere crear identidad/credenciales en S1 (Keycloak) y perfil de `Postulante` en S3, garantizando unicidad de cédula (R1).
- **Decisión:**
  1. **S1 es la fuente de verdad** de la identidad, los roles/tipos de usuario y la unicidad de `Cedula` (`IUsuarioRepository.ObtenerPorCedula`). **Keycloak** almacena exclusivamente credenciales y segundo factor; los cambios de rol se hacen en S1 y se sincronizan hacia Keycloak.
  2. Al registrarse un postulante, S1 valida cédula única, crea la cuenta en Keycloak, guarda `Usuario` + Outbox (`UsuarioRegistrado.v1`) en una transacción local y responde `202/201`.
  3. S3 consume `UsuarioRegistrado.v1` vía Inbox y crea el `Postulante` de forma **idempotente por `UsuarioId`** (DDD v3.3 flujo `16.4f`).
  4. Si el postulante inicia sesión antes de que S3 haya consumido el evento, el frontend consulta `/api/v1/postulantes/mi-perfil` y, mientras devuelva `404`/`202`, muestra el estado *"completando el registro"* con reintento corto.
- **Consecuencia:** Respeta aislamiento de schemas (`s1_identidad` vs `s3_postulacion_evaluacion`), elimina transacciones distribuidas y evita duplicidad de cédulas.
