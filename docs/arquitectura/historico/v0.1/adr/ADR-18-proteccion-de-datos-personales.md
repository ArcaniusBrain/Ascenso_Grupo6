# ADR-18 · Protección de datos personales

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

Los RNF de confidencialidad y de protección de datos personales aplican a la información socioeconómica y a los datos bancarios.

## Decisión

- Las cuentas bancarias se cifran en reposo y se muestran enmascaradas (solo los últimos dígitos), también en el historial (HU-20.3).
- Se registra quién consulta los datos de pago (HU-20.3).
- La información socioeconómica solo la ven los roles que evalúan o deciden, con máscara de campos en la respuesta (HU-37.1).
- Los correos y SMS llevan el contenido mínimo, sin datos sensibles. La única excepción es el motivo de una suspensión (R26).
- Todo lo que va a donantes, y toda exportación para quien no tenga derecho a datos individuales, pasa por una única capa de agregación y anonimización (HU-33.1 y HU-36.1).
- El texto libre de las consultas se guarda y se muestra como texto, sin interpretarlo (HU-32.1).
- Los secretos (credenciales de Firebase, de Keycloak y claves de cifrado) quedan fuera del repositorio. La gestión de las claves de cifrado queda pendiente de detalle (sección 6).

## Fuentes

RNF del PRD; R26; Anexo A de HU-20.3, HU-29.1, HU-32.1, HU-33.1, HU-36.1 y HU-37.1.
