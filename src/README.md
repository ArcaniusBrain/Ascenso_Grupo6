# Backend (.NET 10)

Vacío hasta que el profesor apruebe la partición en servicios (ADR-02). Estructura prevista (ADR-04):

```
src/
├── shared/
│   ├── Ascenso.SharedKernel/      Tipos base y value objects comunes (DDD, sección 5)
│   └── Ascenso.Contracts/         Eventos de integración versionados (Published Language)
├── gateway/                       API Gateway con YARP
└── services/
    ├── identidad/                 S1
    ├── programas/                 S2
    ├── postulacion/               S3
    ├── becas/                     S4
    ├── seguimiento/               S5
    └── comunicaciones/            S6
```

Cada servicio tiene cuatro proyectos (`Domain`, `Application`, `Infrastructure`, `Api`) y dos de pruebas (`UnitTests`, `IntegrationTests`). El dominio no referencia EF Core, MassTransit, ASP.NET Core ni Firebase.
