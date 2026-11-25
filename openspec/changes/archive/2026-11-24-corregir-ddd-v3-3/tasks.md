# Tasks

Detalle de cada ID en `docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`.

## 1. Preparación

- [x] 1.1 Registrar las respuestas del cliente como Ronda 5 en `docs/requisitos/respuestas_cliente.md` y marcar lo respondido en `docs/arquitectura/pendientes-v1.3.md`; verificado: Ronda 5 = R31–R44; solo B5 queda por confirmar
- [x] 1.2 Copiar `docs/dominio/DDD_v3.2.md` a `DDD_v3.3.md` y agregar la fila 3.3 al control de versiones; verificar que la v3.2 queda intacta

## 2. Correcciones (bloqueante primero)

- [x] 2.1 DDD-07: versión explícita en `RaizDeAgregado` que sube con cualquier cambio del agregado, e invariante transversal en 16.1; verificar en §5 y 16.1
- [x] 2.2 Adenda S2/S4: incorporar la ubicación distribuida en §3.1 y en el Context Map (3a, 3.1), sin cambiar agregados; verificar contra `DDD_v3.3_adenda_S2_S4.md`
- [x] 2.3 DDD-14: Trazabilidad deja de ser intermediaria hacia Comunicaciones y Reportería en el Context Map; verificar en 3a y 3.1
- [x] 2.4 DDD-08: un solo disparador de Activa → *Inactiva*: el proceso programado en la fecha de cierre (A8, R38); quitarlo de `IniciarNuevoPeriodo()` (15.2, 16.2, 16.4e); verificar que la transición aparece una sola vez
- [x] 2.5 DDD-09: estado `Aplazado` del desembolso (15.4, 11b, 6.2c, 16.1), y la pausa según la respuesta del cliente; verificar que el diagrama de estados se dibuja
- [x] 2.6 DDD-10: `IServicioOtp` con la firma de la v1.3 §8.2 (3b, 13, 16.4d, 18.1); verificar que no queda `Validar(usuarioId, codigo)`
- [x] 2.7 DDD-11: flujo 16.4f de registro y evento `UsuarioRegistrado` en 16.2; verificar que la secuencia se dibuja
- [x] 2.8 DDD-12 y DDD-13: bitácora de lectura de `DatosPago` y número de cuenta cifrado, alineados con la v1.3 §8.3; verificar en 11a y 6.2c
- [x] 2.9 Respuestas de la Ronda 5 que cambian reglas (R31–R41, R44): monto de renovación al enviar (A1), reserva por toda la beca (A2), sin revocar desde *Otorgada*/*Renovada* (A3), *Rechazada* solo manual (A4), consentimiento por donante e irrevocable (A6), `EstadoMatricula` como texto original + clasificación interna (A7, según C10), consecuencias progresivas (B3); verificar que §20 ya no las lista como pendientes
- [x] 2.10 DDD-15: referencias "14.2" y "16.4b", totales de 6.3 y registro de cambios 3.3; verificar que los totales coinciden con lo dibujado

## 3. Cierre

- [x] 3.1 Regenerar diagramas y enlaces con `python tools/mermaid/generar_diagramas.py docs/dominio/DDD_v3.3.md`; verificar que todos los `.mmd` se dibujan
- [x] 3.2 Actualizar las referencias a `DDD_v3.2.md` en `openspec/config.yaml`, README y `docs/README.md`; verificar con búsqueda de `DDD_v3.2`
- [x] 3.3 En B1, B2 y B6 manda la v1.3 (decisión del equipo): rechazo si no se puede comprobar el doble beneficio, código de estudiante traducido por S5, consulta sin postulación obligatoria; verificar que el DDD v3.3 no dice lo contrario
- [x] 3.4 Ejecutar `openspec validate --all --strict`; verificar que no hay errores

## Workflow follow-up

- Pull request `docs/corregir-ddd-v3-3` revisado por todo el equipo (toca el Shared Kernel en diseño).
- Archivar con `openspec archive corregir-ddd-v3-3 -y` dentro del mismo pull request.
