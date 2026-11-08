# ADR-10 · Código de un solo uso (OTP) para operaciones sensibles

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

El cliente exige un código de un solo uso antes de cualquier acción que mueva dinero o cambie el estado legal de una beca. El Anexo A de HU-20.1 pide un único servicio transversal, reutilizado por todas las operaciones sensibles.

## Decisión

- Un **servicio único de OTP en S1**. Los demás servicios le piden validar el código antes de cambiar su agregado; hasta que el código se valida, nada cambia (HU-25.2).
- **Reglas (R4):** el usuario elige el canal (correo o SMS); el código vale **15 minutos** y se usa una sola vez; tras **3 intentos fallidos** hay una espera de **1 hora**; se puede reenviar. Longitud de **6** **dígitos** (supuesto del equipo: las HU no la definen).
- **Vinculación (propuesta):** el código se emite para un usuario, un tipo de operación y un recurso concretos. Un código pedido para pagar un desembolso de una beca no sirve para revocar otra.
- Los códigos se guardan cifrados con una función de un solo sentido, nunca en texto plano.
- El segundo factor de inicio de sesión y el OTP por operación son flujos independientes, aunque compartan el envío (HU-37.2).
- Para que el código llegue en el momento, S1 lo envía directamente con el adaptador de canal; no pasa por la cola de envíos masivos de S6.

| Operación | Fuente |
|---|---|
| Confirmar un desembolso (registrar el pago) | RF-20, HU-20.1 |
| Resolver un pago fallido o devuelto | HU-20.2 |
| Registrar o modificar los datos de pago (también la primera vez) | R5, R6, HU-20.3 |
| Revocar una beca | RF-22, HU-22.1 |
| Aplicar una consecuencia por incumplimiento que revoca | HU-25.2 |

- **No llevan OTP:** reportes, exportaciones, consentimiento y el propio registro de decisiones (HU-33.1, HU-33.2, HU-34.1, HU-35.1 y HU-36.1).

## Corrige el borrador anterior

Vigencia de 5 minutos y envío solo por correo, que contradecían R4; además faltaban tres operaciones.

## Fuentes

PRD, Restricciones técnicas; R4 a R6; Anexo A de HU-20.1, HU-20.2, HU-20.3, HU-22.1, HU-25.2 y HU-37.2; DDD v3.2, flujo 16.4d.
