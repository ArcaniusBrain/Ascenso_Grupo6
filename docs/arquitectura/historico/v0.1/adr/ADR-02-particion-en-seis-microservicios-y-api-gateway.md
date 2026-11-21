# ADR-02 · Partición en seis microservicios y API Gateway

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

El DDD v3.2 define 12 Bounded Contexts. Un servicio por contexto sería demasiada infraestructura para un equipo de cinco personas. El borrador anterior proponía seis servicios, pero omitía Catálogo Institucional, que es proveedor (OHS) de cuatro contextos y debe existir antes que Programa y Ciclo (Anexo A, HU-38.1).

## Decisión

| Servicio | Contextos del DDD v3.2 | Agregados | Subdominio | Responsable (DDD 2.2) |
|---|---|---|---|---|
| S1 Identidad y Acceso | Identidad y Acceso | Usuario, TipoUsuario, ClienteApiUniversidad; servicio de OTP | Genérico | Integrante 1 |
| S2 Catálogo y Programas | Catálogo Institucional; Programas y Fondos | CalendarioAcademico, UniversidadAliada, Programa, Donante, PresupuestoPrograma | Core (Fondos) | Integrante 1 |
| S3 Postulación y Evaluación | Postulación; Evaluación y Selección (Partnership) | CicloPostulacion, Postulante, Postulacion, Comite, Evaluacion, ConflictoInteres | Core (Evaluación) | Integrantes 2 y 3 |
| S4 Gestión de Becas | Gestión de Becas | Oferta, Beca, BeneficiosPersona, DatosPago, VentanaRenovacion | Soporte | Integrantes 4 y 5 |
| S5 Seguimiento Académico | Seguimiento Académico | ReporteAcademico, PlazoReporte, EvaluacionCumplimiento | Soporte | Integrante 5 |
| S6 Comunicaciones y Reportería | Comunicaciones; Reportería e Impacto | Consulta, RegistroEnvio, ConsentimientoDonante; modelos de lectura | Soporte | Compartido: definir responsable |

- **No son servicios, son componentes compartidos:** Trazabilidad (RegistroDecision se escribe dentro de cada servicio, ADR-12), Archivos (adaptador de Firebase, ADR-06) y Envío de mensajes (adaptador de canal, ADR-20).
- **API Gateway con YARP:** enruta por prefijo de servicio, valida el token y deja pasar WebSockets. Cada servicio vuelve a validar el token y aplica su propia autorización; no confía solo en el gateway.
- **Datos de otro servicio:** se replican por eventos como copia local de solo lectura (por ejemplo, la configuración del programa en S3). La excepción es el presupuesto disponible, que siempre se consulta en vivo a S2 porque es la fuente única de disponibilidad (Anexo A, HU-09.2 y HU-12.1).
- **Lista cerrada de llamadas síncronas** (agregar una requiere un ADR nuevo): S4 a S2 para reservar y liberar presupuesto (ofertas y renovaciones); S3 a S2 para consultar la disponibilidad al generar el ranking y al confirmar un caso límite; cualquier servicio a S1 para validar un OTP.
- **docker-compose** levanta el gateway, los seis servicios, Keycloak, RabbitMQ, PostgreSQL (base transaccional y base de lectura), un servidor de correo de pruebas, el emulador de Firebase y el frontend.

## Alternativas descartadas

- Un servicio por Bounded Context (12 servicios): sobrecarga operativa sin beneficio para el tamaño del equipo.
- Trazabilidad como servicio aparte: la HU-35.1 exige que el cambio de estado y su registro sean atómicos, y eso no se puede garantizar entre dos servicios.
- Postulación y Evaluación separadas: la Postulacion contiene las asignaciones de evaluadores y la regla del mínimo de dos se valida en la misma transacción (HU-06.2).

## Consecuencias

- S2 es el servicio más sensible: si no está disponible, no se emiten ofertas (falla segura).
- S6 lo comparten varios integrantes por módulos: hay que nombrar un responsable de integración.
- Queda por definir en la fase de contratos si S3 conoce las becas activas de una persona (HU-01.2) por réplica de eventos de S4 o por consulta síncrona (ver sección 6).

## Fuentes

DDD v3.2, secciones 2, 2.2, 3 y 6.1; Anexo A de HU-06.2, HU-09.2, HU-12.1 y HU-38.1.
