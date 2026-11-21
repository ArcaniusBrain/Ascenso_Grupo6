# ADR-20 · Notificaciones por canal (correo y SMS)

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

El correo es el canal mínimo y cada programa puede activar otros (clase del 21-sep). El Anexo A de HU-29.1 pide que las notificaciones se disparen por eventos y detrás de una interfaz de canal extensible.

## Decisión

- S6 es el único servicio que envía notificaciones de negocio, y lo hace al recibir eventos; ninguna HU llama directamente al envío. La única excepción es el OTP, que S1 envía directamente (ADR-10).
- Una interfaz de canal con dos implementaciones iniciales: correo (obligatorio) y SMS.
- Los canales activos se configuran en el programa (S2) y se replican a S6.
- RegistroEnvio guarda un envío por persona, hito y anticipación, para no duplicar avisos (HU-29.1 y HU-30.1).
- Los proveedores reales de correo y SMS quedan por definir; en desarrollo se usan un servidor de correo de pruebas y un SMS simulado.

## Fuentes

Anexo A de HU-29.1, HU-29.2, HU-30.1 y HU-31.1; DDD v3.2, sección 3b.
