using Ascenso.SharedKernel.Trazabilidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ascenso.Messaging.Outbox;

/// <summary>
/// Interceptor EF Core que impide modificar o eliminar cualquier fila de RegistroDecision (RF-35, HU-35.1, ADR-12).
/// Opera como defensa en aplicación complementaria a los permisos REVOKE y triggers BEFORE UPDATE/DELETE en BD.
/// </summary>
public sealed class AppendOnlyRegistroDecisionInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidarInmutabilidad(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ValidarInmutabilidad(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ValidarInmutabilidad(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is RegistroDecision or SujetoRef or ActorDecision
                && entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new ExcepcionInmutabilidadRegistro();
            }
        }
    }
}
