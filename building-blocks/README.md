# Building blocks

Código compartido por todos los servicios (arquitectura v1.3, §12). Todavía sin código.

| Proyecto | Contiene | Regla |
|---|---|---|
| `Ascenso.SharedKernel/` | Tipos base del DDD (`Entidad`, `RaizDeAgregado` con `Version`, `IEventoDeDominio`), IDs tipados y value objects comunes: `Dinero`, `Cedula`, `Contacto`, `VentanaFechas`, `FechaLimite`, `Promedio`, `Justificacion`, `ReferenciaArchivo`, `CodigoPeriodo` (DDD §5) | Sin reglas de un contexto concreto. Todo cambio se acuerda entre todos (Shared Kernel del Context Map). |
| `Ascenso.Contracts/` | Eventos de integración públicos y versionados (`OfertaEmitida.v1`…) y DTO compartidos | Nunca expone entidades del dominio ni datos sensibles. Cambios compatibles o nueva versión. |
