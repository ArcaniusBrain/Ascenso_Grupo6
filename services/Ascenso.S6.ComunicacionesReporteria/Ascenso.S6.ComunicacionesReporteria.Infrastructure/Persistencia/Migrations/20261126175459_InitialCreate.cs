using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "s6_comunicaciones");

            migrationBuilder.CreateTable(
                name: "consentimientos_donante",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    becario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    donante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aceptado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consentimientos_donante", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    ConsumerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.ConsumerName, x.EventId });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "proyeccion_aportes_impacto",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    donante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_donante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_programa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto_aportado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyeccion_aportes_impacto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proyeccion_becarios_impacto",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    becario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cedula = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    universidad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    carrera = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ingreso_familiar_sensible = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    composicion_familiar_sensible = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyeccion_becarios_impacto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proyeccion_desembolsos_impacto",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyeccion_desembolsos_impacto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proyeccion_periodos_calendario",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyeccion_periodos_calendario", x => x.codigo_periodo);
                });

            migrationBuilder.CreateTable(
                name: "proyeccion_reportes_academicos_impacto",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    becario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    promedio = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    materias_aprobadas = table.Column<int>(type: "integer", nullable: false),
                    materias_reprobadas = table.Column<int>(type: "integer", nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyeccion_reportes_academicos_impacto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registro_decisiones",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_decision = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tipo_sujeto = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sujeto_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    propietario_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_rol = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    actor_es_sistema = table.Column<bool>(type: "boolean", nullable: false),
                    ocurrido_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    justificacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    estado_anterior = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    estado_nuevo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    datos_json = table.Column<string>(type: "text", nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_decisiones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solicitudes_reporte_impacto",
                schema: "s6_comunicaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    donante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_donante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    codigo_periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    solicitado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol_solicitante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    programas_alcance_csv = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mensaje_estado = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    solicitado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completado_en_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    contenido_json = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes_reporte_impacto", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_consentimientos_donante_becario_donante_unico",
                schema: "s6_comunicaciones",
                table: "consentimientos_donante",
                columns: new[] { "becario_id", "donante_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_pendientes",
                schema: "s6_comunicaciones",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_proyeccion_aportes_donante_periodo",
                schema: "s6_comunicaciones",
                table: "proyeccion_aportes_impacto",
                columns: new[] { "donante_id", "codigo_periodo" });

            migrationBuilder.CreateIndex(
                name: "ix_proyeccion_becarios_programa",
                schema: "s6_comunicaciones",
                table: "proyeccion_becarios_impacto",
                column: "programa_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyeccion_desembolsos_programa_periodo",
                schema: "s6_comunicaciones",
                table: "proyeccion_desembolsos_impacto",
                columns: new[] { "programa_id", "codigo_periodo" });

            migrationBuilder.CreateIndex(
                name: "ix_proyeccion_academicos_programa_periodo",
                schema: "s6_comunicaciones",
                table: "proyeccion_reportes_academicos_impacto",
                columns: new[] { "programa_id", "codigo_periodo" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_correlacion",
                schema: "s6_comunicaciones",
                table: "registro_decisiones",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_fecha",
                schema: "s6_comunicaciones",
                table: "registro_decisiones",
                column: "ocurrido_en_utc");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_programa",
                schema: "s6_comunicaciones",
                table: "registro_decisiones",
                column: "programa_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_propietario",
                schema: "s6_comunicaciones",
                table: "registro_decisiones",
                column: "propietario_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_decisiones_sujeto",
                schema: "s6_comunicaciones",
                table: "registro_decisiones",
                columns: new[] { "tipo_sujeto", "sujeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_reporte_impacto_donante_periodo",
                schema: "s6_comunicaciones",
                table: "solicitudes_reporte_impacto",
                columns: new[] { "donante_id", "codigo_periodo" });

            // Endurecimiento append-only de registro_decisiones en PostgreSQL (RF-35, HU-35.1 Anexo A).
            migrationBuilder.Sql(
                Ascenso.Messaging.Outbox.RegistroDecisionSqlAppendOnly.GenerarSqlPostgreSql(
                    ComunicacionesDbContext.Schema));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consentimientos_donante",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "proyeccion_aportes_impacto",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "proyeccion_becarios_impacto",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "proyeccion_desembolsos_impacto",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "proyeccion_periodos_calendario",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "proyeccion_reportes_academicos_impacto",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "registro_decisiones",
                schema: "s6_comunicaciones");

            migrationBuilder.DropTable(
                name: "solicitudes_reporte_impacto",
                schema: "s6_comunicaciones");
        }
    }
}
