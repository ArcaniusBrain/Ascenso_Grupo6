# ADR-15 · Tiempo real: SignalR como único canal de push

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

El RNF de actualizaciones en tiempo real aplica a la mensajería y al panel de indicadores. El Anexo A de HU-32.2 pide un único mecanismo de push compartido por la mensajería, el panel y el reporte consolidado.

## Decisión

- Un solo hub de **ASP.NET Core SignalR** en S6. Reenvía al navegador los cambios que recibe por eventos: mensajes de consultas (RF-32), panel de indicadores (RF-40) y cambios de estado relevantes para cada usuario.
- Pasa por YARP con WebSockets y se autentica con el mismo token.
- Al reconectarse, el cliente vuelve a pedir el estado real al servidor, en lugar de seguir mostrando un valor viejo (HU-40.1).
- Firebase no se usa para tiempo real; así hay un solo mecanismo.
- Todo lo que no exige que la persona esté conectada se avisa por correo, el canal mínimo (clase del 21-sep).

## Fuentes

RNF de tiempo real; Anexo A de HU-32.2 y HU-40.1; destacados de la clase del 21-sep.
