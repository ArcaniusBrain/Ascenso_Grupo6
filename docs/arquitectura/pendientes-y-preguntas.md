# Pendientes que no bloquean la arquitectura

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice en [`README.md`](README.md).

Ninguno de estos puntos cambia la forma de la arquitectura; afectan reglas o detalles dentro de un servicio.

| Tema | Tipo | Dónde impacta | Referencia |
|---|---|---|---|
| ¿Qué lleva a una postulación a Rechazada? | Cliente | Máquina de estados de Postulación (S3) | HU-10.2 |
| ¿Se puede revocar desde Otorgada o desde Renovada? | Cliente | Máquina de estados de Beca (S4) | HU-22.1 |
| Momento en que se fija el monto de una renovación | Cliente | Reserva de renovación (S4 y S2) | HU-12.2 |
| ¿La reserva de una oferta cubre un periodo o toda la duración? | Cliente | PresupuestoPrograma (S2) | HU-15.1 |
| Prioridad entre fuentes del reporte académico | Cliente | S5 | HU-24.2 |
| Mecanismo de autenticación de la API de universidades | Cliente | S1 y S5 | HU-24.3 |
| Consentimiento por donante y su retiro | Cliente | S6 | HU-33.2 |
| Segundo factor por correo o SMS en Keycloak | Prueba técnica | S1 | ADR-09 |
| Proveedores reales de correo y SMS | Equipo | S6 y S1 | ADR-20 |
| Plazo de vinculación de una reserva | Equipo | S2 | ADR-03 |
| Gestión de las claves de cifrado | Equipo | S4 | ADR-18 |
| Becas activas de la persona al postular: ¿réplica o consulta síncrona? | Equipo | S3 y S4 | ADR-02, HU-01.2 |
| Responsable de integración de S6 | Equipo | Organización | ADR-02 |
| Formato de las planillas históricas de la Fundación | Cliente | ETL | ADR-21 |
| Estado de la cuenta de Firebase ante el aviso de cierre de cuentas | Riesgo | Archivos | ADR-06 |

## Preguntas para el profesor

1. ¿Acepta agrupar los 12 Bounded Contexts en seis servicios, con Trazabilidad como componente compartido y no como servicio? (ADR-02)

2. ¿Acepta CQRS con una base de lectura PostgreSQL separada en lugar de una NoSQL? (ADR-07)

3. ¿Acepta la librería Stateless dentro del dominio como implementación de la máquina de estados, o espera el patrón State escrito a mano? (ADR-08)

4. ¿Acepta que la liberación de presupuesto tras una revocación llegue por evento, segundos después, y la excepción D-3 en la aceptación de una oferta? (ADR-03)

5. ¿Se puede usar el emulador de Firebase en desarrollo y la cuenta de la Fundación solo en la demostración? (ADR-06)

6. Si Keycloak no permite el segundo factor por correo o SMS sin una extensión, ¿acepta el plan B? (ADR-09)
