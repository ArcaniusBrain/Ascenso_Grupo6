## Qué cambia

<!-- Una o dos frases claras sobre el objetivo del PR. -->

## Referencias

<!-- Obligatorio: citar al menos una HU, RF, ADR, regla del cliente o cambio de OpenSpec. Ej.: HU-15.1, RF-35, ADR-25 -->

- 

## Tipo

- [ ] Funcionalidad
- [ ] Corrección
- [ ] Documentación (HU, DDD, ADR)
- [ ] Pruebas
- [ ] Configuración / CI

## Cómo se probó

<!-- Comandos ejecutados (dotnet test, openspec validate --all --strict, pruebas de integración o pasos manuales). -->

## Lista de control del autor

- [ ] La rama sale de `main` actualizada y tiene el nombre del cambio OpenSpec.
- [ ] El cambio OpenSpec está validado (`openspec validate --all --strict`) y archivado antes de fusionar.
- [ ] Las reglas de negocio están en `Domain` (sin EF Core, ASP.NET, MassTransit ni Firebase); handlers delgados.
- [ ] No se lee ni se escribe el schema PostgreSQL de otro servicio (`ADR-05`).
- [ ] Toda raíz modificada incrementa su `Version` explícita (`ADR-25`) y toda decisión deja `RegistroDecision` atómico (`RF-35`, `ADR-12`).
- [ ] Si se modificó el modelo persistido, incluye migración EF Core; si se modificó un evento o ruta, se actualizó `contracts/` y el catálogo CSV.
- [ ] Si se modificó `frontend/`, respeta el sistema de diseño de Figma y `ADR-28` (cookies `HttpOnly`, nunca JWT en `localStorage`).
- [ ] Si se tocó `building-blocks/`, `contracts/` o `Directory.Packages.props`, cuenta con acuerdo del equipo (`MediatR` fijado `< 13.0.0`, `ADR-27`).
- [ ] Hay pruebas para lo nuevo y todas pasan en verde; no hay secretos ni archivos locales en el cambio.

## Lista rápida para el revisor (antes de dar Approve)

- [ ] Checks de CI en verde (`Compilar y probar solución .NET 10`, `Validar specs y cambios`, `Guardrails de PR`).
- [ ] El diff corresponde únicamente al alcance del cambio y no introduce cambios no acordados en carpetas ajenas.
- [ ] Las reglas de negocio coinciden con las HU Plan B / Ronda 5 y `DDD_v3.3.md` sin valores inventados.
- [ ] Todas las conversaciones abiertas del PR están resueltas.
