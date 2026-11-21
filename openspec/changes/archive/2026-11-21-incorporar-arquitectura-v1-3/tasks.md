# Tasks

## 1. Documentación

- [x] 1.1 Copiar el kit v1.3 a `docs/arquitectura/v1.3/` sin la presentación de clase, el PRD en Word ni los duplicados; verificar que el documento maestro y sus diagramas se abren
- [x] 1.2 Mover la v0.1 a `docs/arquitectura/historico/v0.1/` y escribir el índice nuevo de `docs/arquitectura/`; verificar los enlaces del índice
- [x] 1.3 Llevar la adenda DDD v3.3 a `docs/dominio/` sin modificar `DDD_v3.2.md`; verificar con comparación de archivos
- [x] 1.4 Escribir `docs/arquitectura/pendientes-v1.3.md` con preguntas y decisiones abiertas; verificar contra la §16 de la v1.3

## 2. Estructura y configuración

- [x] 2.1 Reemplazar `src/` por `services/`, `building-blocks/`, `gateway/` y `contracts/` según la v1.3 §12; verificar que `src/` ya no existe
- [x] 2.2 Alinear `openspec/config.yaml` con la v1.3 (fuentes, MongoDB, llamadas síncronas, estructura, contratos); verificar con `openspec instructions proposal --change corregir-ddd-v3-3`
- [x] 2.3 Retirar `actualizar-ddd-v3-3-y-adr-v0-2` y crear `corregir-ddd-v3-3`; verificar con `openspec list`
- [x] 2.4 Actualizar README, CONTRIBUTING, guías, `CODEOWNERS`, `deploy/` y `frontend/`; verificar que ningún enlace relativo apunta a un archivo inexistente
- [x] 2.5 Ejecutar `openspec validate --all --strict`; verificar que no hay errores
