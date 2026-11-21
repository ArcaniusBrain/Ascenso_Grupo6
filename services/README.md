# Microservicios (backend .NET 10)

Estructura definida en la arquitectura v1.3, §12. Todavía no hay código: cada carpeta se crea con su primer cambio de OpenSpec.

| Carpeta | Servicio | Contextos del DDD | Agregados |
|---|---|---|---|
| `Ascenso.S1.Identidad/` | S1 Identidad y Acceso | Identidad y Acceso | Usuario, TipoUsuario, ClienteApiUniversidad (+ OTP) |
| `Ascenso.S2.CatalogoFondos/` | S2 Catálogo y Fondos | Catálogo Institucional; Programas y Fondos | CalendarioAcademico, UniversidadAliada, Programa, Donante, PresupuestoPrograma |
| `Ascenso.S3.PostulacionEvaluacion/` | S3 Postulación y Evaluación | Postulación; Evaluación y Selección | CicloPostulacion, Postulante, Postulacion, Comite, Evaluacion, ConflictoInteres |
| `Ascenso.S4.Becas/` | S4 Gestión de Becas | Gestión de Becas | Oferta, Beca, BeneficiosPersona, DatosPago, VentanaRenovacion |
| `Ascenso.S5.SeguimientoAcademico/` | S5 Seguimiento Académico | Seguimiento Académico | ReporteAcademico, PlazoReporte, EvaluacionCumplimiento |
| `Ascenso.S6.ComunicacionesReporteria/` | S6 Comunicaciones y Reportería | Comunicaciones; Reportería e Impacto | Consulta, RegistroEnvio, ConsentimientoDonante (+ proyecciones MongoDB y SignalR) |

Cada servicio tendrá:

```
Ascenso.S<n>.<Nombre>/
├── Ascenso.S<n>.<Nombre>.Api/              Controllers mínimos, autenticación, DTO, ProblemDetails, composición (DI)
├── Ascenso.S<n>.<Nombre>.Application/      Comandos, consultas, handlers, puertos de salida
├── Ascenso.S<n>.<Nombre>.Domain/           Agregados, VO, eventos, servicios de dominio, interfaces de repositorio
├── Ascenso.S<n>.<Nombre>.Infrastructure/   EF Core, Outbox/Inbox, adaptadores (Firebase, Keycloak, correo…), jobs
└── tests/
    ├── Unit/
    ├── Integration/
    └── Architecture/                        Reglas de dependencias como pruebas
```

**Referencias permitidas:** Api → Application; Api → Infrastructure solo en la composición; Infrastructure → Application y Domain; Application → Domain; Domain → `building-blocks/Ascenso.SharedKernel`. Domain no conoce frameworks. Ningún servicio lee el schema de otro.

Orden de construcción (v1.3 §17): base del monorepo y CI → S2 → S1 → S3 y S4 → Outbox/RabbitMQ y S6 → S5 → notificaciones, jobs, ETL y observabilidad → pruebas de aceptación.
