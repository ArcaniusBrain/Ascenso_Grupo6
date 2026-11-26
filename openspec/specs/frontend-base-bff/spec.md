# Especificación: Base Frontend Next.js y BFF (ADR-28)

## Purpose

Establecer la base compartida del frontend de ASCENSO en Next.js App Router operando como BFF ligero (ADR-28), garantizando almacenamiento seguro de sesión en cookies HttpOnly, bloqueo estricto de fronteras privadas, sistema visual fiel a Figma y flujo inicial de acceso con verificación en dos pasos (RF-37).

## Requirements

### Requirement: Sesion BFF cifrada en cookies HttpOnly sin exposicion de tokens al navegador

El BFF de Next.js MUST almacenar los tokens de acceso exclusivamente en cookies `HttpOnly`, `SameSite=Lax` (y `Secure` en producción) cifradas con AES-256-GCM en servidor, y MUST NEVER exponer JWT ni escribir credenciales en `localStorage` o `sessionStorage` (ADR-28).

#### Scenario: Inicio de sesion exitoso sella cookie HttpOnly sin devolver accessToken al cliente

- **WHEN** un usuario autentica sus credenciales mediante `POST /api/auth/login` o completa 2FA mediante `POST /api/auth/2fa/verificar`
- **THEN** el BFF sella la cookie `ascenso_session` con `HttpOnly` y `SameSite=Lax`
- **AND** la respuesta JSON al navegador incluye solo el perfil público de sesión (`usuarioId`, `nombreCompleto`, `correo`, `roles`, `permisos`) sin el campo `accessToken`

#### Scenario: Consulta de sesion valida integridad y expiracion

- **WHEN** se consulta una cookie `ascenso_session` alterada, inválida o vencida
- **THEN** el BFF descarta la sesión y trata al cliente como no autenticado

### Requirement: Proxy BFF hacia YARP o microservicios con bloqueo estricto de fronteras privadas

El BFF MUST enrutar llamadas desde el cliente hacia `/api/v1/*` usando `ASCENSO_GATEWAY_URL` o la configuración de servicios por entorno, sin direcciones hardcodeadas, y MUST rechazar con HTTP 403 cualquier intento de alcanzar rutas `/internal/*` (ADR-28, ADR-11).

#### Scenario: Rechazo de ruta privada interservicios desde el navegador

- **WHEN** un cliente solicita al proxy BFF una ruta que contiene el segmento `/internal/` o intenta escape de ruta (`..`)
- **THEN** el BFF responde inmediatamente HTTP `403` en formato `application/problem+json` sin realizar ninguna llamada saliente

#### Scenario: Enrutamiento hacia YARP o servicio configurado inyectando identidad de sesion

- **WHEN** un usuario autenticado invoca `/api/bff/api/v1/programas`
- **THEN** el BFF resuelve el destino desde la configuración del servidor e inyecta `Authorization: Bearer` y los encabezados de actor derivados exclusivamente de la sesión `HttpOnly`

### Requirement: Sistema visual compartido y pantallas de acceso RF-37

El frontend MUST implementar los tokens de color, tipografía (`Bricolage Grotesque` e `Inter`), componentes base reutilizables, el shell autenticado con navegación filtrada por rol y las pantallas de inicio de sesión (`/login`) y verificación en dos pasos (`/verificacion`) acordes a `RF-37`.

#### Scenario: Acceso con segundo factor obligatorio redirige a verificacion

- **WHEN** personal de la Fundación, comité o enlace universitario ingresa credenciales válidas en `/login` y S1 responde `requiereSegundoFactor = true`
- **THEN** el BFF guarda el desafío pendiente en cookie `HttpOnly` de corta vida
- **AND** la interfaz redirige a `/verificacion` mostrando las 6 casillas de código, reenvío y aviso de bloqueo tras 3 intentos
