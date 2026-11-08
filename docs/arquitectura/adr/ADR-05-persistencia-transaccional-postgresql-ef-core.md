# ADR-05 · Persistencia transaccional: PostgreSQL + EF Core

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Obligatoria / Propuesta

## Contexto

La lámina del profesor fija PostgreSQL con Entity Framework Core. El equipo confirmó que solo se usarán PostgreSQL y Firebase.

## Decisión

- Un servidor PostgreSQL con una base transaccional y **un schema por servicio**.
- **Dos usuarios de base de datos por servicio:** el dueño del schema, que solo ejecuta migraciones, y el usuario de la aplicación, que solo opera sobre su propio schema (y sobre RegistroDecision únicamente puede insertar y consultar, ADR-12). Así la regla «nadie lee el schema de otro» la hace cumplir la base de datos, no solo la disciplina del equipo.
- Todas las tablas del negocio viven en PostgreSQL, incluidos los mensajes de las consultas (HU-32.1).
- El mapeo no contamina el dominio: configuración separada por entidad, atributos privados, constructores privados y value objects mapeados como tipos propios.
- Las migraciones son independientes por servicio.

## Consecuencias

- Compartir servidor es una concesión práctica para el proyecto. En producción se podría pasar a una base por servicio sin cambiar la aplicación.

## Fuentes

Lámina «Decisiones Arquitectónicas»; DDD v3.2, sección 18.1; Anexo A de HU-32.1 y HU-35.1.
