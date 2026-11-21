# ADR-23 · Calidad y gobierno: OpenSpec, pruebas y CI

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Obligatoria / Propuesta

## Contexto

OpenSpec es obligatorio: sin OpenSpec la nota del proyecto es cero (clase del 14-sep). La evaluación individual se hace por el historial de commits en GitHub. En la clase del 24-sep el profesor pidió escribir en el documento de arquitectura una política de cobertura del 90 %.

## Decisión

- **Un solo repositorio (monorepo)** con la estructura de OpenSpec: especificaciones base por capacidad y cambios (propuestas) archivados por fecha. Este documento es el documento de arquitectura y cada ADR es un archivo propio.
- **Pruebas:** 90 % de cobertura unitaria en Domain y Application; pruebas de integración por servicio con PostgreSQL y RabbitMQ reales en contenedores; pruebas de concurrencia para los casos del ADR-16; prueba de carga del reporte consolidado y de los periodos críticos (HU-34.1); verificación de los contratos de eventos.
- El script de pruebas del profesor (el camino mínimo) se usa como prueba de aceptación.
- **Integración continua en GitHub:** en cada pull request se compila, se ejecutan las pruebas y se mide la cobertura. La revisión de cada pull request es obligatoria (clase del 28-sep).

## Fuentes

Clases del 14-sep, 21-sep, 24-sep y 28-sep; Anexo A de HU-34.1.
