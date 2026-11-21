# Adenda de integración DDD v3.3 — registro de compatibilidad, sin modificar DDD v3.2

**Estado:** propuesta documentada para incorporación formal en el DDD por el equipo; `DDD_V3.2.md` original permanece intacto.

La sección 3.1 del DDD v3.2 describe Programas y Fondos y Gestión de Becas como módulos de una misma aplicación. La arquitectura v1.1, por requerimiento de microservicios de la consigna informada, los ubica en **S2 y S4**, respectivamente.

No cambian los agregados ni sus invariantes: `PresupuestoPrograma` sigue en S2 y `Beca`, `Oferta` y `BeneficiosPersona` en S4. La reserva de presupuesto precede a la emisión y es síncrona y privada; confirmación, liberación y proyecciones se coordinan por integración idempotente con Outbox/Inbox; el reconciliador consulta el estado real antes de liberar. La excepción D-3 permanece **local a S4**.

**Cambio esperado en DDD v3.3:** actualizar únicamente el texto de ubicación y frontera distribuida en §3.1 y las dependencias técnicas del Context Map, sin modificar contenido del dominio ni prometer consistencia transaccional global entre S2 y S4. Toda modificación del DDD requiere aprobación del equipo.
