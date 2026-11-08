# ADR-07 · CQRS: base de lectura en PostgreSQL, alimentada por eventos

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Decidida / Propuesta

## Contexto

El DDD v3.2 (D-14) proponía una base de consulta NoSQL. El equipo restringe la tecnología a PostgreSQL y Firebase, y la HU-32.2 impide guardar estado en Firebase. Las RF-34 y RF-40 exigen que los reportes y el panel no compitan con la operación diaria; el Anexo A de HU-34.1 acepta un «almacén de reportes» separado y el de HU-40.1 pide contadores atómicos actualizados por eventos.

## Decisión: dos niveles de lectura

- **Nivel 1, consultas operativas de un servicio sobre sus propios datos** (mis postulaciones, detalle de una beca, ranking del ciclo): handlers de consulta separados de los comandos (CQRS lógico), de solo lectura, sobre el propio schema y con índices. El ranking es una acción puntual con una consulta ordenada e indexada (Anexo A de HU-09.2).
- **Nivel 2, reportes, panel, línea de tiempo, registro de decisiones, impacto al donante,** **estado financiero y consolidado institucional:** una **base de lectura PostgreSQL aparte**, propiedad de S6, que solo escriben los proyectores al consumir eventos de integración. Sus tablas son desnormalizadas, los contadores se incrementan dentro de la base, cada proyector registra el evento ya procesado en la misma transacción (para no contarlo dos veces) y los datos para donantes se guardan ya anonimizados (HU-33.1).
- **Prohibido:** vistas materializadas o consultas que lean los schemas de otros servicios, porque romperían la regla de ADR-02.

## Alternativas descartadas

- MongoDB u otra NoSQL: fuera de la restricción tecnológica del equipo.
- Firestore: la HU-32.2 prohíbe guardar estado en Firebase.
- Vistas materializadas sobre los schemas de los servicios (borrador anterior): equivalen a cruzar datos entre servicios.

## Consecuencias

- Los reportes reflejan cada cambio unos segundos después (consistencia eventual). Las reglas de negocio nunca se deciden con la base de lectura.
- Si más adelante se habilita otro motor, solo cambia la infraestructura de S6: las interfaces de consulta viven en Application.
- Hay que actualizar D-14 y la sección 18 del DDD (sección 5 de este documento).

## Fuentes

DDD v3.2, D-10, D-14 y sección 18; clase del 21-sep (CQRS); Anexo A de HU-09.2, HU-23.1, HU-33.1, HU-34.1 y HU-40.1.
