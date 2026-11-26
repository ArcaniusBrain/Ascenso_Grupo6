# ASCENSO · Plataforma de becas y ayudas estudiantiles

Proyecto semestral de **Desarrollo de Software** (Escuela de Ingeniería Informática, UCAB) · Grupo 6.

ASCENSO gestiona el ciclo completo de becas de la Fundación Ascenso: postulación, evaluación por comité, otorgamiento, desembolsos, seguimiento académico, renovación, comunicaciones, reportería y auditoría.

> **Etapa actual: implementación activa en `.NET 10`.** Ya están en `main` la base del monorepo (`building-blocks/`, `deploy/`, CI), la trazabilidad inmutable (`RF-35`), **`S1` Identidad + 2FA + OTP** (`RF-37`) y **`S2` Calendario Académico** (`RF-38`). Consulta las **[Guías de Integración para el equipo](docs/guias-integracion/README.md)** para conectarte con lo ya construido.

## Documentación

| Documento | Versión | Dónde |
|---|---|---|
| **Guías de integración (qué hay en `main` y cómo usarlo)** | Vigente | [`docs/guias-integracion/README.md`](docs/guias-integracion/README.md) |
| **Guía de inicio del equipo (preparar tu máquina)** | — | [`docs/guia-inicio-equipo.md`](docs/guia-inicio-equipo.md) |
| PRD del cliente | 17-sep-2026 | [`docs/requisitos/prd.md`](docs/requisitos/prd.md) |
| Respuestas del cliente (C1–C5, R1–R44) | Rondas 1 a 5 | [`docs/requisitos/respuestas_cliente.md`](docs/requisitos/respuestas_cliente.md) |
| Historias de usuario · Plan B (73 HU, 714 escenarios) | Final, 14-oct-2026 | [`docs/requisitos/historias-de-usuario/`](docs/requisitos/historias-de-usuario/README.md) |
| Modelo de dominio (DDD) | **v3.3**, 24-nov-2026 | [`docs/dominio/DDD_v3.3.md`](docs/dominio/DDD_v3.3.md) *(v3.2 conservada como histórico)* |
| **Arquitectura de software** | **v1.3**, 20-nov-2026 | [`docs/arquitectura/`](docs/arquitectura/README.md) |
| Pendientes después de la v1.3 | — | [`docs/arquitectura/pendientes-v1.3.md`](docs/arquitectura/pendientes-v1.3.md) |
| Guía de OpenSpec del equipo | — | [`docs/guia-openspec.md`](docs/guia-openspec.md) |

**Precedencia entre documentos:** historias de usuario (con las decisiones del cliente, salvo R39, R40 y R44, donde manda la v1.3) → DDD → lámina «Decisiones Arquitectónicas» del profesor → arquitectura.

## Arquitectura en una línea

Seis microservicios .NET 10 con Clean Architecture + DDD detrás de un API Gateway YARP; frontend React + Next.js; PostgreSQL + EF Core para escribir (un schema por servicio) y MongoDB para las proyecciones de lectura; archivos en Firebase Storage; eventos con MassTransit 8 + RabbitMQ (Outbox e Inbox); Keycloak para identidad; Hangfire para procesos programados; SignalR para tiempo real. Detalle en [`docs/arquitectura/`](docs/arquitectura/README.md).

| Servicio | Contextos | Carpeta |
|---|---|---|
| S1 Identidad y Acceso | Identidad y Acceso (+ OTP) | [`services/Ascenso.S1.Identidad`](services/Ascenso.S1.Identidad/) |
| S2 Catálogo y Fondos | Catálogo Institucional; Programas y Fondos | [`services/Ascenso.S2.CatalogoFondos`](services/Ascenso.S2.CatalogoFondos/) |
| S3 Postulación y Evaluación | Postulación; Evaluación y Selección | [`services/Ascenso.S3.PostulacionEvaluacion`](services/Ascenso.S3.PostulacionEvaluacion/) |
| S4 Gestión de Becas | Gestión de Becas | [`services/Ascenso.S4.Becas`](services/Ascenso.S4.Becas/) |
| S5 Seguimiento Académico | Seguimiento Académico | [`services/Ascenso.S5.SeguimientoAcademico`](services/Ascenso.S5.SeguimientoAcademico/) |
| S6 Comunicaciones y Reportería | Comunicaciones; Reportería e Impacto | [`services/Ascenso.S6.ComunicacionesReporteria`](services/Ascenso.S6.ComunicacionesReporteria/) |

## Estructura del repositorio

Sigue la arquitectura v1.3, §12:

```
.
├── frontend/          React + Next.js
├── gateway/           API Gateway YARP (sin reglas de negocio)
├── services/          Ascenso.S1 … Ascenso.S6, cada uno con Api, Application, Domain, Infrastructure y tests
├── building-blocks/   Ascenso.SharedKernel y Ascenso.Contracts
├── contracts/         OpenAPI, eventos y permisos (fuente actual: docs/arquitectura/v1.3/03_CONTRATOS)
├── deploy/            docker-compose y configuración local, sin secretos
├── openspec/          Specs por capacidad y cambios (obligatorio)
├── docs/
│   ├── requisitos/    PRD, respuestas del cliente, historias de usuario
│   ├── dominio/       DDD v3.3 (vigente), v3.2 histórico, diagramas
│   └── arquitectura/  Arquitectura v1.3 (vigente) e histórico
├── tools/             Scripts de apoyo (OpenSpec, diagramas, ETL)
└── .github/           Validación automática, plantilla de pull request, responsables por carpeta
```

## Cómo trabajamos

Las reglas de ramas, commits y pull requests están en [`CONTRIBUTING.md`](CONTRIBUTING.md): nadie sube directo a `main`, todo entra por pull request con al menos una revisión, y todo trabajo empieza como un cambio de OpenSpec.

## Próximos pasos

1. **Cada integrante prepara su máquina** con la [guía de inicio](docs/guia-inicio-equipo.md) (Node.js 24 LTS o superior, .NET 10, Docker y `bash tools/openspec/configurar.sh`).
2. **Decisiones pendientes:** las del equipo y las preguntas al profesor de [`docs/arquitectura/pendientes-v1.3.md`](docs/arquitectura/pendientes-v1.3.md). En B1, B2 y B6 de la Ronda 5 el equipo mantiene lo que dice la v1.3.
3. **Correcciones del dominio** que la v1.3 deja abiertas: cambio [`corregir-ddd-v3-3`](openspec/changes/corregir-ddd-v3-3/proposal.md).
4. **Pruebas técnicas:** segundo factor por correo/SMS en Keycloak (ADR-09) y Firebase Storage desde .NET con el emulador (ADR-06).
5. **Primer cambio de código** (v1.3 §17, paso 1): monorepo, Shared Kernel, Contracts e integración continua de .NET. Después, S2 con sus pruebas de concurrencia.
