# ADR-17 · Reloj único y fechas límite en UTC-4

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

R20: las fechas límite se evalúan en UTC-4 e incluyen el día de cierre hasta las 23:59. Varias HU prohíben usar la hora del navegador.

## Decisión

- Un reloj abstraído e inyectado (el proveedor de tiempo de .NET); el dominio nunca consulta la hora del sistema directamente.
- Toda comparación de plazos pasa por los value objects VentanaFechas y FechaLimite: una sola implementación (D-7).
- Las fechas se guardan con su zona horaria explícita y los procesos programados usan UTC-4.
- Las pruebas usan un reloj controlado para verificar los casos de las 23:59.

## Fuentes

R20; DDD v3.2, D-7; Anexo A de HU-02.2, HU-03.2, HU-08.2, HU-24.1, HU-26.1 y HU-28.1.
