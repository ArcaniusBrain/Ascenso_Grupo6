# ASCENSO — Verificaciones antes de programar y antes de entregar

## Regla del profesor — Clase 09, láminas 31 y 39

Debe existir un **test automatizado de arquitectura** que inspeccione las referencias de código: Domain no puede depender de ASP.NET, EF Core, Firebase, MongoDB, MassTransit ni de Application/Infrastructure; Application no puede depender de Infra. No basta con dibujar flechas.

### Prueba de referencia (adaptar `typeof` al servicio creado)

```csharp
[Fact]
public void Domain_No_Referencia_Tecnologia_Externa()
{
    var result = Types.InAssembly(typeof(Beca).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny("Ascenso.S4.Application", "Ascenso.S4.Infrastructure",
           "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore",
           "MassTransit", "MongoDB", "FirebaseAdmin")
        .GetResult();
    Assert.True(result.IsSuccessful);
}
```

**No es resultado de una prueba:** todavía no se ha implementado ni ejecutado código.

## Prueba funcional y de fallos — mínimo por caso

| Suite | Afirmación observable | Prioridad |
|---|---|---|
| `PresupuestoPrograma` | Disponible jamás negativo; cupo programa/ciclo no excedidos; monto igual al disponible aceptado | P0 |
| `BeneficiosPersona` | Nunca dos becas incompatibles ni suma superior al costo de matrícula | P0 |
| `Beca` | Pago no permitido durante estados excluidos; revocación impide pago tardío | P0 |
| `Oferta` | Una aceptación / expiración, nunca ambas; fecha límite UTC-4 inclusiva | P0 |
| D-3 | Oferta+BeneficiosPersona cambian en una misma transacción S4 | P0 |
| D-6 | Agregado+RegistroDecision+Outbox juntos o nada | P0 |
| Distribuido | S4 cae tras reserva S2 -> compensar/reconciliar sin liberar oferta real | P0 |
| Eventos | Publicación/reentrega duplica mensaje pero nunca efecto | P0 |
| Seguridad | OTP usado dos veces no autoriza; rol válido sin alcance no puede actuar | P0 |
| CQRS | MongoDB retrasado nunca autoriza fondos | P0 |
| Calendario | Cierre en día límite 23:59 UTC-4, fuera desde el siguiente minuto | P1 |
| Universidad | Lote doble no duplica reporte; otro tenant no puede escribir | P1 |
| Privacidad | Exportación para donante anonimizada / consentimiento verificado | P1 |
| Frontend | SignalR reconexión fuerza GET estado actual | P2 |
| ETL | Dry-run no escribe; carga repetida idempotente; conciliación | P1 |

Las pruebas unitarias del DDD se hacen sin infraestructura, las de integración usan PostgreSQL, RabbitMQ, MongoDB, Keycloak y emulator Firebase en contenedores cuando proceda. Documentar evidencia ejecutada, no atribuirla a este documento.
