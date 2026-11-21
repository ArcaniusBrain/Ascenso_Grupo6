# ADR-24 · Versiones y licencias

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

Varias librerías del ecosistema .NET cambiaron de licencia en 2025, y la versión de .NET 8 pierde soporte este mes.

## Decisión

| Componente | Versión propuesta | Motivo |
|---|---|---|
| .NET y ASP.NET Core | .NET 10 (LTS) | .NET 8 y .NET 9 dejan de tener soporte el 10-nov-2026; .NET 10 tiene soporte hasta noviembre de 2028. |
| MassTransit | 8.x | La versión 9 es comercial; la 8 es open source (Apache 2.0) con correcciones al menos hasta fines de 2026. |
| MediatR | Edición Community (13 o posterior) o 12.5 | Desde la 13 tiene doble licencia (RPL-1.5 o comercial), con edición Community gratuita para uso educativo; la 12.5 es la última con licencia Apache 2.0. |
| Stateless, Hangfire, YARP, FluentValidation, proveedor de PostgreSQL | Última versión estable | Verificar la licencia de cada paquete al fijar las versiones. |
| Keycloak, RabbitMQ, PostgreSQL | Última versión estable en contenedor | Fijar la etiqueta exacta de la imagen; nunca «latest». |
| React y Next.js | Versión estable vigente | La define el equipo de frontend. |

- Las versiones se fijan exactas y en un archivo central de paquetes, sin rangos.

## Fuentes

Anuncios oficiales de Microsoft (.NET), de MassTransit y de Lucky Penny Software (MediatR).
