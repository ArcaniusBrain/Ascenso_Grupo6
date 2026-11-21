# ADR-22 · Índices de base de datos

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

La lámina propone usar índices. Varias HU piden recorridos indexados para sostener el crecimiento de 12 a más de 25 programas.

## Decisión

| Índice | Servicio | Fuente |
|---|---|---|
| Postulaciones por (ciclo, estado) y por (postulante, estado) | S3 | HU-01.1, HU-29.2 |
| Ranking por (ciclo, puntuación consolidada) | S3 | Anexo A HU-09.2 |
| Becas por (programa, código de periodo, estado) | S4 | HU-26.2, HU-38.2 |
| Ofertas por (estado, fecha límite) | S4 | HU-15.2 |
| Aportes por (donante, periodo) | S2 | Anexo A HU-13.2 |
| Plazos por (fecha de vencimiento, estado de cumplimiento) | S5, S6 | Anexo A HU-30.1 |
| RegistroDecision por (sujeto, fecha) | Todos | Anexo A HU-28.1 |
| RegistroEnvio único por (persona, hito, anticipación) | S6 | Anexo A HU-29.1 |

- Se definen en la configuración de EF Core de cada servicio y se revisan con los planes de ejecución durante las pruebas de carga.

## Corrige el borrador anterior

Usaba un identificador de periodo; el DDD referencia los periodos por CodigoPeriodo (D-5). El índice de aportes es por (donante, periodo), no por (donante, programa).
