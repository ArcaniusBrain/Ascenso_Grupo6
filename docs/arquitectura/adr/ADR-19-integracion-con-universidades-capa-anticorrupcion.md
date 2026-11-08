# ADR-19 · Integración con universidades (capa anticorrupción)

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

C5 amplía el alcance: el promedio y la información académica pueden llegar por API desde la universidad o por registro manual. El convenio de cada universidad indica la vía (HU-14.1). En la clase del 21-sep el profesor pidió simular las APIs de terceros.

## Decisión

- **Entrada:** S5 expone una API para que las universidades envíen reportes académicos. Cada universidad se autentica con su propio cliente (ADR-09), solo puede escribir datos de su universidad y los lotes se procesan de forma asíncrona, paginada e idempotente (HU-24.3).
- **Consulta:** una interfaz de dominio de fuente académica con tres implementaciones (API, registro manual y simulada), cada una con su traductor (capa anticorrupción, Clase 8, lámina 18). S5 la usa para los reportes y S3 para el promedio del postulante (HU-04.1).
- Las APIs universitarias se simulan para el desarrollo y la demostración.

## Pendiente

La prioridad entre fuentes cuando llegan datos por más de una vía (HU-24.2) y el mecanismo de autenticación que acepten las universidades (HU-24.3).

## Fuentes

C5; DDD v3.2, secciones 3b y 3.1; Anexo A de HU-14.1, HU-24.2 y HU-24.3; clase del 21-sep.
