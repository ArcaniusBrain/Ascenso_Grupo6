# Tasks

Referencia de cada ID: `docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`.

## 1. Modelo de dominio v3.3 (bloqueante primero)

- [ ] 1.1 Copiar `docs/dominio/DDD_v3.2.md` a `DDD_v3.3.md` y agregar la fila 3.3 al control de versiones; verificar que el archivo nuevo existe y el v3.2 queda intacto
- [ ] 1.2 Aplicar DDD-07 (versión explícita en la raíz e invariante transversal); verificar con búsqueda de `Version` en la sección 5 y la nueva fila en 16.1
- [ ] 1.3 Aplicar DDD-01 a DDD-06 (cambios pedidos por la sección 5 del ADR); verificar que no queda la palabra `NoSQL` ni "Keycloak o Firebase Authentication"
- [ ] 1.4 Aplicar DDD-08 (un solo disparador de Activa → Inactiva) en 15.2, 16.2 y 16.4e; verificar que la transición aparece una sola vez con `IniciarNuevoPeriodo()`
- [ ] 1.5 Aplicar DDD-09 (estado `Aplazado`, R12) en 15.4, 11b, 6.2c y 16.1; verificar que el diagrama de estados del desembolso se dibuja
- [ ] 1.6 Aplicar DDD-10 (OTP con operación y recurso, `TipoOperacionSensible`) en 3b, 13, 16.4d y 18.1; verificar que no queda la firma `Validar(usuarioId, codigo)`
- [ ] 1.7 Aplicar DDD-11 (flujo 16.4f de registro y evento `UsuarioRegistrado`); verificar que el diagrama de secuencia se dibuja
- [ ] 1.8 Aplicar DDD-12 a DDD-14 (bitácora de lectura, cuenta cifrada, Context Map); verificar en los diagramas 3a, 11a y 6.2c
- [ ] 1.9 Aplicar DDD-15 (referencias "14.2" y "16.4b", totales de 6.3, registro de cambios 3.3); verificar que los totales coinciden con lo dibujado
- [ ] 1.10 Regenerar diagramas y enlaces con `python tools/mermaid/generar_diagramas.py docs/dominio/DDD_v3.3.md`; verificar que todos los `.mmd` se dibujan en mermaid.live o con `mmdc`

## 2. Arquitectura v0.2 (bloqueantes primero)

- [ ] 2.1 Aplicar ADR-B01 en ADR-16 (versión explícita y prueba de hijos concurrentes); verificar que no queda la mención a la columna de sistema
- [ ] 2.2 Aplicar ADR-B02 en ADR-03, ADR-14 y la lista cerrada del ADR-02 (consulta S2 → S4 antes de liberar); verificar que la lista cerrada tiene la nueva llamada
- [ ] 2.3 Aplicar ADR-B03 a ADR-B07 (Hangfire sin "becas no renovadas", desembolso aplazado, tiempos límite, registro de cuenta, OTP); verificar contra la lista de cambios
- [ ] 2.4 Aplicar ADR-B08 a ADR-B11 (Firebase en .NET, fechas en UTC, exportación de auditoría, bitácora de lectura); verificar contra la lista de cambios
- [ ] 2.5 Crear ADR-25 (observabilidad y operación), ADR-26 (frontend) y ADR-27 (despliegue de la demostración) con el formato de los demás; verificar que aparecen en `docs/arquitectura/adr/README.md`
- [ ] 2.6 Aplicar ADR-B15 y ADR-B16 (MediatR opcional; secciones 5 y 6 al día); verificar que la sección 5 lista DDD-01 a DDD-14
- [ ] 2.7 Cambiar "Borrador v0.1" por "Borrador v0.2" en `docs/arquitectura/README.md` y en la hoja de revisión; verificar con búsqueda de `v0.1`

## 3. Requisitos y referencias

- [ ] 3.1 Agregar al Anexo B las dos preguntas nuevas (disparador de Inactiva; R12 en la pausa); verificar que aparecen en la tabla de preguntas generales
- [ ] 3.2 Actualizar `README.md`, `docs/README.md` y `openspec/config.yaml` para que apunten a `DDD_v3.3.md`; verificar que ningún enlace apunta a un archivo inexistente
- [ ] 3.3 Ejecutar `openspec validate --all --strict` y revisar la lista de verificación (parte D) de `docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`; verificar que todas las casillas quedan marcadas

## Workflow follow-up

- Abrir el pull request `docs/actualizar-ddd-v3-3-y-adr-v0-2` y pedir revisión a todo el equipo (toca el Shared Kernel en diseño).
- Archivar el cambio con `openspec archive actualizar-ddd-v3-3-y-adr-v0-2` dentro del mismo pull request, antes de fusionarlo.
