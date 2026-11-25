namespace Ascenso.Messaging.Outbox;

/// <summary>
/// Scripts SQL reutilizables para endurecer la inmutabilidad append-only de la tabla registro_decisiones
/// directamente en el motor de base de datos (HU-35.1 Anexo A: inmutabilidad real a nivel de permisos de datos).
/// </summary>
public static class RegistroDecisionSqlAppendOnly
{
    /// <summary>
    /// Genera el SQL PostgreSQL para las migraciones iniciales de cada servicio (S1..S6):
    /// revoca UPDATE/DELETE al rol runtime del schema y crea el trigger BEFORE UPDATE OR DELETE.
    /// </summary>
    public static string GenerarSqlPostgreSql(string schemaServicio)
    {
        if (string.IsNullOrWhiteSpace(schemaServicio))
        {
            throw new ArgumentException("El nombre del schema es obligatorio.", nameof(schemaServicio));
        }

        var schema = schemaServicio.Trim();
        var rolRuntime = $"{schema}_app";

        return $"""
            REVOKE UPDATE, DELETE, TRUNCATE ON TABLE "{schema}"."registro_decisiones" FROM "{rolRuntime}";
            DROP TRIGGER IF EXISTS trg_registro_decisiones_append_only ON "{schema}"."registro_decisiones";
            CREATE TRIGGER trg_registro_decisiones_append_only
            BEFORE UPDATE OR DELETE ON "{schema}"."registro_decisiones"
            FOR EACH ROW
            EXECUTE FUNCTION public.impedir_modificacion_registro_decisiones();
            """;
    }

    /// <summary>
    /// Triggers SQLite equivalentes para pruebas de integración locales y en CI sin requerir Docker.
    /// </summary>
    public const string SqlTriggersSqlite = """
        CREATE TRIGGER IF NOT EXISTS trg_registro_decisiones_no_update
        BEFORE UPDATE ON registro_decisiones
        BEGIN
            SELECT RAISE(ABORT, 'El registro de decisiones no se puede modificar ni eliminar.');
        END;

        CREATE TRIGGER IF NOT EXISTS trg_registro_decisiones_no_delete
        BEFORE DELETE ON registro_decisiones
        BEGIN
            SELECT RAISE(ABORT, 'El registro de decisiones no se puede modificar ni eliminar.');
        END;
        """;
}
