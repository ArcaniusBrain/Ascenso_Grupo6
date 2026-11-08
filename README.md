# ASCENSO · Plataforma de becas y ayudas estudiantiles

Proyecto semestral de **Desarrollo de Software** (Escuela de Ingeniería Informática, UCAB) · Grupo 6.

ASCENSO gestiona el ciclo completo de becas de la Fundación Ascenso: postulación, evaluación por comité, otorgamiento, desembolsos, seguimiento académico, renovación, comunicaciones, reportería y auditoría.

> **Etapa actual: diseño.** El repositorio contiene los requisitos, el modelo de dominio y la propuesta de arquitectura. El código empieza cuando el profesor revise la arquitectura (ver [Próximos pasos](#próximos-pasos)).

## Documentación

| Documento | Versión | Dónde |
|---|---|---|
| PRD del cliente | 17-sep-2026 | [`docs/requisitos/prd.md`](docs/requisitos/prd.md) |
| Respuestas del cliente (C1–C5, R1–R30) | Rondas 1 a 4 | [`docs/requisitos/respuestas_cliente.md`](docs/requisitos/respuestas_cliente.md) |
| Historias de usuario · Plan B (73 HU, 714 escenarios) | Final, 14-oct-2026 | [`docs/requisitos/historias-de-usuario/`](docs/requisitos/historias-de-usuario/README.md) |
| Modelo de dominio (DDD) | v3.2, 4-nov-2026 | [`docs/dominio/DDD_v3.2.md`](docs/dominio/DDD_v3.2.md) |
| Diagramas del modelo (44) | v3.2 | [`docs/dominio/diagramas/`](docs/dominio/diagramas/README.md) |
| Propuesta de arquitectura (ADR-01 a ADR-24) | Borrador v0.1, 4-nov-2026 | [`docs/arquitectura/`](docs/arquitectura/README.md) |
| Cambios previstos para DDD v3.3 y ADR v0.2 | 7-nov-2026 | [`docs/cambios/`](docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md) |

**Precedencia entre documentos:** cuando dos fuentes difieren, mandan las **historias de usuario** (con las decisiones del cliente); luego el **DDD**; luego la **arquitectura**.

## Arquitectura en una línea

Seis microservicios en .NET 10 alineados con los Bounded Contexts del DDD, detrás de un API Gateway (YARP), con frontend en React + Next.js, PostgreSQL (un schema por servicio) con EF Core, archivos en Firebase Storage, eventos con MassTransit 8 sobre RabbitMQ (con Outbox), Keycloak para identidad y una base de lectura separada para reportes. Detalle en [`docs/arquitectura/`](docs/arquitectura/README.md).

| Servicio | Contextos | Carpeta (cuando exista código) |
|---|---|---|
| S1 Identidad y Acceso | Identidad y Acceso (+ OTP) | `src/services/identidad` |
| S2 Catálogo y Programas | Catálogo Institucional; Programas y Fondos | `src/services/programas` |
| S3 Postulación y Evaluación | Postulación; Evaluación y Selección | `src/services/postulacion` |
| S4 Gestión de Becas | Gestión de Becas | `src/services/becas` |
| S5 Seguimiento Académico | Seguimiento Académico | `src/services/seguimiento` |
| S6 Comunicaciones y Reportería | Comunicaciones; Reportería e Impacto | `src/services/comunicaciones` |

La partición está pendiente de aprobación del profesor (ADR-02); por eso `src/` todavía está vacío.

## Estructura del repositorio

```
.
├── docs/
│   ├── requisitos/        PRD, respuestas del cliente, historias de usuario (md + PDF)
│   ├── dominio/           DDD (md), diagramas Mermaid y enlaces a mermaid.live
│   ├── arquitectura/      README, un archivo por ADR, figuras y PDF original
│   └── cambios/           Cambios previstos para las próximas versiones
├── openspec/              (se crea con `openspec init`, ver Próximos pasos)
├── src/                   Backend .NET: servicios, SharedKernel y Contracts
├── frontend/              Aplicación Next.js
├── deploy/                docker-compose y configuración local
├── tools/                 Scripts de apoyo (diagramas, ETL…)
└── .github/               Plantilla de pull request y responsables por carpeta
```

## Cómo trabajamos

Las reglas de ramas, commits y pull requests están en [`CONTRIBUTING.md`](CONTRIBUTING.md). En resumen: nadie sube directo a `main`; todo entra por pull request con al menos una revisión, y cada commit cita la HU o el ADR que trabaja.

## Próximos pasos

1. **Proteger `main`** (Settings → Branches): pull request obligatorio con una aprobación.
2. **Agregar a los integrantes** como colaboradores y completar [`.github/CODEOWNERS`](.github/CODEOWNERS) con sus usuarios.
3. **Inicializar OpenSpec** (obligatorio, ADR-23). Requiere Node.js 20 o superior:
   ```bash
   npm install -g @fission-ai/openspec@latest
   openspec init
   ```
4. **Primera propuesta de OpenSpec:** aplicar [`docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`](docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md) para producir el DDD v3.3 y el ADR v0.2.
5. **Pruebas técnicas** antes de cerrar la arquitectura: segundo factor por correo/SMS en Keycloak (ADR-09) y enlaces firmados contra el emulador de Firebase (ADR-06).
6. **Después de la revisión del profesor:** crear la solución .NET en `src/`, el `docker-compose` en `deploy/` y el proyecto Next.js en `frontend/`.
