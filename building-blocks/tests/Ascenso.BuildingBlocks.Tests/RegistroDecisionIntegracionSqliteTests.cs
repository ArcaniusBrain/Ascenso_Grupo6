using Ascenso.Messaging.Outbox;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.Trazabilidad;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class RegistroDecisionIntegracionSqliteTests
{
    private sealed class BecaPrueba : RaizDeAgregado<Guid>
    {
        public string Estado { get; private set; } = "Activa";
        public int PagosConfirmados { get; private set; }

        private BecaPrueba()
        {
        }

        public BecaPrueba(Guid id) : base(id)
        {
        }

        public void CambiarEstado(string nuevoEstado)
        {
            Estado = nuevoEstado;
            IncrementarVersion();
        }

        public void ConfirmarDesembolso()
        {
            if (Estado != "Activa")
            {
                throw new ExcepcionDeDominio("No se puede confirmar un desembolso si la beca no está Activa.", "BECA_NO_ACTIVA");
            }

            PagosConfirmados++;
            IncrementarVersion();
        }
    }

    private sealed class TrazabilidadTestDbContext : DbContext
    {
        public DbSet<BecaPrueba> Becas => Set<BecaPrueba>();
        public DbSet<RegistroDecision> RegistroDecisiones => Set<RegistroDecision>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public TrazabilidadTestDbContext(DbContextOptions<TrazabilidadTestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigurarOutboxEInbox();
            modelBuilder.ConfigurarRegistroDecisiones();

            modelBuilder.Entity<BecaPrueba>(builder =>
            {
                builder.ToTable("becas_prueba");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Estado).HasMaxLength(40).IsRequired();
                builder.ConfigurarVersionExplicita<BecaPrueba, Guid>();
            });
        }
    }

    private sealed class RelojFijo(DateTimeOffset ahoraUtc) : IRelojNegocio
    {
        public DateTimeOffset AhoraUtc => ahoraUtc;
        public DateTimeOffset AhoraCaracas => TimeZoneInfo.ConvertTime(ahoraUtc, TimeZoneInfo.CreateCustomTimeZone("VET", TimeSpan.FromHours(-4), "VET", "VET"));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }

    private static (SqliteConnection Conexion, DbContextOptions<TrazabilidadTestDbContext> Options) CrearBaseSqlite()
    {
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();

        var options = new DbContextOptionsBuilder<TrazabilidadTestDbContext>()
            .UseSqlite(conexion)
            .AddInterceptors(new AppendOnlyRegistroDecisionInterceptor())
            .Options;

        using var db = new TrazabilidadTestDbContext(options);
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw(RegistroDecisionSqlAppendOnly.SqlTriggersSqlite);

        return (conexion, options);
    }

    [Fact]
    public async Task HU35_1_Escenarios1y6_Confirmar_Atomicamente_Guarda_Agregado_Registro_Y_Outbox()
    {
        var (conexion, options) = CrearBaseSqlite();
        await using var _ = conexion;

        var becaId = Guid.NewGuid();
        var programaId = Guid.NewGuid();
        var carlosId = Guid.NewGuid();
        var lauraId = Guid.NewGuid();

        await using (var seed = new TrazabilidadTestDbContext(options))
        {
            seed.Becas.Add(new BecaPrueba(becaId));
            await seed.SaveChangesAsync();
        }

        // Escenario 1: Suspensión
        await using (var db1 = new TrazabilidadTestDbContext(options))
        {
            var beca = await db1.Becas.SingleAsync(x => x.Id == becaId);
            await UnidadAtomicaDecisiones.ConfirmarAsync(db1, _ =>
            {
                beca.CambiarEstado("Suspendida");
                return Task.FromResult(RegistroDecision.Crear(
                    TipoDecision.SUSPENSION,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
                    ActorDecision.Usuario(lauraId, "COORDINADOR"),
                    new DateTimeOffset(2026, 10, 5, 14, 0, 0, TimeSpan.FromHours(-4)),
                    "No entregó constancia de matrícula",
                    "S4",
                    Guid.NewGuid(),
                    programaId,
                    carlosId,
                    "Activa",
                    "Suspendida"));
            });
        }

        // Escenario 6: Nueva decisión (Reanudación) conserva intacta la anterior
        await using (var db2 = new TrazabilidadTestDbContext(options))
        {
            var beca = await db2.Becas.SingleAsync(x => x.Id == becaId);
            await UnidadAtomicaDecisiones.ConfirmarAsync(db2, _ =>
            {
                beca.CambiarEstado("Activa");
                return Task.FromResult(RegistroDecision.Crear(
                    TipoDecision.REANUDACION,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
                    ActorDecision.Usuario(lauraId, "COORDINADOR"),
                    new DateTimeOffset(2026, 10, 20, 14, 0, 0, TimeSpan.FromHours(-4)),
                    "Entregó constancia vigente",
                    "S4",
                    Guid.NewGuid(),
                    programaId,
                    carlosId,
                    "Suspendida",
                    "Activa"));
            });
        }

        await using var verificacion = new TrazabilidadTestDbContext(options);
        var becaFinal = await verificacion.Becas.AsNoTracking().SingleAsync(x => x.Id == becaId);
        var registros = await verificacion.RegistroDecisiones.AsNoTracking().ToListAsync();
        var outbox = await verificacion.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Equal("Activa", becaFinal.Estado);
        Assert.Equal(2, becaFinal.Version);
        Assert.Equal(2, registros.Count);
        Assert.Equal(2, outbox.Count);
        Assert.Contains(registros, r => r.Tipo == TipoDecision.SUSPENSION && r.Justificacion.Texto == "No entregó constancia de matrícula");
        Assert.Contains(registros, r => r.Tipo == TipoDecision.REANUDACION && r.Justificacion.Texto == "Entregó constancia vigente");
    }

    [Fact]
    public async Task HU35_1_Escenario3_Fallo_Al_Guardar_Registro_Revierte_Cambio_De_Estado_Y_Outbox()
    {
        var (conexion, options) = CrearBaseSqlite();
        await using var _ = conexion;

        var becaId = Guid.NewGuid();
        await using (var seed = new TrazabilidadTestDbContext(options))
        {
            seed.Becas.Add(new BecaPrueba(becaId));
            await seed.SaveChangesAsync();

            // Inyectar fallo real en la base de datos al insertar en registro_decisiones
            await seed.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER trg_simular_fallo_insert
                BEFORE INSERT ON registro_decisiones
                BEGIN
                    SELECT RAISE(ABORT, 'Fallo simulado de almacenamiento en registro_decisiones');
                END;
                """);
        }

        await using (var db = new TrazabilidadTestDbContext(options))
        {
            var beca = await db.Becas.SingleAsync(x => x.Id == becaId);

            var ex = await Assert.ThrowsAsync<ExcepcionPersistenciaDecision>(() =>
                UnidadAtomicaDecisiones.ConfirmarAsync(db, _ =>
                {
                    beca.CambiarEstado("Renovada");
                    return Task.FromResult(RegistroDecision.Crear(
                        TipoDecision.RENOVACION_APROBADA,
                        SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
                        ActorDecision.Usuario(Guid.NewGuid(), "COORDINADOR"),
                        DateTimeOffset.UtcNow,
                        "Cumple promedio y requisitos de renovación",
                        "S4",
                        Guid.NewGuid(),
                        estadoAnterior: "Activa",
                        estadoNuevo: "Renovada"));
                }));

            Assert.Equal("No se pudo registrar la decisión. No se aplicó ningún cambio.", ex.Message);
        }

        // Comprobar rollback atómico real en conexión fresca
        await using var verificacion = new TrazabilidadTestDbContext(options);
        var becaTrasFallo = await verificacion.Becas.AsNoTracking().SingleAsync(x => x.Id == becaId);
        Assert.Equal("Activa", becaTrasFallo.Estado);
        Assert.Equal(0, becaTrasFallo.Version);
        Assert.Equal(0, await verificacion.RegistroDecisiones.CountAsync());
        Assert.Equal(0, await verificacion.OutboxMessages.CountAsync());
    }

    [Fact]
    public async Task HU35_1_Escenarios4y5_Doble_Barrera_AppendOnly_Bloquea_Modificacion_Y_Eliminacion()
    {
        var (conexion, options) = CrearBaseSqlite();
        await using var _ = conexion;

        var becaId = Guid.NewGuid();
        var registroId = Guid.NewGuid();

        await using (var seed = new TrazabilidadTestDbContext(options))
        {
            seed.Becas.Add(new BecaPrueba(becaId));
            await seed.SaveChangesAsync();

            var beca = await seed.Becas.SingleAsync(x => x.Id == becaId);
            await UnidadAtomicaDecisiones.ConfirmarAsync(seed, _ =>
            {
                beca.CambiarEstado("Suspendida");
                return Task.FromResult(RegistroDecision.Crear(
                    TipoDecision.SUSPENSION,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
                    ActorDecision.Usuario(Guid.NewGuid(), "COORDINADOR"),
                    DateTimeOffset.UtcNow,
                    "No entregó constancia de matrícula",
                    "S4",
                    Guid.NewGuid(),
                    id: registroId));
            });
        }

        // Barrera 1: EF Core Interceptor rechaza Modified y Deleted
        await using (var dbEf = new TrazabilidadTestDbContext(options))
        {
            var registro = await dbEf.RegistroDecisiones.SingleAsync(x => x.Id == registroId);
            dbEf.Entry(registro).State = EntityState.Modified;

            var exUpdate = await Assert.ThrowsAsync<ExcepcionInmutabilidadRegistro>(() => dbEf.SaveChangesAsync());
            Assert.Equal("El registro de decisiones no se puede modificar ni eliminar.", exUpdate.Message);

            dbEf.ChangeTracker.Clear();

            var registroBorrar = await dbEf.RegistroDecisiones.SingleAsync(x => x.Id == registroId);
            dbEf.RegistroDecisiones.Remove(registroBorrar);

            var exDelete = await Assert.ThrowsAsync<ExcepcionInmutabilidadRegistro>(() => dbEf.SaveChangesAsync());
            Assert.Equal("El registro de decisiones no se puede modificar ni eliminar.", exDelete.Message);
        }

        // Barrera 2: SQL directo (saltándose EF Core) es bloqueado por los triggers BEFORE UPDATE / BEFORE DELETE
        await using (var dbSql = new TrazabilidadTestDbContext(options))
        {
            var exSqlUpdate = await Assert.ThrowsAsync<SqliteException>(() =>
                dbSql.Database.ExecuteSqlRawAsync(
                    "UPDATE registro_decisiones SET justificacion = 'Alterada' WHERE id = {0}", registroId));
            Assert.Contains("El registro de decisiones no se puede modificar ni eliminar.", exSqlUpdate.Message);

            var exSqlDelete = await Assert.ThrowsAsync<SqliteException>(() =>
                dbSql.Database.ExecuteSqlRawAsync(
                    "DELETE FROM registro_decisiones WHERE id = {0}", registroId));
            Assert.Contains("El registro de decisiones no se puede modificar ni eliminar.", exSqlDelete.Message);
        }

        // La fila sigue intacta
        await using var verificacion = new TrazabilidadTestDbContext(options);
        var intacto = await verificacion.RegistroDecisiones.AsNoTracking().SingleAsync(x => x.Id == registroId);
        Assert.Equal("No entregó constancia de matrícula", intacto.Justificacion.Texto);
    }

    [Fact]
    public async Task HU35_1_Escenario8_Dos_Decisiones_Simultaneas_Sobre_La_Misma_Beca_Sin_Registros_Huerfanos()
    {
        var (conexion, options) = CrearBaseSqlite();
        await using var _ = conexion;

        var becaId = Guid.NewGuid();
        var programaId = Guid.NewGuid();
        var carlosId = Guid.NewGuid();

        await using (var seed = new TrazabilidadTestDbContext(options))
        {
            seed.Becas.Add(new BecaPrueba(becaId));
            await seed.SaveChangesAsync();
        }

        // Dos transacciones cargan la misma versión (Version = 0) simultáneamente
        await using var ctxLaura = new TrazabilidadTestDbContext(options);
        await using var ctxFinanzas = new TrazabilidadTestDbContext(options);

        var becaParaLaura = await ctxLaura.Becas.SingleAsync(x => x.Id == becaId);
        var becaParaFinanzas = await ctxFinanzas.Becas.SingleAsync(x => x.Id == becaId);

        // Laura confirma primero la revocación (Version 0 -> 1)
        await UnidadAtomicaDecisiones.ConfirmarAsync(ctxLaura, _ =>
        {
            becaParaLaura.CambiarEstado("Revocada");
            return Task.FromResult(RegistroDecision.Crear(
                TipoDecision.REVOCACION,
                SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
                ActorDecision.Usuario(Guid.NewGuid(), "COORDINADOR"),
                DateTimeOffset.UtcNow,
                "Información falsa en el ingreso familiar declarado",
                "S4",
                Guid.NewGuid(),
                programaId,
                carlosId,
                "Activa",
                "Revocada"));
        });

        // Finanzas intenta confirmar un desembolso con la copia obsoleta (Version = 0) -> conflicto de concurrencia
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            UnidadAtomicaDecisiones.ConfirmarAsync(ctxFinanzas, _ =>
            {
                becaParaFinanzas.ConfirmarDesembolso();
                return Task.FromResult(RegistroDecision.Crear(
                    TipoDecision.PAGO_CONFIRMADO,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
                    ActorDecision.Usuario(Guid.NewGuid(), "FINANZAS"),
                    DateTimeOffset.UtcNow,
                    "Confirmación de desembolso del periodo",
                    "S4",
                    Guid.NewGuid(),
                    programaId,
                    carlosId));
            }));

        // Estado final coherente: exactamente 1 estado final (Revocada), 0 pagos, 1 registro, 1 outbox
        await using var verificacion = new TrazabilidadTestDbContext(options);
        var becaFinal = await verificacion.Becas.AsNoTracking().SingleAsync(x => x.Id == becaId);
        var registros = await verificacion.RegistroDecisiones.AsNoTracking().ToListAsync();
        var outbox = await verificacion.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Equal("Revocada", becaFinal.Estado);
        Assert.Equal(0, becaFinal.PagosConfirmados);
        Assert.Equal(1, becaFinal.Version);
        Assert.Single(registros);
        Assert.Equal(TipoDecision.REVOCACION, registros[0].Tipo);
        Assert.Single(outbox);
    }

    [Fact]
    public async Task HU35_2_Escenarios_Completos_Orden_Vacio_SoloLectura_Y_Alcances_Por_Rol()
    {
        var (conexion, options) = CrearBaseSqlite();
        await using var _ = conexion;

        var reloj = new RelojFijo(new DateTimeOffset(2026, 10, 25, 16, 0, 0, TimeSpan.Zero));
        var progIngenieria = Guid.NewGuid();
        var progSalud = Guid.NewGuid();
        var becaCarlosId = Guid.NewGuid();
        var becaSaludId = Guid.NewGuid();
        var postulacionJorgeId = Guid.NewGuid();
        var carlosUserId = Guid.NewGuid();
        var anaUserId = Guid.NewGuid();
        var lauraId = Guid.NewGuid();
        var finanzasId = Guid.NewGuid();
        var direccionId = Guid.NewGuid();

        await using (var seed = new TrazabilidadTestDbContext(options))
        {
            // Insertar deliberadamente fuera de orden cronológico para verificar el ordenamiento ascendente
            seed.RegistroDecisiones.AddRange(
                RegistroDecision.Crear(
                    TipoDecision.REANUDACION,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaCarlosId),
                    ActorDecision.Usuario(lauraId, "COORDINADOR"),
                    new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.FromHours(-4)),
                    "Consignó constancia de matrícula",
                    "S4",
                    Guid.NewGuid(),
                    progIngenieria,
                    carlosUserId,
                    "Suspendida",
                    "Activa"),
                RegistroDecision.Crear(
                    TipoDecision.SUSPENSION,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaCarlosId),
                    ActorDecision.Usuario(lauraId, "COORDINADOR"),
                    new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.FromHours(-4)),
                    "No entregó constancia de matrícula",
                    "S4",
                    Guid.NewGuid(),
                    progIngenieria,
                    carlosUserId,
                    "Activa",
                    "Suspendida"),
                RegistroDecision.Crear(
                    TipoDecision.PAGO_CONFIRMADO,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaCarlosId),
                    ActorDecision.Usuario(finanzasId, "FINANZAS"),
                    new DateTimeOffset(2026, 10, 22, 12, 0, 0, TimeSpan.FromHours(-4)),
                    "Transferencia ejecutada al becario",
                    "S4",
                    Guid.NewGuid(),
                    progIngenieria,
                    carlosUserId),
                RegistroDecision.Crear(
                    TipoDecision.SUSPENSION,
                    SujetoRef.DesdeGuid(TipoSujeto.BECA, becaSaludId),
                    ActorDecision.Usuario(Guid.NewGuid(), "COORDINADOR"),
                    new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.FromHours(-4)),
                    "Motivo interno Becas Salud",
                    "S4",
                    Guid.NewGuid(),
                    progSalud,
                    anaUserId));

            await seed.SaveChangesAsync();
        }

        await using var lecturaDb = new TrazabilidadTestDbContext(options);
        var consultor = new ConsultorRegistroDecisionesEfCore(lecturaDb, reloj);

        var direccion = new ContextoSolicitanteDecision(direccionId, PerfilAccesoDecision.DIRECCION);
        var laura = new ContextoSolicitanteDecision(
            lauraId,
            PerfilAccesoDecision.COORDINADOR,
            TienePoderDeDecisionEnCaso: true,
            ProgramasAsignados: new HashSet<Guid> { progIngenieria });
        var finanzas = new ContextoSolicitanteDecision(finanzasId, PerfilAccesoDecision.FINANZAS);
        var carlos = new ContextoSolicitanteDecision(carlosUserId, PerfilAccesoDecision.POSTULANTE_O_BECARIO);
        var ana = new ContextoSolicitanteDecision(anaUserId, PerfilAccesoDecision.POSTULANTE_O_BECARIO);

        var sujetoBecaCarlos = SujetoRef.DesdeGuid(TipoSujeto.BECA, becaCarlosId);

        // HU-35.2 Escenario 1: Dirección ve decisiones ordenadas cronológicamente (05-oct -> 20-oct -> 22-oct)
        var resDireccion = await consultor.ConsultarPorSujetoAsync(sujetoBecaCarlos, direccion);
        Assert.Equal(3, resDireccion.Total);
        Assert.Equal(TipoDecision.SUSPENSION, resDireccion.Items[0].Tipo);
        Assert.Equal(TipoDecision.REANUDACION, resDireccion.Items[1].Tipo);
        Assert.Equal(TipoDecision.PAGO_CONFIRMADO, resDireccion.Items[2].Tipo);
        Assert.Null(resDireccion.MensajeVacio);

        // HU-35.2 Escenario 2: Caso sin decisiones devuelve mensaje exacto
        var resVacio = await consultor.ConsultarPorSujetoAsync(
            SujetoRef.DesdeGuid(TipoSujeto.POSTULACION, postulacionJorgeId),
            direccion);
        Assert.Empty(resVacio.Items);
        Assert.Equal(0, resVacio.Total);
        Assert.Equal("Este caso aún no tiene decisiones registradas.", resVacio.MensajeVacio);

        // HU-35.2 Escenario 3: Consultar no modifica ni rastrea el registro
        Assert.Empty(lecturaDb.ChangeTracker.Entries());
        Assert.Equal(4, await lecturaDb.RegistroDecisiones.CountAsync());

        // HU-35.2 Escenario 4: Becario ajeno (Ana) no puede ver el registro de Carlos
        await Assert.ThrowsAsync<ExcepcionAccesoDenegadoDecision>(() =>
            consultor.ConsultarPorSujetoAsync(sujetoBecaCarlos, ana));

        // HU-35.2 (Cierre pregunta #2): Carlos sí consulta el registro completo de su propio caso
        var resCarlos = await consultor.ConsultarPorSujetoAsync(sujetoBecaCarlos, carlos);
        Assert.Equal(3, resCarlos.Total);
        Assert.Equal("No entregó constancia de matrícula", resCarlos.Items[0].Justificacion);

        // HU-35.2 (Cierre pregunta #1): Coordinador ve su programa (Ingeniería) y es denegado en otro (Salud)
        var resLauraIng = await consultor.ConsultarPorSujetoAsync(sujetoBecaCarlos, laura);
        Assert.Equal(3, resLauraIng.Total);

        await Assert.ThrowsAsync<ExcepcionAccesoDenegadoDecision>(() =>
            consultor.ConsultarPorSujetoAsync(SujetoRef.DesdeGuid(TipoSujeto.BECA, becaSaludId), laura));

        // HU-35.2 (Cierre Finanzas Opción 1): Finanzas solo ve decisiones de ámbito financiero
        var resFinanzas = await consultor.ConsultarPorSujetoAsync(sujetoBecaCarlos, finanzas);
        Assert.Single(resFinanzas.Items);
        Assert.Equal(TipoDecision.PAGO_CONFIRMADO, resFinanzas.Items[0].Tipo);
        Assert.DoesNotContain(resFinanzas.Items, x => x.Tipo == TipoDecision.SUSPENSION);
    }
}
