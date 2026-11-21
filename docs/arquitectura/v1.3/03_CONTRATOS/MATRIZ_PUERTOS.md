# Puertos y adaptadores — contrato de dependencias

| Puerto | Proyecto interior | Responsable exterior | Pruebas |
|---|---|---|---|
| `IPresupuestoProgramaRepository` | `S2.Domain` | `S2.Infrastructure` con EF Core/Npgsql | Concurrencia, saldo igual al disponible, atomicidad |
| `IAlmacenamientoArchivos` | `S3.Application`/`S4.Application` | Firebase Storage adapter | Errores/red, autorización, referencia privada |
| `ICanalNotificacion` | `S6.Application` | Correo y SMS adapters | Reintento, duplicación, datos mínimos |
| `IFuenteAcademica` | `S3.Application` / `S5.Application` (contratos independientes) | Adaptadores API, carga manual y simulada por servicio | S3 certifica promedio HU-04.1; S5 recibe reportes HU-24.3; traducción y procedencia C5 |
| `IValidadorOtp` | `S4.Application` | Cliente S1 | OTP por operación/recurso, replay, caídas |
| `IConsultaPanel` | `S6.Application` | MongoDB | Lag, resync, masking, idempotencia |
| `IRelojNegocio` | `Application` | `TimeProvider` servidor/falso | UTC-4 y cierre 23:59 |

El puerto **pertenece a quien lo necesita** (Clase 09, láminas 14–18). La implementación exterior realiza su contrato. Prohibido insertar `DbContext` en agregado o en controlador. Los repositorios DDD están en `Domain`, no son interfaces genéricas para tablas. En `Api` se registran las implementaciones mediante DI sin añadir decisiones de negocio.
