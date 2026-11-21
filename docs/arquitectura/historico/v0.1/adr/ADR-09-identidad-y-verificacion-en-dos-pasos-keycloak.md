# ADR-09 · Identidad y verificación en dos pasos: Keycloak

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta / Pendiente

## Contexto

La lámina del profesor deja como opcional elegir entre Keycloak, Active Directory o LDAP; en la clase del 21-sep el profesor recomendó Keycloak. El DDD v3.2 (D-8) dejó pendiente la elección entre Keycloak y Firebase Authentication. Por restricción del cliente, deben usar segundo factor los coordinadores, finanzas, Dirección y el comité, incluidos los evaluadores externos; R3 agrega a los enlaces de universidad. R4 fija las reglas del código.

## Decisión

- **Keycloak** en contenedor como proveedor de identidad (OpenID Connect). Identidad y Acceso adopta su modelo de tokens y roles (relación Conformist del Context Map).
- El frontend inicia sesión con el flujo estándar de Keycloak; el gateway y cada servicio validan el token.
- Qué tipos de usuario exigen segundo factor lo define la configuración de tipos de usuario, no una lista fija en el código (HU-37.2).
- Postulantes y becarios crean su propia cuenta; los usuarios internos los crea Dirección (S1).
- Cada universidad que envía reportes por API es un cliente propio de Keycloak, con credenciales de cliente distintas de las del enlace (HU-24.3).

## Alternativas descartadas

- Active Directory o LDAP: pensados para personal interno; no sirven bien a miles de postulantes externos.
- Firebase Authentication (sugerida por el Anexo A de HU-20.1): se prefiere Keycloak por la recomendación del profesor, porque se ejecuta en docker-compose sin depender de la nube y porque permite configurar roles y flujos de autenticación por tipo de usuario.

## Pendiente: prueba técnica de 1 o 2 días

R4 exige que el segundo factor llegue por correo o SMS, a elección del usuario, con vigencia de 15 minutos, 3 intentos y bloqueo de 1 hora. El segundo factor nativo de Keycloak es una aplicación autenticadora; el envío por correo o SMS puede requerir una extensión. Si la prueba falla, el plan B es que Keycloak valide la contraseña y S1 emita el segundo factor de sesión con el mismo motor de códigos del ADR-10.

## Fuentes

Lámina «Decisiones Arquitectónicas»; PRD, Restricciones técnicas; HU Plan B, R3, R4 y reglas transversales 4 y 5; Anexo A de HU-37.1 y HU-37.2; DDD v3.2, D-8.
