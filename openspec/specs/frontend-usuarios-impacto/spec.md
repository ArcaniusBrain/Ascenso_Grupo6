# Especificación: Frontend Usuarios, Roles y Reportes de Impacto (RF-37 y RF-33)

## Purpose

Implementar en `frontend/` las pantallas administrativas de usuarios y tipos de usuario (`/usuarios`, RF-37), generación y visualización asíncrona de reportes de impacto anonimizados (`/reportes-impacto`, RF-33 · HU-33.1) y consentimiento irrevocable del becario por donante (`/consentimientos`, RF-33 · HU-33.2), conectadas exclusivamente vía el BFF (`ADR-28`).

## Requirements

### Requirement: Administracion de usuarios internos y tipos de usuario (RF-37)

El frontend MUST permitir listar y crear tipos de usuario con permisos y política de 2FA, editar permisos, eliminar tipos no predefinidos sin uso, crear usuarios internos y actualizar roles consumiendo `S1` a través del BFF.

#### Scenario: Cambio de roles delega validacion de auto-escalamiento al backend

- **WHEN** un administrador modifica los roles de un usuario en `/usuarios`
- **THEN** la solicitud se envía mediante `PUT /api/bff/api/v1/usuarios/{id}/roles`
- **AND** si `S1` rechaza por auto-escalamiento o falta de permisos, la interfaz muestra el mensaje `ProblemDetails` exacto

### Requirement: Reportes de impacto asincronos y anonimizados (RF-33 · HU-33.1)

El frontend MUST solicitar la generación de reportes de impacto mediante `POST /api/v1/reportes/impacto` (`202 Accepted`), reflejar el estado `PREPARANDO` hasta su paso a `LISTO` y mostrar únicamente métricas agregadas más los becarios con consentimiento explícito (nombre, cédula, universidad, carrera, programa y promedio), sin datos socioeconómicos.

#### Scenario: Solicitud de reporte muestra estado preparando y luego contenido anonimizado

- **WHEN** dirección o coordinación solicita un reporte para un donante y periodo en `/reportes-impacto`
- **THEN** el frontend muestra el aviso `"El reporte se está preparando."` mientras `S6` procesa en segundo plano
- **AND** al pasar a `LISTO`, presenta las cifras agregadas, desembolsos confirmados y la lista de becarios con consentimiento vigente

### Requirement: Consentimiento irrevocable del becario por donante (RF-33 · HU-33.2)

El frontend MUST permitir al propio becario consultar sus consentimientos vigentes y otorgar consentimiento explícito e irrevocable hacia un donante mediante `/consentimientos`, informando claramente qué datos se muestran y que nunca se comparten datos socioeconómicos (R36/A6).

#### Scenario: Confirmacion de consentimiento exige aceptacion explicita de irrevocabilidad

- **WHEN** un becario otorga consentimiento en `/consentimientos`
- **THEN** la interfaz exige marcar la confirmación explícita de irrevocabilidad antes de invocar `POST /api/bff/api/v1/consentimientos-donantes`
- **AND** lista el consentimiento registrado como irrevocable y vigente
