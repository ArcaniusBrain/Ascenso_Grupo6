# Cambios respecto del borrador anterior del equipo

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice en [`README.md`](README.md).

El borrador «Datos_Importantes_Profesor.txt» interpretaba bien la lámina del profesor. Ningún punto se elimina por completo; se corrigen los que contradecían las HU (que mandan) y se eliminan algunas sub-ideas.

| Punto del borrador | Veredicto | Qué cambia | ADR |
|---|---|---|---|
| 1. Partición y gateway | Se mantiene con ajustes | Se agrega Catálogo Institucional (dentro de S2); Trazabilidad deja de considerarse un servicio; lista cerrada de llamadas síncronas; un usuario de base de datos por servicio. | 02, 03 |
| 2. Capas | Se mantiene y se completa | Se agregan SharedKernel, Contracts y proyectos de pruebas; «un caso de uso por HU» pasa a «un comando o consulta por operación». | 04 |
| 3. PostgreSQL y Firebase | Se mantiene y se precisa | También van a Firebase las exportaciones; se guarda la ruta interna, no una URL pública (se elimina «FileUrl»). | 05, 06 |
| 4. Máquinas de estado | Se corrige (crítico) | Transiciones alineadas con las HU (estado Inactiva y sus transiciones, No elegible, Retirada); se elimina la opción de saga; se elige Stateless. | 08 |
| 5. Identidad y OTP | Se corrige (crítico) | OTP de 15 minutos, 3 intentos y 1 hora de espera, por correo o SMS; tres operaciones más; segundo factor también para enlaces y evaluadores externos. | 09, 10 |
| 6. Auditoría | Se ajusta | La tabla se llama RegistroDecision y se alimenta de los eventos de dominio, junto con el Outbox; cubre más agregados. | 12 |
| 7. MassTransit y Hangfire | Se corrige (crítico) | Beca no renovada pasa a Inactiva; la lista de espera se sugiere y no se promueve; cinco procesos más; Outbox e idempotencia. | 13, 14 |
| 8. SignalR y concurrencia | Se mantiene y se amplía | Un solo canal de push; concurrencia optimista en todas las raíces con los nombres del lenguaje ubicuo. | 15, 16 |
| 9. Reportes e índices | Se corrige | Se eliminan las vistas materializadas que cruzaban schemas; base de lectura aparte alimentada por eventos; índices corregidos. | 07, 22 |
| 10. ETL | Se ajusta | Carga por servicio, con las fábricas del dominio, repetible y con ensayo previo; no hay una transacción única. | 21 |
| (no estaba) | Se agrega | OpenSpec, autorización fina, protección de datos, reloj único, integración con universidades, notificaciones, versiones y licencias. | 11, 17–20, 23, 24 |
