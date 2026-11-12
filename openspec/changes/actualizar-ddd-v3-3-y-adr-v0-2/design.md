# Design

## Context

Ver proposal.md (Why). Hoy existen `docs/dominio/DDD_v3.2.md` y los 24 ADR en archivos separados bajo `docs/arquitectura/adr/`, más el PDF de la propuesta v0.1 como respaldo de lo entregado.

## Goals / Non-Goals

**Goals:**
- Que el DDD v3.3 y el ADR v0.2 no se contradigan entre sí ni con las HU.
- Que cada cambio aplicado se pueda rastrear a su ID de la lista de cambios.

**Non-Goals:**
- Escribir código o crear la solución .NET.
- Resolver preguntas que dependen del cliente o del profesor: se registran, no se deciden.

## Decisions

- **El DDD se versiona como archivo nuevo (`DDD_v3.3.md`) y el v3.2 se conserva.** El profesor y el PDF entregado citan la v3.2; borrarla rompería esas referencias. Alternativa descartada: editar el v3.2 en el lugar (se pierde la versión entregada salvo en el historial de git).
- **Los ADR se editan en su archivo.** El historial de git conserva la v0.1 y el PDF original queda como respaldo; un ADR por archivo hace que el diff de cada decisión se revise por separado.
- **Los diagramas se regeneran con el script, no a mano.** Así los `.mmd` y la página de enlaces nunca se separan del `.md` del DDD.
- **Un solo pull request, con commits por ID** (por ejemplo `docs(ddd): DDD-07 versión explícita en la raíz`), para que cada integrante revise la parte que le toca.

## Risks / Trade-offs

- [El profesor rechaza la partición en seis servicios o la base de lectura PostgreSQL] → Los cambios de esta propuesta siguen valiendo; solo cambiaría la tabla de servicios (DDD-04).
- [El cliente responde distinto sobre el disparador de Inactiva] → DDD-08 deja la decisión en un solo lugar, así que invertirla es un cambio pequeño.

## Open Questions

- Disparador de Activa → Inactiva (al cerrar la ventana o al iniciar el periodo): pregunta nueva al cliente.
- ¿R12 aplica igual a la pausa? (Anexo B: 12).
