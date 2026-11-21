# ADR-11 · Autorización fina por recurso y por datos

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

La HU-37.1 pide tipos de usuario y permisos configurables, sin cambiar el código para agregar uno nuevo, con un punto único de aplicación por servicio y condiciones según el estado de la entidad. Varias HU exigen que el filtro se aplique en la consulta de datos y no solo en la interfaz.

## Decisión: tres niveles

- **1. Autenticación y rol general** en Keycloak.
- **2. Permisos configurables** en S1: un catálogo de tipos de usuario y una matriz de permisos guardados como datos. El catálogo de permisos es un artefacto único y versionado en el repositorio, para que dos áreas no nombren distinto el mismo permiso.
- **3. Autorización por recurso** en cada servicio, aplicada en un punto central (políticas de ASP.NET Core por recurso) y evaluando condiciones del negocio:
  - Un coordinador solo opera sobre sus programas (HU-07.1).
  - Un enlace solo ve su universidad (HU-24.2).
  - Un evaluador solo ve sus asignaciones vigentes y sin conflicto declarado; el acceso se calcula al abrir, no se guarda como permiso (HU-06.2 y HU-39.1).
  - Las puntuaciones de otros evaluadores se ocultan hasta el cierre del periodo de revisión (HU-08.2).
  - Un becario solo ve lo suyo (HU-24.1, HU-26.1 y HU-28.1).
  - Los campos sensibles se enmascaran en la respuesta del servidor (HU-37.1).

## Fuentes

RF-37; Anexo A de HU-06.2, HU-07.1, HU-08.2, HU-10.1, HU-24.2, HU-37.1 y HU-39.1; RNF de confidencialidad.
