# ADR-16 · Concurrencia optimista en todas las raíces de agregado

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

El RNF de consistencia bajo concurrencia da el ejemplo de finanzas confirmando un desembolso mientras se revoca la misma beca. En el DDD, cada raíz de agregado tiene una versión que se compara al guardar (lámina 31).

## Decisión

- La versión de cada raíz se mapea a la columna de sistema de PostgreSQL que cambia con cada actualización. Esto aplica a **todas** las raíces, no solo a dos.
- **Casos críticos que se prueban:** PresupuestoPrograma (regla 8, HU-12.1); Beca (pago contra revocación, HU-20.1 y HU-22.1); BeneficiosPersona (C2, D-3); CalendarioAcademico (un solo periodo actual, HU-38.2); Postulacion (mínimo de dos evaluadores y consolidación idempotente, HU-06.2 y HU-09.1); Oferta (expiración contra aceptación, HU-15.2); Consulta (HU-32.2).
- **Ante un conflicto:** si lo provocó un usuario, recibe un mensaje claro para reintentar; si lo provocó un proceso programado o un consumidor, se reintenta automáticamente con el agregado recargado.
- Los contadores de la base de lectura se incrementan dentro de la base, en una sola operación (HU-40.1).

## Corrige el borrador anterior

Usaba «ProgramaBeca», un nombre que no existe en el lenguaje ubicuo (el agregado es PresupuestoPrograma), y protegía solo dos entidades.

## Fuentes

RNF de consistencia; Clase 8, lámina 31; DDD v3.2, sección 5, D-1 y D-3; Anexo A de las HU citadas.
