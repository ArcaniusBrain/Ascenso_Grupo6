# ADR-01 · Estilo arquitectónico

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Obligatoria

## Contexto

La lámina del profesor fija como obligatoria una «arquitectura escalable en capas, de microservicios conectados por APIs», con frontend React + Next.js y backend .NET (C#). La lámina 15 de la Clase 8 aclara que un Bounded Context no es un microservicio por definición: primero se dibujan los contextos y después se decide cómo se despliegan.

## Decisión

- Microservicios alineados con los Bounded Contexts del DDD v3.2, agrupando en un mismo servicio los contextos que forman una sola frontera de consistencia o que son pequeños (ADR-02).
- Cada servicio, internamente, en capas (ADR-04).
- Desde el navegador se entra siempre por el API Gateway. Entre servicios se usan eventos asíncronos por defecto, y llamadas síncronas solo en una lista cerrada de casos (ADR-02 y ADR-03).

## Consecuencias

- Cada servicio tiene un dueño en el equipo y se despliega por separado.
- Costo: consistencia eventual entre servicios y más infraestructura (gateway y broker de mensajes).

## Fuentes

Lámina «Decisiones Arquitectónicas»; Clase 8, láminas 15 y 17; clase del 14-sep (microservicios y DDD).
